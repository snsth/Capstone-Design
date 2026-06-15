using UnityEngine;

// Place this object AT the exact water surface level.
// The trigger collider should extend downward from the surface.
public class BR_WaterZone : MonoBehaviour
{
    // SurfaceY = this object's world Y — place the object at the visible water surface.
    public float SurfaceY => transform.position.y;

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (!GetComponent<Rigidbody>())
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }
}
