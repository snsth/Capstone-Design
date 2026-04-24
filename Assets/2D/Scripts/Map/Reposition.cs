using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 맵 타일 및 적 오브젝트의 위치를 재배치하는 스크립트.
/// 플레이어가 Collider(Area)를 벗어날 때 호출되어 오브젝트를 플레이어 진행 방향 앞으로 이동시킨다.
/// 이를 통해 무한 맵처럼 보이는 효과를 구현한다.
///
/// [태그별 동작]
/// - "Ground" : 플레이어 이동 방향(X 또는 Y 중 더 많이 벗어난 축)으로 40유닛 이동
/// - "Enemy"  : 플레이어 진행 방향으로 20유닛 + 약간의 랜덤 오프셋을 더해 이동
/// </summary>
public class Reposition : MonoBehaviour
{
    /// <summary>
    /// 이 오브젝트의 Collider2D. 적 오브젝트의 활성화 여부 확인에 사용.
    /// </summary>
    Collider2D myCollider;

    void Awake()
    {
        myCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // "Area" 태그를 가진 Collider가 아니면 무시 (Area = 카메라 주변 경계 영역)
        if (!collision.CompareTag("Area"))
            return;

        Vector3 playerPos = Gamemanager.instance.player.transform.position;
        Vector3 myPos = transform.position;

        // 플레이어와 이 오브젝트 간의 X, Y 거리 차이 계산
        float differnetX = Mathf.Abs(playerPos.x - myPos.x);
        float differnetY = Mathf.Abs(playerPos.y - myPos.y);

        // 플레이어의 현재 입력 방향 벡터
        Vector3 playerDir = Gamemanager.instance.player.inputVector;

        // 각 축의 이동 방향 부호 (-1 또는 1)
        float dirX = playerDir.x < 0 ? -1 : 1;
        float dirY = playerDir.y < 0 ? -1 : 1;

        switch (transform.tag)
        {
            case "Ground":
                // X축으로 더 많이 벗어났다면 X방향으로 재배치, Y축이면 Y방향으로 재배치
                // 40유닛 = 타일 크기에 맞춰 한 칸 앞으로 이동
                if (differnetX > differnetY)
                {
                    transform.Translate(Vector3.right * dirX * 40);
                }
                else if (differnetY > differnetX)
                {
                    transform.Translate(Vector3.up * dirY * 40);
                }
                break;

            case "Enemy":
                // 콜라이더가 비활성화된 적(사망 등)은 재배치 생략
                if (myCollider.enabled)
                {
                    // 플레이어 위치 기준 절대 좌표로 재배치 (상대 이동 시 멀어진 적이 Area 밖에 머무는 버그 방지)
                    Vector3 dir = playerDir.sqrMagnitude > 0 ? playerDir.normalized : Vector3.right;
                    transform.position = playerPos + dir * 20f + new Vector3(Random.Range(-3f, 3f), Random.Range(-3f, 3f), 0f);
                }
                break;
        }
    }
}