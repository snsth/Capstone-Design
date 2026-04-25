using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrowsyProximityTrigger : MonoBehaviour
{
    [Header("References")]
    public Transform player;                        // camera-driver
    public DrowsySteeringMinigame minigame;        // 비워두면 player에서 자동 탐색

    [Header("Trigger Settings")]
    public float radius = 6f;                       // 발동 반경
    public bool triggerOnce = true;                 // 한 번만 발동
    public float retriggerCooldown = 5f;            // 재발동 대기(once=false일 때)
    public bool startOnlyWhenNotRunning = true;     // 미니게임 실행 중이면 무시
    [TextArea] public string overrideStartMessage = ""; // 구간별 커스텀 문구

    // 내부 상태
    bool armed = true;
    float cooldownTimer = 0f;

    void Awake()
    {
        // 기본 플레이어 자동 할당(없으면 Camera.main)
        if (player == null && Camera.main != null)
            player = Camera.main.transform;

        // minigame 자동 연결: player에 붙은 컴포넌트에서 찾기
        if (minigame == null && player != null)
            minigame = player.GetComponent<DrowsySteeringMinigame>();
    }

    void Update()
    {
        if (player == null || minigame == null) return;

        // 쿨다운
        if (!armed && !triggerOnce)
        {
            if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
            else armed = true;
        }

        if (!armed) return;
        if (startOnlyWhenNotRunning && minigame.IsActive) return;

        float sqr = (player.position - transform.position).sqrMagnitude;
        if (sqr <= radius * radius)
        {
            if (!string.IsNullOrEmpty(overrideStartMessage))
                minigame.startMessage = overrideStartMessage;

            minigame.StartMinigame();

            if (triggerOnce)
            {
                armed = false; // 영구 해제
            }
            else
            {
                armed = false;
                cooldownTimer = retriggerCooldown;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}