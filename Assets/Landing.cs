using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Landing : MonoBehaviour
{
    public float thrust = 10f;
    public float rotationSpeed = 50f;
    public float pitchSpeed = 30f;
    public float largeAcceleration = 15f;
    public float smoothing = 0.1f;
    public float resetDuration = 2f;
    public float verticalSpeed = 5f; // Speed for vertical movement
    public Transform landmark; // Reference to the landmark object
    public float fadeDistance = 10f; // Distance range for fading effect

    public Transform atmTarget; // Drag the specific planet here
    private Material atm1Material; // The atmosphere material (auto-assigned from planetTarget)

    public GameObject firstPersonObject; // Assign the parent object for first-person view
    public GameObject thirdPersonObject; // Assign the parent object for third-person view
    public GameObject canvasFirstPerson; // Assign the first-person canvas
    public GameObject canvasThirdPerson; // Assign the third-person canvas

    private bool isThirdPerson = false; // Tracks the current view mode

    private Rigidbody rb;
    private bool isResetting = false;

    void Start()
    {
        //(before the first frame update)

        // Called once when the script starts running.
        // Used for setup, assigning values, and initializing components.
        
        if (atmTarget != null)
        {
            // Assigns the planet’s atmosphere material 
            AssignAtmosphereMaterial();
        }
        else
        {
            Debug.LogError("atmTarget target is not assigned!");
        }
        LogShaderProperties(atm1Material);

        // The spaceship needs this component for physics-based movement.
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // Adds a slight resistance to movement to make the spaceship more realistic.
        rb.drag = 0.1f;
        rb.angularDrag = 0.1f;
        //Determines first-person or third-person mode.
        UpdateViewMode(); // Set initial view mode
    }

    // (called every frame)
    // Runs once per frame (at 60 FPS, it runs 60 times per second).
    // Used for continuous updates like player input detection.
        void Update() 
    {
        // Gets VR Controller Input
        // have OVR Camera Rig in the hierarchy.
        // Unity's XR API (InputDevices.GetDeviceAtXRNode) automatically detects and assigns the correct controller at runtime
        // No need to manually reference them in the Inspector.
        // trackingSpace and OVR Interaction work seperately behind the scenes
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        // Reads joystick input and button presses to move and rotate the spaceship.
        HandleManualControls(rightHand, leftHand);
        
        // Moves the spaceship up/down based on A/B button presses.
        HandleVerticalMovement(rightHand);

        // Modifies the planet’s atmosphere transparency based on spaceship height.
        // fading effect
        AdjustAtm1Material();

        //Toggles first-person vs third-person mode using the Y button.
        HandleViewSwitch(leftHand); // Handle object switching with the left hand
    }

    private void HandleManualControls(InputDevice rightHand, InputDevice leftHand)
    {
        // Right Joystick for forward/backward thrust and yaw rotation
        // rightJoystick.y (Up/Down on joystick) → Moves the spaceship forward/backward.
        // rightJoystick.x (Left/Right on joystick) → Rotates the spaceship left/right.
        if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightJoystick) && !isResetting)
        {
            float smoothedThrust = Mathf.Lerp(0, rightJoystick.y * thrust, smoothing);
            rb.AddForce(transform.forward * smoothedThrust * Time.deltaTime);

            float smoothedYaw = Mathf.Lerp(0, rightJoystick.x * rotationSpeed, smoothing);
            rb.AddTorque(transform.up * smoothedYaw * Time.deltaTime);
        }

        // Left Joystick for pitch and roll control
        // leftJoystick.y(Up / Down on joystick) → Controls pitch (Tilts the spaceship nose up or down.)
        // leftJoystick.x (Left/Right on joystick) → Controls roll (Rotates the spaceship left or right, like a barrel roll.)
        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 leftJoystick) && !isResetting)
        {
            // using Mathf.Lerp() Gradually applies rotation for a smoother experience.
            float smoothedPitch = Mathf.Lerp(0, leftJoystick.y * pitchSpeed, smoothing);
            // Negative pitch (-smoothedPitch) flips the Y-axis for correct rotation.
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

    private void HandleVerticalMovement(InputDevice rightHand)
    {
        // Pressing "A" Button → Moves UP.
        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool isAPressed) && isAPressed)
        {
            rb.MovePosition(transform.position + Vector3.up * verticalSpeed * Time.deltaTime);
            Debug.Log(transform.position.y);
        }

        // Pressing "B" Button → Moves DOWN.
        if (rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isBPressed) && isBPressed)
        {
            rb.MovePosition(transform.position - Vector3.up * verticalSpeed * Time.deltaTime);
        }
    }

    private void HandleViewSwitch(InputDevice leftHand)
    {
        // Switch between first-person and third-person view using the "Y" button (Secondary Button on the left hand)
        // Pressing "Y" Button → Toggles between first-person and third-person views.
        if (leftHand.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isYPressed) && isYPressed)
        {
            isThirdPerson = !isThirdPerson;
            UpdateViewMode();
        }
    }

    private void UpdateViewMode()
    {
        if (isThirdPerson)
        {
            EnableView(thirdPersonObject, canvasThirdPerson);
            DisableView(firstPersonObject, canvasFirstPerson);

        }
        else
        {
            EnableView(firstPersonObject, canvasFirstPerson);
            DisableView(thirdPersonObject, canvasThirdPerson);
        }
    }


    private void EnableView(GameObject viewObject, GameObject viewCanvas)
    {
        var cameras = viewObject.GetComponentsInChildren<Camera>(true);
        foreach (var cam in cameras)
        {
            cam.enabled = true;
        }
        viewCanvas.SetActive(true);
    }

    private void DisableView(GameObject viewObject, GameObject viewCanvas)
    {
        var cameras = viewObject.GetComponentsInChildren<Camera>(true);
        foreach (var cam in cameras)
        {
            cam.enabled = false;
        }
        viewCanvas.SetActive(false);
    }

    private void AssignAtmosphereMaterial()
    {
        if (atmTarget != null)
        {
       
            Renderer atmosphereRenderer = atmTarget.GetComponentInChildren<Renderer>();
            if (atmosphereRenderer != null)
            {
                atm1Material = atmosphereRenderer.sharedMaterial;
                Debug.Log($"Assigned atmosphere shared material from {atmTarget.name} to spaceship.");
            }
            else
            {
                Debug.LogError($"No Renderer with atmosphere material found in {atmTarget.name}'s hierarchy!");
            }
        }
        else
        {
            Debug.LogError("atmTarget target is null! Cannot assign atmosphere material.");
        }
    }

    private void AdjustAtm1Material()
    {

        // When spaceship moves closer to referenceY (Y = 400m) → Atmosphere becomes denser.
        // When spaceship moves father to referenceY (Y = 400m) → Atmosphere fade out.(both side up / down)
        if (atm1Material != null)
        {
            float referenceY = 400f;
            float currentY = transform.position.y;
            float fadeAmount = Mathf.Clamp01((currentY - referenceY) / fadeDistance);

            if (atm1Material.HasProperty("_ExteriorIntensity"))
            {
                atm1Material.SetFloat("_ExteriorIntensity", fadeAmount);
                Debug.Log($"Updated ExteriorIntensity: {fadeAmount}");
            }
        }
        else
        {
            Debug.LogError("Atmosphere material is not assigned!");
        }
    }

    // Logs all properties in the shader (Unity Editor only).
    // Helps debug atmosphere fade settings.

    private void LogShaderProperties(Material material)
    {
        if (material == null)
        {
            Debug.LogError("Material is null!");
            return;
        }

        

#if UNITY_EDITOR
        Shader shader = material.shader;
        Debug.Log($"Shader: {shader.name}");
        int propertyCount = ShaderUtil.GetPropertyCount(shader);

        for (int i = 0; i < propertyCount; i++)
        {
            string propertyName = ShaderUtil.GetPropertyName(shader, i);
            Debug.Log($"Property {i}: {propertyName}");
        }
#else
        Debug.LogError("Shader property logging is only available in the Unity Editor.");
#endif
    }
}
