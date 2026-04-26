using UnityEngine;
public class PlayerMovement : MonoBehaviour
{

    public CharacterController controller;

    public float maxSpeed = 1.75f;
    public float sprintSpeed = 4.8f;
    public float crouchSpeed = 1.05f;
    public float acceleration = 10f;
    public float deceleration = 12f;
    public float gravity = -15f;

    public float standingHeight = 1.8f;
    public float crouchHeight = 1.25f;
    public float crouchTransitionSpeed = 12f;
    public LayerMask standUpBlockLayers = ~0;
    public float standUpCheckSkin = 0.02f;

    public float maxStamina = 10f;
    public float staminaDrainPerSecond = 1f;
    public float staminaRegenPerSecond = 0.8f;
    public float staminaRegenDelay = 1f;
    public float staminaRegenIdleMultiplier = 2.2f;
    public float staminaRegenCrouchMultiplier = 1.6f;
    public float staminaRegenSlowMoveMultiplier = 1.25f;
    public bool showStaminaDebug = true;

    public float CurrentHorizontalSpeed => new Vector3(horizontalVelocity.x, 0f, horizontalVelocity.z).magnitude;
    public bool IsGrounded => isGrounded;
    public float CurrentStamina => currentStamina;
    public bool IsSprinting => isSprinting;
    public bool IsCrouching => isCrouching;

    Vector3 velocity;
    Vector3 horizontalVelocity;

    bool isGrounded;
    bool isSprinting;
    bool isCrouching;
    float currentStamina;
    float staminaRegenTimer;
    Vector3 initialControllerCenter;
    float initialControllerBottom;

    void OnValidate()
    {
        if (sprintSpeed <= maxSpeed)
        {
            sprintSpeed = maxSpeed + 0.5f;
        }

        if (crouchHeight >= standingHeight)
        {
            crouchHeight = Mathf.Max(0.8f, standingHeight - 0.2f);
        }
    }

    void Start()
    {
        currentStamina = maxStamina;

        if (controller != null)
        {
            standingHeight = controller.height;
            initialControllerCenter = controller.center;
            initialControllerBottom = controller.center.y - (controller.height * 0.5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(x, 0f, z);
        input = Vector3.ClampMagnitude(input, 1f);

        bool isMoving = input.sqrMagnitude > 0.001f;
        bool crouchHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.C);

        if (crouchHeld)
        {
            isCrouching = true;
        }
        else if (isCrouching)
        {
            isCrouching = !CanStandUp();
        }

        bool sprintHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool canSprint = currentStamina > 0f;
        isSprinting = sprintHeld && isMoving && canSprint && !isCrouching;

        if (isSprinting)
        {
            currentStamina -= staminaDrainPerSecond * Time.deltaTime;
            currentStamina = Mathf.Max(0f, currentStamina);
            staminaRegenTimer = staminaRegenDelay;
        }
        else
        {
            if (staminaRegenTimer > 0f)
            {
                staminaRegenTimer -= Time.deltaTime;
            }
            else
            {
                float regenMultiplier;
                if (!isMoving)
                {
                    regenMultiplier = staminaRegenIdleMultiplier;
                }
                else if (isCrouching)
                {
                    regenMultiplier = staminaRegenCrouchMultiplier;
                }
                else
                {
                    float speedRatio = Mathf.Clamp01(CurrentHorizontalSpeed / Mathf.Max(0.01f, maxSpeed));
                    regenMultiplier = Mathf.Lerp(staminaRegenSlowMoveMultiplier, 1f, speedRatio);
                }

                currentStamina += staminaRegenPerSecond * regenMultiplier * Time.deltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }
        }

        float effectiveSprintSpeed = Mathf.Max(sprintSpeed, maxSpeed + 0.5f);
        float targetSpeed = isCrouching ? crouchSpeed : (isSprinting ? effectiveSprintSpeed : maxSpeed);
        Vector3 targetMove = (transform.right * input.x + transform.forward * input.z) * targetSpeed;
        float moveRate = isMoving ? (isSprinting ? acceleration * 1.5f : acceleration) : deceleration;
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetMove, moveRate * Time.deltaTime);

        if (controller != null)
        {
            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);

            Vector3 center = controller.center;
            center.x = initialControllerCenter.x;
            center.z = initialControllerCenter.z;
            center.y = initialControllerBottom + (controller.height * 0.5f);
            controller.center = center;
        }

        controller.Move(horizontalVelocity * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }

    bool CanStandUp()
    {
        if (controller == null)
        {
            return true;
        }

        Vector3 standingCenterLocal = new Vector3(
            initialControllerCenter.x,
            initialControllerBottom + (standingHeight * 0.5f),
            initialControllerCenter.z
        );

        Vector3 standingCenterWorld = transform.TransformPoint(standingCenterLocal);
        float radius = Mathf.Max(0.01f, controller.radius - standUpCheckSkin);
        float halfHeight = Mathf.Max(radius, (standingHeight * 0.5f) - standUpCheckSkin);

        Vector3 bottom = standingCenterWorld - transform.up * (halfHeight - radius);
        Vector3 top = standingCenterWorld + transform.up * (halfHeight - radius);

        Collider[] hits = Physics.OverlapCapsule(bottom, top, radius, standUpBlockLayers, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    void OnGUI()
    {
        if (!showStaminaDebug)
        {
            return;
        }

        string staminaState;
        if (isSprinting)
        {
            staminaState = "Draining";
        }
        else if (currentStamina >= maxStamina - 0.01f)
        {
            staminaState = "Full";
        }
        else if (staminaRegenTimer > 0f)
        {
            staminaState = "Regen Delay";
        }
        else
        {
            staminaState = "Regenerating";
        }

        Color stateColor;
        if (isSprinting)
        {
            stateColor = new Color(1f, 0.35f, 0.35f);
        }
        else if (currentStamina >= maxStamina - 0.01f)
        {
            stateColor = new Color(0.35f, 1f, 0.45f);
        }
        else if (staminaRegenTimer > 0f)
        {
            stateColor = new Color(1f, 0.75f, 0.25f);
        }
        else
        {
            stateColor = new Color(0.45f, 0.9f, 1f);
        }

        Color staminaColor = Color.Lerp(new Color(1f, 0.3f, 0.3f), new Color(0.35f, 1f, 0.45f), Mathf.Clamp01(currentStamina / Mathf.Max(0.01f, maxStamina)));
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        GUIStyle valueStyle = new GUIStyle(GUI.skin.label);
        valueStyle.normal.textColor = staminaColor;
        GUIStyle stateStyle = new GUIStyle(GUI.skin.label);
        stateStyle.normal.textColor = stateColor;

        GUI.Box(new Rect(12f, 12f, 280f, 70f), "Stamina Debug");
        GUI.Label(new Rect(22f, 36f, 70f, 20f), "Stamina:", labelStyle);
        GUI.Label(new Rect(92f, 36f, 190f, 20f), $"{currentStamina:0.0} / {maxStamina:0.0}", valueStyle);
        GUI.Label(new Rect(22f, 56f, 45f, 20f), "State:", labelStyle);
        GUI.Label(new Rect(67f, 56f, 210f, 20f), staminaState, stateStyle);
    }
}

