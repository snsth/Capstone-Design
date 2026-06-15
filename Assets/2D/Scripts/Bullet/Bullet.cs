using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public int per; // 관통 데미지

    Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    
    public void Init(float damage,int per, Vector2 direction)
    {
        this.damage = damage;
        this.per = per;

        // per > -1 : 원거리 발사 불릿에만 속도와 수명 타이머를 적용한다.
        // per == -1은 근접 회전 무기(무한 관통)이므로 velocity와 Invoke 모두 불필요.
        if (per > -1)
        {
            rigid.linearVelocity = direction * 15f;

            // 적에 맞지 않고 맵 밖으로 나간 불릿을 5초 후 자동 회수한다.
            // Destroy 대신 SetActive(false)를 사용하는 이유:
            //   - 이 프로젝트는 오브젝트 풀링을 사용하므로 오브젝트를 파괴하면
            //     풀에서 재사용할 수 없어 매번 새로 생성해야 한다.
            //   - SetActive(false)로 비활성화하면 풀이 나중에 꺼내 재사용할 수 있어
            //     메모리 할당/GC 비용을 줄일 수 있다.
            Invoke("DeactivateBullet", 5f);
        }

    }

    // Invoke로 예약된 수명 타이머가 만료되면 호출된다.
    // velocity를 먼저 0으로 초기화해 비활성화 직전 관성이 남지 않도록 한다.
    void DeactivateBullet()
    {
        rigid.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    // 적에 맞아 OnTriggerEnter2D에서 먼저 SetActive(false)가 호출되면
    // OnDisable이 발생한다. 이때 아직 대기 중인 Invoke 타이머를 취소해
    // 이미 비활성화된 오브젝트에 DeactivateBullet이 중복 호출되는 것을 방지한다.
    void OnDisable()
    {
        CancelInvoke("DeactivateBullet");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Enemy 태그가 아닌 충돌(벽, 플레이어 등)은 무시한다.
        // per == -1인 근접 회전 무기는 충돌해도 사라지지 않아야 하므로 함께 무시한다.
        if (!collision.CompareTag("Enemy") || per == -1)
        {
            return;
        }

        // 관통 횟수를 1 차감한다.
        // per = 0으로 초기화된 일반 불릿은 이 시점에서 -1이 되어 바로 사라진다.
        per--;

        if (per == -1)
        {
            rigid.linearVelocity = Vector2.zero;
            gameObject.SetActive(false); // → OnDisable 호출 → CancelInvoke로 타이머 정리
        }

    }
}
