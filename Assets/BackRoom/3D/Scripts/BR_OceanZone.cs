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

    // BR_PlayerController reads this to know where the surface is
    public float SurfaceY => transform.position.y;

    void Awake()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null) box = gameObject.AddComponent<BoxCollider>();

        // Top of the box aligns with this object's Y position (water surface)
        box.isTrigger = true;
        box.center    = new Vector3(0f, -depth * 0.5f, 0f);
        box.size      = new Vector3(horizontalExtent * 2f, depth, horizontalExtent * 2f);

        if (!GetComponent<Rigidbody>())
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;
        }
    }
}
