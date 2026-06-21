using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 계단 오브젝트에 붙이세요 (BoxCollider isTrigger는 직접 추가).
// 플레이어가 들어오면 부모 BackRoom_Pool 층에 몬스터를 스폰합니다.
public class BR_StairsSpawn : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject monsterPrefab;
    public int   spawnCount      = 3;
    public float spawnRadius     = 10f;
    public float minDistPlayer   = 6f;
    public float floorYTolerance = 3f;
    public float respawnCooldown = 5f;

    Transform roomRoot;
    Transform player;
    float lastSpawnTime = -999f;
    readonly List<GameObject> spawned = new();

    void Awake()
    {
        Transform t = transform.parent;
        while (t != null)
        {
            if (t.name.StartsWith("BackRoom_Pool", System.StringComparison.OrdinalIgnoreCase))
            { roomRoot = t; break; }
            t = t.parent;
        }
        if (roomRoot == null)
            Debug.LogWarning($"[BR_StairsSpawn] '{name}' — 부모에서 BackRoom_Pool을 찾지 못했습니다.");
    }

    void Start()
    {
        var pc = FindAnyObjectByType<BR_PlayerController>();
        if (pc != null) player = pc.transform;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<BR_PlayerController>() == null) return;
        if (Time.time - lastSpawnTime < respawnCooldown) return;

        spawned.RemoveAll(m => m == null);
        if (spawned.Count > 0) return;

        SpawnMonsters();
    }

    void SpawnMonsters()
    {
        if (monsterPrefab == null) { Debug.LogWarning("[BR_StairsSpawn] monsterPrefab을 연결하세요."); return; }

        Vector3 center    = roomRoot != null ? roomRoot.position : transform.position;
        Vector3 playerPos = player != null ? player.position : center;
        int count = 0;

        for (int i = 0; i < 200 && count < spawnCount; i++)
        {
            Vector2 c   = Random.insideUnitCircle.normalized * Random.Range(minDistPlayer, spawnRadius);
            Vector3 pos = center + new Vector3(c.x, 0, c.y);

            if (Vector3.Distance(pos, playerPos) < minDistPlayer) continue;
            if (!NavMesh.SamplePosition(pos, out NavMeshHit hit, 3f, NavMesh.AllAreas)) continue;
            if (Mathf.Abs(hit.position.y - center.y) > floorYTolerance) continue;

            Vector3 bot = hit.position + Vector3.up * 0.4f;
            Vector3 top = hit.position + Vector3.up * 1.4f;
            if (Physics.CheckCapsule(bot, top, 0.4f, ~0, QueryTriggerInteraction.Ignore)) continue;

            var m = Instantiate(monsterPrefab, hit.position, Quaternion.identity);
            spawned.Add(m);
            count++;
        }

        lastSpawnTime = Time.time;
        Debug.Log($"[BR_StairsSpawn] '{roomRoot?.name}' 몬스터 {count}마리 스폰");
    }
}
