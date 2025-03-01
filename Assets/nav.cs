using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class nav : MonoBehaviour
{
    public float thrust = 10f;
    public float rotationSpeed = 50f;   // Controls yaw sensitivity
    public float pitchSpeed = 30f;     // Controls pitch sensitivity
    public float largeAcceleration = 15f; // For large thrust changes
    public float smoothing = 0.1f;     // Smoothing factor for responsive controls
    public float resetDuration = 2f;   // Duration for smooth reset

    public float landingSpeed = 5f;       // Speed for smooth landing
    public float approachSpeed = 10f;    // Speed for approaching the target
    public float landingThreshold = 5f;  // Distance threshold to start landing
    public float surfaceOffset = 1.5f;   // Distance from the planet's surface
    public Transform targetPlanet;       // Reference to the planet's transform

    private Rigidbody rb;
    private bool isResetting = false;    // Flag for smooth reset
    private bool isLanding = false;     // Flag for landing sequence
    private bool isNavigating = false;  // Flag for navigation
    private bool manualLand = false;    // Flag for manual landing

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Enable interpolation for smoother movement
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Set initial Rigidbody parameters
        rb.mass = 50f;       // Adjust mass for stability
        rb.angularDrag = 5f; // Increase angular drag to dampen rotations
        rb.drag = 1f;        // Add linear drag for smoother stops
    }

    void Update()
    {
        // Get input from Quest Pro controllers
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        // Detect Button A for smooth reset
        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressedA) && isPressedA && !isResetting)
        {
            StartCoroutine(SmoothReset());
        }

        // Detect Button B for navigation and landing
        if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressedB) && isPressedB && !isLanding && !isNavigating)
        {
            manualLand = false;
            StartCoroutine(NavigateAndLand());
        }

        // Detect X Button to toggle manual landing
        if (leftHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressedX) && isPressedX)
        {
            manualLand = true;
        }

        // Keep manual controls active unless landing or navigating
        if (!isLanding && !isNavigating)
        {
            HandleManualControls(rightHand, leftHand);
        }
    }

    private void HandleManualControls(InputDevice rightHand, InputDevice leftHand)
    {
        // Right Joystick for forward/backward thrust and yaw rotation
        if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightJoystick) && !isResetting)
        {
            float smoothedThrust = Mathf.Lerp(0, rightJoystick.y * thrust, smoothing);
            rb.AddForce(transform.forward * smoothedThrust * Time.deltaTime);

            float smoothedYaw = Mathf.Lerp(0, rightJoystick.x * rotationSpeed, smoothing);
            rb.AddTorque(transform.up * smoothedYaw * Time.deltaTime);
        }

        // Left Joystick for pitch and roll control
        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftJoystick) && !isResetting)
        {
            float smoothedPitch = Mathf.Lerp(0, leftJoystick.y * pitchSpeed, smoothing);
            rb.AddTorque(transform.right * -smoothedPitch * Time.deltaTime);

            float smoothedRoll = Mathf.Lerp(0, leftJoystick.x * rotationSpeed, smoothing);
            rb.AddTorque(transform.forward * smoothedRoll * Time.deltaTime);
        }

        // Right Trigger for large acceleration forward
        if (rightHand.TryGetFeatureValue(CommonUsages.trigger, out float rightTriggerValue) && !isResetting)
        {
            float smoothedAcceleration = Mathf.Lerp(0, rightTriggerValue * largeAcceleration, smoothing);
            rb.AddForce(transform.forward * smoothedAcceleration * Time.deltaTime);
        }

        // Left Trigger for large deceleration
        if (leftHand.TryGetFeatureValue(CommonUsages.trigger, out float leftTriggerValue) && !isResetting)
        {
            float smoothedDeceleration = Mathf.Lerp(0, -leftTriggerValue * largeAcceleration, smoothing);
            rb.AddForce(transform.forward * smoothedDeceleration * Time.deltaTime);
        }
    }

    private IEnumerator NavigateAndLand()
    {
        isNavigating = true;

        // Step 1: Navigate to the landing threshold
        Vector3 directionToPlanet = (targetPlanet.position - transform.position).normalized;
        Vector3 landingPoint = targetPlanet.position + directionToPlanet * (targetPlanet.localScale.x / 2 + surfaceOffset);

        while (Vector3.Distance(transform.position, landingPoint) > landingThreshold)
        {
            Vector3 approachDirection = (landingPoint - transform.position).normalized;
            rb.velocity = approachDirection * approachSpeed;

            // Smoothly rotate toward the landing point
            Quaternion targetRotation = Quaternion.LookRotation(approachDirection, transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Check if manual landing was triggered
            if (manualLand)
            {
                rb.velocity = Vector3.zero;
                isNavigating = false;
                yield break; // Exit coroutine for manual control
            }

            yield return null;
        }

        rb.velocity = Vector3.zero;

        // Step 2: Auto landing sequence
        isLanding = true;
        while (Vector3.Distance(transform.position, landingPoint) > surfaceOffset)
        {
            Vector3 descentDirection = (landingPoint - transform.position).normalized;
            rb.velocity = descentDirection * landingSpeed;

            // Align to the planet's surface normal
            Vector3 surfaceNormal = (transform.position - targetPlanet.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.Cross(transform.right, surfaceNormal), surfaceNormal);
            float proximityFactor = Mathf.Clamp01(Vector3.Distance(transform.position, landingPoint) / surfaceOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, proximityFactor * Time.deltaTime * rotationSpeed);

            // Freeze motion when close to the surface
            if (Vector3.Distance(transform.position, landingPoint) <= surfaceOffset)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            yield return null;
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isLanding = false;
        isNavigating = false;
    }

    private IEnumerator SmoothReset()
    {
        isResetting = true;

        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.identity; // Reset to default rotation
        Vector3 initialVelocity = rb.velocity;
        Vector3 initialAngularVelocity = rb.angularVelocity;

        float elapsed = 0f;

        while (elapsed < resetDuration)
        {
            float t = elapsed / resetDuration;

            // Smoothly interpolate rotation and velocities to zero
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);
            rb.velocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
            rb.angularVelocity = Vector3.Lerp(initialAngularVelocity, Vector3.zero, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isResetting = false;
    }
}
