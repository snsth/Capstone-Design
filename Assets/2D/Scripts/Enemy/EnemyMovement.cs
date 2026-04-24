using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 적의 이동 및 스프라이트 반전을 담당하는 컨트롤러.
/// 매 물리 프레임마다 플레이어 위치를 추적하여 접근한다.
/// 오브젝트 풀링과 함께 사용되며, OnEnable에서 플레이어 타겟을 재설정한다.
/// </summary>
public class EnemyMovement : MonoBehaviour
{
    /// <summary>
    /// 적의 이동 속도. 인스펙터에서 적 종류별로 다르게 설정 가능.
    /// </summary>
    public float speed;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animControllers;

    /// <summary>
    /// 추적할 플레이어의 Rigidbody2D. OnEnable에서 자동으로 할당된다.
    /// </summary>
    public Rigidbody2D target;

    /// <summary>
    /// 적의 생존 여부. false이면 이동과 반전 처리를 모두 중단한다.
    /// </summary>
    bool isLive; // 테스트용 true

    Rigidbody2D rigid; // 물리 이동에 사용하는 Rigidbody2D 컴포넌트
    Collider2D coll;
    SpriteRenderer sr; // 좌우 반전 처리에 사용하는 SpriteRenderer 컴포넌트
    Animator anim; // 애니메이션 제어에 사용하는 Animator 컴포넌트
    WaitForFixedUpdate wait;
    
    void Awake()
    {
        // 컴포넌트 캐싱 (Awake에서 한 번만 가져와 성능 최적화)
        rigid = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        wait=new WaitForFixedUpdate();
        coll = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        // 사망 상태면 이동 처리 생략
        if (!isLive || anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
            return;

        // 적 → 플레이어 방향 벡터 (크기 = 현재 거리)
        Vector2 direction = target.position - rigid.position;

        // [강제 재배치 조건] 플레이어와의 거리가 20유닛을 초과하면 재배치
        //
        // 왜 20유닛인가?
        //   - Area 트리거의 크기는 20x20 (반경 10유닛).
        //   - SpawnPoint 중 일부는 플레이어 기준 최대 ~14유닛 거리에 배치되어 있어
        //     Area 경계(10유닛) 바깥에서 적이 스폰되는 경우가 생긴다.
        //   - 이 상태에서 플레이어(speed=3)가 적(speed=1.5)보다 2배 빠르게 도망치면
        //     적은 Area에 한 번도 진입하지 못한 채 영원히 뒤를 쫓게 된다.
        //   - OnTriggerExit2D는 Enter 없이는 발생하지 않으므로
        //     Reposition.cs의 재배치 로직이 절대 호출되지 않는다.
        //   - 따라서 Area 반경(10) + 최대 SpawnPoint 거리(~14)를 고려해
        //     20유닛을 초과하면 "완전히 뒤처졌다"고 판단하고 강제 재배치한다.
        //
        // 왜 sqrMagnitude(400f)를 사용하는가?
        //   - distance = sqrt(sqrMagnitude) 이므로 20유닛 = sqrMagnitude 400
        //   - sqrt 연산은 매 FixedUpdate(초당 ~50회) 모든 적마다 수행하면 비용이 크다.
        //   - sqrMagnitude는 sqrt 없이 비교 가능하므로 성능상 더 유리하다.
        if (direction.sqrMagnitude > 400f)
        {
            // 플레이어의 현재 입력 방향을 재배치 방향으로 사용
            Vector2 playerDir = Gamemanager.instance.player.inputVector;

            // 플레이어가 정지 중(inputVector = 0)이면 기본값 right로 폴백
            // → 정지 상태에서 방향 벡터가 (0,0)이 되어 엉뚱한 곳에 배치되는 것을 방지
            Vector2 moveDir = playerDir.sqrMagnitude > 0 ? playerDir.normalized : Vector2.right;

            // 플레이어 위치에서 진행 방향으로 20유닛 앞에 절대 좌표로 배치
            // + 랜덤 오프셋(-3~3): 여러 적이 같은 지점에 겹쳐 스폰되는 것을 방지
            rigid.position = (Vector2)target.position + moveDir * 20f
                             + new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));
            return;
        }

        // normalized: 방향만 유지하고 크기를 1로 정규화 → 속도가 거리에 영향받지 않음
        
        
        Vector2 nextVector = direction.normalized * Time.fixedDeltaTime * speed;
        rigid.MovePosition(rigid.position + nextVector);

        // 관성 제거: MovePosition 후 남아있는 물리 속도를 0으로 초기화
        rigid.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        // 사망 상태면 반전 처리 생략
        if (!isLive)
            return;

        // 플레이어가 적의 왼쪽에 있으면 스프라이트를 왼쪽으로 반전
        sr.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable()
    {
        // 풀에서 꺼내질 때마다 플레이어 타겟을 재탐색
        // 씬 재시작 또는 플레이어 오브젝트 변경 시에도 올바른 타겟을 참조하도록 보장
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        sr.sortingOrder = 2;
        anim.SetBool("Dead",false);
        health = maxHealth;
        
        
        
    }

    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animControllers[data.spriteType];
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;

    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet")|| !isLive)
        {
            return;
        }

        health -= collision.GetComponent<Bullet>().damage;
        StartCoroutine(KnockBack());

        if (health > 0)
        {
            if (HasParameter("Hit"))
                anim.SetTrigger("Hit");
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            sr.sortingOrder = 1;
            anim.SetBool("Dead",true);
            Gamemanager.instance.kill++;
            Gamemanager.instance.GetExp();

        }
        
    }

    IEnumerator KnockBack()
    {
        yield return wait; // 다음 하나의 물리 프레임 딜레이
        Vector3 playerPos = Gamemanager.instance.player.transform.position;
        Vector3 dirVec = transform.position - playerPos;
        rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
    }

    void Dead()
    {
        gameObject.SetActive(false);
    }

    bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in anim.parameters)
            if (param.name == paramName) return true;
        return false;
    }
}