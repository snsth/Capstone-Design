using UnityEngine;

// Pool (3) 오브젝트에 붙이세요 (BoxCollider isTrigger는 직접 추가).
// 플레이어가 들어오면 부모 BackRoom_Pool 범위 안의 몬스터를 제거합니다.
public class BR_PoolClear : MonoBehaviour
{
    [Tooltip("이 반경 안의 몬스터를 제거 (해당 층 BackRoom_Pool 기준)")]
    public float clearRadius = 25f;

    Transform roomRoot;

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
            Debug.LogWarning($"[BR_PoolClear] '{name}' — 부모에서 BackRoom_Pool을 찾지 못했습니다.");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<BR_PlayerController>() == null) return;

        Vector3 center = roomRoot != null ? roomRoot.position : transform.position;
        var monsters = FindObjectsByType<BR_Monster>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var m in monsters)
        {
            if (Vector3.Distance(m.transform.position, center) <= clearRadius)
            {
                Destroy(m.gameObject);
                count++;
            }
        }
        Debug.Log($"[BR_PoolClear] '{roomRoot?.name}' 몬스터 {count}마리 제거");
    }
}
