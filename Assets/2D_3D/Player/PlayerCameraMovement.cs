using UnityEngine;
public class PlayerCameraMovement : MonoBehaviour
{
    public Transform playerTransform;
    public Transform cameraTransform;
    public PlayerMovement playerMovement;

    public float mouseSensitivity = 5.5f;
    public float lookSmoothing = 0.03f;
    public float minPitch = -75f;
    public float maxPitch = 75f;
    public bool lockCursorOnStart = true;

    public float headBobFrequency = 1.6f;
    public float sprintBobFrequencyMultiplier = 1.35f;
    public float walkBobAmplitude = 0.05f;
    public float sprintBobAmplitude = 0.085f;
    public float movementSwayAmplitude = 0.004f;
    public float idleBreathAmplitude = 0.004f;
    public float idleDriftAmplitude = 0.006f;
    public float idleDriftFrequency = 0.35f;
    public float cameraTiltAmount = 0.35f;
    public float movementTiltAmount = 1.2f;
    public float cameraTiltSmooth = 10f;
    public float baseFov = 68f;
    public float sprintFov = 71f;
    public float fovLerpSpeed = 8f;
    public float crouchCameraDrop = 0.32f;
    public float bobSpeedForFullEffect = 5f;
    public float cameraPositionLerpSpeed = 16f;
    public float movementBlendLerpSpeed = 14f;
    public float bobVariationStrength = 0.35f;
    public float bobVariationFrequency = 2.15f;

    float pitch;
    float bobTimer;
    float currentCameraRoll;
    Vector2 currentLookDelta;
    Vector2 lookDeltaVelocity;
    Vector3 cameraDefaultLocalPos;
    Camera cachedCamera;
    float movementBlend;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (cameraTransform != null)
        {
            cameraDefaultLocalPos = cameraTransform.localPosition;
            cachedCamera = cameraTransform.GetComponent<Camera>();
            if (cachedCamera != null)
            {
                cachedCamera.fieldOfView = baseFov;
            }
        }

        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        UpdateLook();
        UpdateCameraEffects();
    }

    void UpdateLook()
    {
        Vector2 targetLookDelta = new Vector2(
            Input.GetAxisRaw("Mouse X") * mouseSensitivity,
            Input.GetAxisRaw("Mouse Y") * mouseSensitivity
        );

        currentLookDelta = Vector2.SmoothDamp(currentLookDelta, targetLookDelta, ref lookDeltaVelocity, lookSmoothing);
        float mouseX = currentLookDelta.x;
        float mouseY = currentLookDelta.y;

        if (playerTransform != null)
        {
            playerTransform.Rotate(Vector3.up * mouseX);
        }

        if (cameraTransform == null)
        {
            return;
        }

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        float lateralInput = Input.GetAxisRaw("Horizontal");
        float targetRoll = (-mouseX * cameraTiltAmount) + (-lateralInput * movementTiltAmount * movementBlend);
        currentCameraRoll = Mathf.Lerp(currentCameraRoll, targetRoll, cameraTiltSmooth * Time.deltaTime);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, currentCameraRoll);
    }

    void UpdateCameraEffects()
    {
        if (cameraTransform == null)
        {
            return;
        }

        float speed = playerMovement != null ? playerMovement.CurrentHorizontalSpeed : 0f;
        bool isSprinting = playerMovement != null && playerMovement.IsSprinting;

        float targetMovementBlend = Mathf.Clamp01(speed / Mathf.Max(0.01f, bobSpeedForFullEffect));
        movementBlend = Mathf.Lerp(movementBlend, targetMovementBlend, movementBlendLerpSpeed * Time.deltaTime);
        bool isMoving = movementBlend > 0.02f;

        if (isMoving)
        {
            float speedMultiplier = Mathf.Lerp(0.6f, isSprinting ? sprintBobFrequencyMultiplier : 1f, movementBlend);
            bobTimer += Time.deltaTime * headBobFrequency * speedMultiplier;
        }
        else
        {
            bobTimer += Time.deltaTime * (headBobFrequency * 0.35f);
        }

        float bobAmplitude = Mathf.Lerp(idleBreathAmplitude, isSprinting ? sprintBobAmplitude : walkBobAmplitude, movementBlend);
        float phase = bobTimer * Mathf.PI * 2f;
        float primaryBobY = Mathf.Sin(phase) * bobAmplitude;
        float secondaryBobY = Mathf.Sin(phase * bobVariationFrequency) * bobAmplitude * bobVariationStrength * movementBlend;
        float bobY = primaryBobY + secondaryBobY;
        float movingBobX = (Mathf.Cos(phase) + Mathf.Sin(phase * 1.5f) * 0.35f) * movementSwayAmplitude * movementBlend;
        float idleBobX = Mathf.Sin(Time.time * idleDriftFrequency * Mathf.PI * 2f) * idleDriftAmplitude;
        float bobX = Mathf.Lerp(idleBobX, movingBobX, movementBlend);

        float crouchOffset = (playerMovement != null && playerMovement.IsCrouching) ? crouchCameraDrop : 0f;

        Vector3 targetLocalPos = cameraDefaultLocalPos + new Vector3(bobX, bobY - crouchOffset, 0f);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetLocalPos, cameraPositionLerpSpeed * Time.deltaTime);

        if (cachedCamera != null)
        {
            float targetFov = isSprinting ? sprintFov : baseFov;
            cachedCamera.fieldOfView = Mathf.Lerp(cachedCamera.fieldOfView, targetFov, fovLerpSpeed * Time.deltaTime);
        }
    }
}

