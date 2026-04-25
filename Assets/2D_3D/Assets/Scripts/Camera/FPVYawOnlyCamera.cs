using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FPVYawOnlyCamera : MonoBehaviour
{
    public Transform target;                  // 자동차 Transform
    public Vector3 localSeatOffset = new Vector3(0f, 1.2f, 0.3f); // 운전석 위치 오프셋
    public float yawOffset = 0f;              // 필요하면 좌/우 추가 보정각
    public float posSmooth = 0f;              // 0이면 즉시, >0이면 부드럽게
    public float rotSmooth = 0f;              // 0이면 즉시, >0이면 부드럽게

    void LateUpdate()
    {
        if (!target) return;

        // 위치: 타겟의 로컬 좌표를 월드로 변환(차량이 피치/롤해도 좌석 위치는 따라감)
        Vector3 desiredPos = target.TransformPoint(localSeatOffset);

        // 회전: 타겟의 Yaw만 사용(피치/롤 제거)
        float yaw = target.eulerAngles.y + yawOffset;
        Quaternion desiredRot = Quaternion.Euler(0f, yaw, 0f);

        if (posSmooth > 0f)
            transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-posSmooth * Time.deltaTime));
        else
            transform.position = desiredPos;

        if (rotSmooth > 0f)
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, 1f - Mathf.Exp(-rotSmooth * Time.deltaTime));
        else
            transform.rotation = desiredRot;
    }
}
