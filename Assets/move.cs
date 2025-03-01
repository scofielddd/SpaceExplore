using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

public class Move : MonoBehaviour
{
    public float thrust = 10f;
    public float rotationSpeed = 50f;
    public float pitchSpeed = 30f;
    public float largeAcceleration = 15f;
    public float smoothing = 0.1f;
    public float resetDuration = 2f;

    public float landingSpeed = 5f;
    public float approachSpeed = 10f;
    public float landingThreshold = 5f;
    public float surfaceOffset = 1.5f;
    public Transform targetPlanet;

    private Rigidbody rb;
    private bool isResetting = false;
    private float maxHeightInLandScene = 400f;

    private static Move instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        OVRManager ovrManager = FindObjectOfType<OVRManager>();
        if (ovrManager != null && ovrManager.transform.parent == null)
        {
            DontDestroyOnLoad(ovrManager.gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.mass = 0f;
        rb.angularDrag = 0.1f;
        rb.drag = 0.1f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Land")
        {
            rb.isKinematic = true;
            Invoke(nameof(AdjustInitialHeightInLandScene), 0.1f);
            rb.isKinematic = false;
        }
        else if (scene.name == "Universe")
        {
            Transform cameraTransform = GetComponentInChildren<Camera>()?.transform;
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.identity;
                Debug.Log($"Camera rotation reset: {cameraTransform.localRotation}");
            }
        }
    }

    void Update()
    {
        HandleSceneTransitions();

        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        HandleManualControls(rightHand, leftHand);
        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressedA) && isPressedA && !isResetting)
        {
            StartCoroutine(SmoothReset());
        }
    }

    private void HandleSceneTransitions()
    {

        

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Universe")
        {
            Debug.Log($"My current position is {Vector3.Distance(transform.position, targetPlanet.position)}, threshold is {landingThreshold}");
            if (Vector3.Distance(transform.position, targetPlanet.position) <= landingThreshold)
            {
                SceneManager.LoadScene("Land");
            }
        }
        else if (currentScene == "Land")
        {
            Debug.Log($"y position: {transform.position.y}");

            if (transform.position.y >= maxHeightInLandScene)
            {
               


                SceneManager.LoadScene("Universe");
            }
        }
    }

    private void AdjustInitialHeightInLandScene()
    {
        rb.MovePosition(new Vector3(-1f, 350f, 0f));

        Debug.Log($"Adjusted position: {transform.position}");
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