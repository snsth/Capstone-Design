using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BR_PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Look")]
    public Transform cameraTransform;
    public float mouseSensitivity = 3f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Head Bob")]
    public float walkBobFreq = 1.8f;
    public float sprintBobFreq = 2.6f;
    public float walkBobAmplY = 0.055f;
    public float walkBobAmplX = 0.028f;
    public float sprintBobAmplY = 0.095f;
    public float sprintBobAmplX = 0.048f;
    public float bobSmoothing = 14f;
    public float tiltAmount = 1.8f;

    [Header("Ladder")]
    public float ladderSpeed = 2.5f;

    [Header("Swimming & Diving")]
    public float swimSpeed = 2.5f;
    public float maxAir = 15f;
    public float airDrainPerSecond = 1f;
    public float airRegenPerSecond = 5f;

    [Header("Interaction")]
    public float interactDistance = 2.5f;

    CharacterController cc;
    float pitch;
    float roll;
    Vector3 vertVel;
    Vector3 camDefaultLocalPos;
    float bobTimer;

    bool onLadder;
    int waterCount;
    int oceanCount;
    float waterSurfaceY;
    bool submerged;
    public bool IsSubmerged => submerged;
    float currentAir;
    float footstepTimer;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!GetComponent<Rigidbody>())
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (cameraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam != null) cameraTransform = cam.transform;
        }
        currentAir = maxAir;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Start()
    {
        if (cameraTransform != null)
            camDefaultLocalPos = cameraTransform.localPosition;
    }

    void Update()
    {
        bool inventoryOpen = BR_UIManager.Instance != null && BR_UIManager.Instance.InventoryOpen;
        if (!inventoryOpen)
        {
            Look();
            Move();
            HeadBob();
            Interact();
        }
        UpdateAirUI();
        BR_SoundManager.Instance?.SetNearWater(waterCount > 0);
    }

    void Look()
    {
        float mx = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float my = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        float lateral = Input.GetAxisRaw("Horizontal");

        transform.Rotate(0f, mx, 0f);
        pitch = Mathf.Clamp(pitch - my, minPitch, maxPitch);

        // Camera tilt when strafing
        float targetRoll = -lateral * tiltAmount;
        roll = Mathf.Lerp(roll, targetRoll, 10f * Time.deltaTime);

        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, roll);
    }

    void Move()
    {
        if (onLadder) { LadderMove(); return; }

        bool inWater = waterCount > 0;
        submerged = inWater && cameraTransform != null && cameraTransform.position.y < waterSurfaceY - 0.15f;

        if (inWater) SwimMove();
        else GroundMove();

        ManageAir(inWater);
    }

    void GroundMove()
    {
        if (cc.isGrounded && vertVel.y < 0f) vertVel.y = -2f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        bool sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Vector3 move = (transform.right * x + transform.forward * z).normalized;
        cc.Move(move * (sprint ? sprintSpeed : walkSpeed) * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && cc.isGrounded)
            vertVel.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        vertVel.y += gravity * Time.deltaTime;
        cc.Move(vertVel * Time.deltaTime);
    }

    void SwimMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = (transform.right * x + transform.forward * z).normalized * swimSpeed;

        bool inOcean = oceanCount > 0;
        if (!inOcean && Input.GetKey(KeyCode.Space)) move.y += swimSpeed;
        else if (Input.GetKey(KeyCode.LeftControl)) move.y -= swimSpeed;
        else if (!submerged) move.y -= 1f;

        cc.Move(move * Time.deltaTime);
        vertVel = Vector3.zero;
    }

    void LadderMove()
    {
        vertVel = Vector3.zero;
        float z = Input.GetAxisRaw("Vertical");
        cc.Move(Vector3.up * z * ladderSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump"))
        {
            onLadder = false;
            vertVel.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        // C key: step off ladder without jumping
        if (Input.GetKeyDown(KeyCode.C))
            onLadder = false;
    }

    void HeadBob()
    {
        if (cameraTransform == null || onLadder || waterCount > 0) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        bool isMoving = (x != 0f || z != 0f) && cc.isGrounded;
        bool isSprinting = isMoving && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));

        float freq = isSprinting ? sprintBobFreq : walkBobFreq;
        if (isMoving)
        {
            bobTimer += Time.deltaTime * freq;

            // 발소리: 한 스텝(반 bob 주기)마다 재생
            float stepInterval = isSprinting ? 1f / (sprintBobFreq * 2f) : 1f / (walkBobFreq * 2f);
            if (footstepTimer <= 0f)
            {
                BR_SoundManager.Instance?.PlayFootstep();
                footstepTimer = stepInterval;
            }
            footstepTimer -= Time.deltaTime;
        }
        else
        {
            footstepTimer = 0f;
        }

        float amplY = isMoving ? (isSprinting ? sprintBobAmplY : walkBobAmplY) : 0f;
        float amplX = isMoving ? (isSprinting ? sprintBobAmplX : walkBobAmplX) : 0f;

        float bobY = Mathf.Sin(bobTimer * Mathf.PI * 2f) * amplY;
        float bobX = Mathf.Cos(bobTimer * Mathf.PI) * amplX;

        Vector3 targetPos = camDefaultLocalPos + new Vector3(bobX, bobY, 0f);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetPos, bobSmoothing * Time.deltaTime);
    }

    void ManageAir(bool inWater)
    {
        if (submerged)
        {
            currentAir = Mathf.Max(0f, currentAir - airDrainPerSecond * Time.deltaTime);
            if (currentAir <= 0f)
            {
                if (oceanCount > 0)
                    Application.Quit();
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene(
                        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                return;
            }
        }
        else if (!inWater && currentAir < maxAir)
            currentAir = Mathf.Min(maxAir, currentAir + airRegenPerSecond * Time.deltaTime);
    }

    void UpdateAirUI()
    {
        BR_UIManager.Instance?.SetAir(currentAir / maxAir, submerged);
    }

    void Interact()
    {
        if (cameraTransform == null) return;

        string prompt = string.Empty;
        BR_WorldItem item = null;
        BR_Door door = null;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, interactDistance))
        {
            item = hit.collider.GetComponentInParent<BR_WorldItem>();
            if (item != null)
            {
                prompt = $"[E] Pick up {item.ItemName}";
            }
            else
            {
                door = hit.collider.GetComponentInParent<BR_Door>();
                if (door != null && !door.IsOpen)
                {
                    bool hasKey = BR_Inventory.Instance != null && BR_Inventory.Instance.Has(door.RequiredItemId);
                    prompt = hasKey ? "[E] Open door" : $"[Locked] Need {door.RequiredItemName}";
                }
            }
        }

        BR_UIManager.Instance?.SetPrompt(prompt);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (item != null) item.Pickup();
            else if (door != null) door.TryOpen();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<BR_WaterZone>(out var zone))
        {
            if (waterCount == 0) BR_SoundManager.Instance?.PlayWaterEntry();
            waterCount++;
            waterSurfaceY = zone.SurfaceY;
        }
        else if (other.TryGetComponent<BR_OceanZone>(out var ocean))
        {
            if (waterCount == 0) BR_SoundManager.Instance?.PlayWaterEntry();
            waterCount++;
            oceanCount++;
            waterSurfaceY = ocean.SurfaceY;
        }
        if (other.gameObject.name.StartsWith("Railing_sideWall"))
            onLadder = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<BR_WaterZone>(out _))
            waterCount = Mathf.Max(0, waterCount - 1);
        else if (other.TryGetComponent<BR_OceanZone>(out _))
        {
            waterCount = Mathf.Max(0, waterCount - 1);
            oceanCount = Mathf.Max(0, oceanCount - 1);
        }
        if (other.gameObject.name.StartsWith("Railing_sideWall"))
            onLadder = false;
    }
}
