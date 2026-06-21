using UnityEngine;

public class MonsterKill : MonoBehaviour
{
    private JumpScareManager jumpScareManager;

    private void Start()
    {
        jumpScareManager =
            FindFirstObjectByType<JumpScareManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        jumpScareManager.StartJumpScare();
    }
}