using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WaterZone : MonoBehaviour
{
    [Tooltip("이 오브젝트의 Y 위치가 수면으로 사용됩니다. 오프셋으로 조정 가능.")]
    public float surfaceOffset = 0f;

    public float SurfaceY => transform.position.y + transform.localScale.y * 0.5f + surfaceOffset;

    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
            player.EnterWater(SurfaceY);
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
            player.ExitWater();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.8f);
        Vector3 surfaceLine = new Vector3(transform.position.x, SurfaceY, transform.position.z);
        Gizmos.DrawWireCube(surfaceLine, new Vector3(transform.localScale.x, 0.05f, transform.localScale.z));
    }
}
