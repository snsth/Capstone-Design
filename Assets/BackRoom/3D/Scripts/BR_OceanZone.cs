using UnityEngine;

// Place on the ocean/large water surface object (at surface level).
// Auto-creates a massive trigger collider — no manual collider setup needed.
public class BR_OceanZone : MonoBehaviour
{
    [Header("Trigger Size")]
    [Tooltip("Half-extent in X and Z. Should be larger than your ocean mesh.")]
    public float horizontalExtent = 5000f;
    [Tooltip("Depth of the trigger below the surface.")]
    public float depth = 300f;

    [Header("Ocean Floor")]
    [Tooltip("수면 아래 이 거리에 보이지 않는 바닥 콜라이더를 생성 (맵 밖으로 빠지는 것 방지)")]
    public float floorDepth = 50f;

    public float SurfaceY => transform.position.y;

    void Awake()
    {
        // 수영 트리거
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center    = new Vector3(0f, -depth * 0.5f, 0f);
        box.size      = new Vector3(horizontalExtent * 2f, depth, horizontalExtent * 2f);

        if (!GetComponent<Rigidbody>())
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;
        }

        // 바닥 콜라이더 — 별도 자식 오브젝트로 생성
        var floorGO = new GameObject("OceanFloor_Collider");
        floorGO.transform.SetParent(transform);
        floorGO.transform.localPosition = new Vector3(0f, -floorDepth, 0f);
        floorGO.layer = gameObject.layer;

        var floorBox = floorGO.AddComponent<BoxCollider>();
        floorBox.isTrigger = false;
        floorBox.center = Vector3.zero;
        floorBox.size   = new Vector3(horizontalExtent * 2f, 1f, horizontalExtent * 2f);
    }
}
