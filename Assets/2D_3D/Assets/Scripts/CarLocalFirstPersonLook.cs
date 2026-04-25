using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLocalFirstPersonLook : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform; // 비워두면 자동: 자신 → 자식 Camera → Camera.main

    [Header("Sensitivity")]
    public float xSensitivity = 120f;         // 좌우 감도(도/초)
    public float ySensitivity = 120f;         // 상하 감도(도/초)
    public float sensitivityMultiplier = 1f;  // 전체 감도 배율(미니게임 때 낮춤)

    [Header("Pitch Settings")]
    public float minPitch = -80f;
    public float maxPitch = 80f;
    public bool invertY = false;

    [Header("Cursor")]
    public bool lockCursorOnStart = true;
    public KeyCode toggleCursorKey = KeyCode.Escape;

    [Header("Yaw Limit (좌우 제한)")]
    [Range(0f, 180f)] public float yawClamp = 50f; // 좌우 각각 제한(±yawClamp)

    [Header("Smoothing")]
    public bool useSmoothing = true;
    [Range(0f, 0.2f)] public float smoothTime = 0.05f;

    float baseLocalYaw;      // 시작 시 로컬 Y 기준
    float yawOffset;         // 현재 좌우 오프셋
    float targetYawOffset;   // 목표 좌우 오프셋
    float pitch;             // 현재 피치
    float targetPitch;       // 목표 피치
    float yawVel;            // SmoothDamp용
    float pitchVel;          // SmoothDamp용
    bool useSeparatePitch;   // cameraTransform이 자식 카메라인지 여부

    void Awake()
    {
        if (cameraTransform == null)
        {
            var camSelf = GetComponent<Camera>();
            if (camSelf != null) cameraTransform = transform;
            else
            {
                var camChild = GetComponentInChildren<Camera>(true);
                if (camChild != null) cameraTransform = camChild.transform;
                else if (Camera.main != null) cameraTransform = Camera.main.transform; // 확실하지 않음
            }
        }
        useSeparatePitch = (cameraTransform != null && cameraTransform != transform);
    }

    void Start()
    {
        if (lockCursorOnStart) SetCursorLock(true);

        baseLocalYaw = NormalizeAngle(transform.localEulerAngles.y);
        yawOffset = targetYawOffset = 0f;

        Transform pRef = cameraTransform != null ? cameraTransform : transform;
        pitch = NormalizeAngle(pRef.localEulerAngles.x);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        targetPitch = pitch;

        ApplyLocalRotations();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleCursorKey))
            SetCursorLock(Cursor.lockState != CursorLockMode.Locked);

        float mx = Input.GetAxisRaw("Mouse X") * xSensitivity * sensitivityMultiplier * Time.deltaTime;
        float my = Input.GetAxisRaw("Mouse Y") * ySensitivity * sensitivityMultiplier * Time.deltaTime;

        // 목표 각도 갱신
        targetYawOffset = Mathf.Clamp(targetYawOffset + mx, -yawClamp, yawClamp);
        float yIn = invertY ? my : -my;
        targetPitch = Mathf.Clamp(targetPitch + yIn, minPitch, maxPitch);

        // 스무딩
        if (useSmoothing)
        {
            yawOffset = Mathf.SmoothDamp(yawOffset, targetYawOffset, ref yawVel, smoothTime);
            pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVel, smoothTime);
        }
        else
        {
            yawOffset = targetYawOffset;
            pitch = targetPitch;
        }

        ApplyLocalRotations();
    }

    void ApplyLocalRotations()
    {
        float targetLocalY = baseLocalYaw + yawOffset;

        if (useSeparatePitch)
        {
            transform.localRotation = Quaternion.Euler(0f, targetLocalY, 0f);    // Yaw
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);     // Pitch
        }
        else
        {
            transform.localRotation = Quaternion.Euler(pitch, targetLocalY, 0f);
        }
    }

    float NormalizeAngle(float a) => (a > 180f) ? a - 360f : a;

    void SetCursorLock(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
