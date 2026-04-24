using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 입력 처리, 물리 이동, 애니메이션, 스프라이트 반전을 담당하는 컨트롤러.
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 현재 프레임의 입력 방향 벡터 (-1, 0, 1 값만 가짐).
    /// 다른 스크립트(Reposition 등)에서 플레이어 이동 방향을 참조할 때 사용.
    /// </summary>
    public Vector2 inputVector;

    /// <summary>
    /// 플레이어 이동 속도. 인스펙터에서 조절 가능.
    /// </summary>
    public float speed;
    public Scanner  scanner;
    
    
    Rigidbody2D rigid;       // 물리 이동에 사용하는 Rigidbody2D 컴포넌트
    SpriteRenderer spriter;  // 좌우 반전 처리에 사용하는 SpriteRenderer 컴포넌트
    Animator anim;           // 애니메이션 파라미터 제어에 사용하는 Animator 컴포넌트
    
    void Awake()
    {
        // 컴포넌트 캐싱 (매 프레임 GetComponent 호출을 피하기 위해 Awake에서 한 번만 가져옴)
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner=GetComponent<Scanner>();
    }

    void Update()
    {
        // GetAxisRaw: -1, 0, 1 의 정수값만 반환 → 대각선 이동 시 normalized 처리 전 원본 입력
        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        // 대각선 이동 시 속도가 빨라지지 않도록 normalized로 방향 벡터 정규화
        // fixedDeltaTime을 곱해 프레임레이트와 무관한 일정한 속도 유지
        Vector2 nextVector = inputVector.normalized * Time.fixedDeltaTime * speed;
        rigid.MovePosition(rigid.position + nextVector);
    }

    void LateUpdate()
    {
        // Speed: 입력이 없으면 0, 있으면 1 이상 → Idle/Walk 애니메이션 전환에 사용
        anim.SetFloat("Speed", inputVector.magnitude);

        // 마지막으로 이동한 방향을 저장 → 정지 시 마지막 방향의 Idle 애니메이션 유지
        if (inputVector.x != 0 || inputVector.y != 0)
        {
            anim.SetFloat("LastDirX", inputVector.x);
            anim.SetFloat("LastDirY", inputVector.y);
        }

        // 현재 이동 방향을 애니메이터에 전달 → 방향별 Walk 애니메이션 전환에 사용
        anim.SetFloat("DirX", inputVector.x);
        anim.SetFloat("DirY", inputVector.y);

        // 왼쪽 이동 시 스프라이트를 수평 반전하여 좌우 방향 표현
        if (inputVector.x != 0)
        {
            spriter.flipX = inputVector.x < 0;
        }
    }
}