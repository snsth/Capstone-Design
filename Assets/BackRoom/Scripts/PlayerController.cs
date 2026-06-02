using UnityEngine;

public enum GameMode
{
    Ground,
    WaterSurface,
    Underwater
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(OxygenSystem))]
[RequireComponent(typeof(InventorySystem))]
[RequireComponent(typeof(InteractionSystem))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Ground Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Water Surface Movement")]
    public float swimSpeed = 3.5f;
    public float surfaceSnapStrength = 4f;

    [Header("Diving Movement")]
    public float diveHorizontalSpeed = 3f;
    public float diveVerticalSpeed = 2f;
    public float waterDrag = 5f;

    [Header("Water Detection")]
    public float divingThreshold = 0.8f;

    // 현재 게임 모드
    public GameMode CurrentMode { get; private set; } = GameMode.Ground;

    // 컴포넌트 참조
    CharacterController controller;
    OxygenSystem oxygenSystem;
    InventorySystem inventorySystem;
    InteractionSystem interactionSystem;
    HUDManager hudManager;

    // 물리 상태
    Vector3 groundVelocity;
    Vector3 diveVelocity;
    float pitchAngle;

    // 물 상태
    bool isInWater;
    float waterSurfaceY;

    void Awake()
    {
        if (Instance == null) Instance = this;

        controller = GetComponent<CharacterController>();
        oxygenSystem = GetComponent<OxygenSystem>();
        inventorySystem = GetComponent<InventorySystem>();
        interactionSystem = GetComponent<InteractionSystem>();
        hudManager = GetComponent<HUDManager>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 인벤토리 열려있으면 이동/카메라 입력 차단
        if (inventorySystem != null && inventorySystem.IsOpen) return;

        HandleCameraLook();
        UpdateWaterMode();
        HandleMovement();
    }

    // ──────────────────────────────
    //  카메라 마우스 룩
    // ──────────────────────────────
    void HandleCameraLook()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up, mouseX, Space.Self);

        pitchAngle = Mathf.Clamp(pitchAngle - mouseY, minPitch, maxPitch);
        cameraTransform.localRotation = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    // ──────────────────────────────
    //  모드 전환 판정
    // ──────────────────────────────
    void UpdateWaterMode()
    {
        if (!isInWater)
        {
            ChangeMode(GameMode.Ground);
            return;
        }

        float depth = waterSurfaceY - transform.position.y;
        ChangeMode(depth > divingThreshold ? GameMode.Underwater : GameMode.WaterSurface);
    }

    void ChangeMode(GameMode newMode)
    {
        if (CurrentMode == newMode) return;
        CurrentMode = newMode;

        // 산소 시스템 제어
        if (oxygenSystem != null)
        {
            if (newMode == GameMode.Underwater) oxygenSystem.StartDraining();
            else if (newMode == GameMode.WaterSurface) oxygenSystem.StartRefilling();
            else oxygenSystem.StopAll();
        }

        // 잠수 진입 시 dive 속도 초기화
        if (newMode == GameMode.Underwater)
            diveVelocity = Vector3.zero;

        hudManager?.OnModeChanged(newMode);
    }

    // ──────────────────────────────
    //  이동 처리
    // ──────────────────────────────
    void HandleMovement()
    {
        switch (CurrentMode)
        {
            case GameMode.Ground:       HandleGroundMovement();   break;
            case GameMode.WaterSurface: HandleSurfaceSwim();      break;
            case GameMode.Underwater:   HandleDiving();            break;
        }
    }

    void HandleGroundMovement()
    {
        bool isGrounded = controller.isGrounded;
        if (isGrounded && groundVelocity.y < 0f)
            groundVelocity.y = -2f;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float speed = isRunning ? runSpeed : walkSpeed;

        Vector3 moveDir = (transform.right * h + transform.forward * v);
        moveDir.y = 0f;
        if (moveDir.magnitude > 1f) moveDir.Normalize();

        controller.Move(moveDir * speed * Time.deltaTime);

        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
            groundVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // 중력
        groundVelocity.y += gravity * Time.deltaTime;
        controller.Move(Vector3.up * groundVelocity.y * Time.deltaTime);
    }

    void HandleSurfaceSwim()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * h + transform.forward * v);
        moveDir.y = 0f;
        if (moveDir.magnitude > 1f) moveDir.Normalize();

        // 수면으로 자연스럽게 밀어올림
        float distToSurface = transform.position.y - waterSurfaceY;
        float upwardPush = Mathf.Clamp(-distToSurface * surfaceSnapStrength, -3f, 3f);

        Vector3 finalMove = moveDir * swimSpeed + Vector3.up * upwardPush;
        controller.Move(finalMove * Time.deltaTime);
    }

    void HandleDiving()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 horizontalMove = (transform.right * h + transform.forward * v);
        horizontalMove.y = 0f;
        if (horizontalMove.magnitude > 1f) horizontalMove.Normalize();

        // Space = 상승, Ctrl/C = 하강
        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.Space)) verticalInput = 1f;
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C)) verticalInput = -1f;

        Vector3 targetVelocity = horizontalMove * diveHorizontalSpeed + Vector3.up * verticalInput * diveVerticalSpeed;

        // 물의 저항감 (서서히 감속/가속)
        diveVelocity = Vector3.Lerp(diveVelocity, targetVelocity, waterDrag * Time.deltaTime);
        controller.Move(diveVelocity * Time.deltaTime);
    }

    // ──────────────────────────────
    //  외부 호출 (WaterZone)
    // ──────────────────────────────
    public void EnterWater(float surfaceY)
    {
        isInWater = true;
        waterSurfaceY = surfaceY;
        groundVelocity = Vector3.zero;
    }

    public void ExitWater()
    {
        isInWater = false;
        diveVelocity = Vector3.zero;
        ChangeMode(GameMode.Ground);
    }

    // ──────────────────────────────
    //  HUD용 정보 제공
    // ──────────────────────────────
    public float GetDepth()
    {
        return Mathf.Max(0f, waterSurfaceY - transform.position.y);
    }

    public bool IsRunning()
    {
        return CurrentMode == GameMode.Ground &&
               (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
    }
}
