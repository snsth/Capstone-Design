using UnityEngine;
using UnityEngine.AI;

public class BR_MonsterSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject monsterPrefab;
    public int spawnCount = 3;
    [Tooltip("이 오브젝트 중심에서 스폰 반경")]
    public float spawnRadius = 30f;
    [Tooltip("플레이어와 최소 이격 거리 (너무 가까이 스폰 방지)")]
    public float minDistFromPlayer = 15f;

    void Start()
    {
        if (monsterPrefab == null)
        {
            Debug.LogWarning("[BR_MonsterSpawner] monsterPrefab이 할당되지 않았습니다.");
            return;
        }
        SpawnMonsters();
    }

    void SpawnMonsters()
    {
        var pc = FindObjectOfType<BR_PlayerController>();
        Vector3 playerPos = pc != null ? pc.transform.position : transform.position;

        int spawned = 0;
        int maxAttempts = 200;

        for (int attempt = 0; attempt < maxAttempts && spawned < spawnCount; attempt++)
        {
            // 랜덤 위치 생성
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(minDistFromPlayer, spawnRadius);
            Vector3 candidate = transform.position + new Vector3(circle.x, 0f, circle.y);

            // 플레이어와 최소 거리 체크
            if (Vector3.Distance(candidate, playerPos) < minDistFromPlayer) continue;

            // NavMesh 위 유효 지점 찾기
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit navHit, 3f, NavMesh.AllAreas)) continue;

            // 물속 스폰 방지
            if (IsInWater(navHit.position)) continue;

            Instantiate(monsterPrefab, navHit.position, Quaternion.identity);
            spawned++;
        }

        if (spawned < spawnCount)
            Debug.LogWarning($"[BR_MonsterSpawner] 목표 {spawnCount}마리 중 {spawned}마리만 스폰됨. NavMesh 베이킹 확인 권장.");
    }

    bool IsInWater(Vector3 pos)
    {
        var cols = Physics.OverlapSphere(pos, 0.5f, ~0, QueryTriggerInteraction.Collide);
        foreach (var col in cols)
        {
            if (col.GetComponent<BR_WaterZone>() != null) return true;
            if (col.GetComponent<BR_OceanZone>() != null) return true;
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, minDistFromPlayer);
    }
}
