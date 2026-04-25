using UnityEngine;

public class SteeringWheelController : MonoBehaviour
{
    [Header("Target")]
    public Transform steeringWheel; // 비우면 현재 오브젝트 사용

    [Header("Tuning")]
    [Tooltip("키를 누르고 있을 때 초당 회전량 (도/초)")]
    public float rotationSpeed = 180f;

    [Tooltip("최대 좌/우 회전 제한 (도). 0 이하이면 제한 없음")]
    public float maxAbsAngle = 450f;

    [Tooltip("키 입력이 없을 때 중앙(0도)으로 복귀하는 속도(도/초)")]
    public float autoCenterSpeed = 300f;

    private Quaternion initialLocalRotation;
    private float currentAngle; // Z축 기준 상대 각도(도)

    void Awake()
    {
        if (steeringWheel == null) steeringWheel = transform;
        initialLocalRotation = steeringWheel.localRotation;
        currentAngle = 0f;
    }

    void Update()
    {
        float input = 0f;

        if (Input.GetKey(KeyCode.A)) input += 1f;  // 좌측(각도 증가)
        if (Input.GetKey(KeyCode.D)) input -= 1f;  // 우측(각도 감소)

        if (input != 0f)
        {
            currentAngle += input * rotationSpeed * Time.deltaTime;
        }
        else
        {
            // 입력이 없으면 0도로 자동 복귀
            currentAngle = Mathf.MoveTowards(currentAngle, 0f, autoCenterSpeed * Time.deltaTime);
        }

        if (maxAbsAngle > 0f)
            currentAngle = Mathf.Clamp(currentAngle, -maxAbsAngle, maxAbsAngle);

        steeringWheel.localRotation =
            initialLocalRotation * Quaternion.AngleAxis(currentAngle, Vector3.forward);
    }
}