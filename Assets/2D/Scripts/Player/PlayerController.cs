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
    public Hand[] hands;

    // 공포 이벤트: 입력 제어 플래그
    public static bool isInputBlocked  = false;  // true면 이동 완전 차단
    public static bool isInputReversed = false;  // true면 WASD 방향 반전
    
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
        //hands=GetComponentsInChildren<Hand>(true);
    }

    void Start()
    {
        // Area 크기 = max(카메라 뷰 + 여유, 타일 그리드 전체 스팬)
        // 타일 그리드: 4×4 배치(간격 20), 스팬 80×80
        // - 카메라보다 작으면 빈 화면 노출
        // - 타일 그리드(80)보다 작으면 외곽 타일이 Area 밖에서 시작해 OnTriggerExit가 발동 안 됨
        // - jump(80)보다 Area_half가 너무 작으면 재배치 후 즉시 Area 밖에 착지해 연쇄 이동 발생
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        Transform area = transform.Find("Area");
        if (area != null)
        {
            float areaW = Mathf.Max(camWidth + 20f, 84f);
            float areaH = Mathf.Max(camHeight + 20f, 84f);
            area.GetComponent<BoxCollider2D>().size = new Vector2(areaW, areaH);
        }
    }

    void Update()
    {
        if (Gamemanager.instance.isLive == false) return;

        if (isInputBlocked)
        {
            inputVector = Vector2.zero;
            return;
        }

        inputVector.x = Input.GetAxisRaw("Horizontal");
        inputVector.y = Input.GetAxisRaw("Vertical");

        if (isInputReversed) inputVector = -inputVector;
    }

    void FixedUpdate()
    {
        if (Gamemanager.instance.isLive == false) return;
        // 대각선 이동 시 속도가 빨라지지 않도록 normalized로 방향 벡터 정규화
        // fixedDeltaTime을 곱해 프레임레이트와 무관한 일정한 속도 유지
        Vector2 nextVector = inputVector.normalized * Time.fixedDeltaTime * speed;
        rigid.MovePosition(rigid.position + nextVector);
    }

    void LateUpdate()
    {
        if (Gamemanager.instance.isLive == false) return;
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

    void OnCollisionStay2D(Collision2D collision)
    {
        if(!Gamemanager.instance.isLive) return;

        Gamemanager.instance.health -= Time.deltaTime * 10f;

        if(Gamemanager.instance.health < 0)
        {
            Gamemanager.instance.health = 0;
            Gamemanager.instance.isLive = false;

            for(int index = 2; index < transform.childCount; index++)
            {
                transform.GetChild(index).gameObject.SetActive(false);
            }

            anim.SetTrigger("Dead");
            Gamemanager.instance.GameOver();
        }
    }
}