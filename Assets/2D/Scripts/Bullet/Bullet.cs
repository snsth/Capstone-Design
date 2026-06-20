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

        
        if (per >= 0)
        {
            rigid.linearVelocity = direction * 15f;

            
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
        if (!collision.CompareTag("Enemy") || per == -100)
        {
            return;
        }

        // 관통 횟수를 1 차감한다.
        // per = 0으로 초기화된 일반 불릿은 이 시점에서 -1이 되어 바로 사라진다.
        per--;

        if (per < 0)
        {
            rigid.linearVelocity = Vector2.zero;
            gameObject.SetActive(false); // → OnDisable 호출 → CancelInvoke로 타이머 정리
        }

    }
}
