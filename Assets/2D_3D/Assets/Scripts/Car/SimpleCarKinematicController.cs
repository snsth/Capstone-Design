using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCarKinematicController : MonoBehaviour
{
    public float forwardSpeed = 10f;   // 전진 속도(유닛/초)
    public float reverseSpeed = 6f;    // 후진 속도
    public float turnSpeed = 120f;     // 회전 속도(도/초)
    public bool lockYHeight = true;    // 시작 높이 고정 여부

    private float startY;

    void Start()
    {
        if (lockYHeight) startY = transform.position.y;
    }

    void Update()
    {
        // 이동
        float move = 0f;
        if (Input.GetKey(KeyCode.W)) move += forwardSpeed;
        if (Input.GetKey(KeyCode.S)) move -= reverseSpeed;

        if (move != 0f)
            transform.position += transform.forward * move * Time.deltaTime;

        // 회전(Yaw)
        float steer = 0f;
        if (Input.GetKey(KeyCode.A)) steer -= 1f;
        if (Input.GetKey(KeyCode.D)) steer += 1f;

        if (steer != 0f)
            transform.Rotate(0f, steer * turnSpeed * Time.deltaTime, 0f, Space.Self);

        // Y 높이 고정
        if (lockYHeight)
        {
            Vector3 p = transform.position;
            p.y = startY;
            transform.position = p;
        }
    }
}
