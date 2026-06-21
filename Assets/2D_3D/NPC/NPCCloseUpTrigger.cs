using UnityEngine;
using Cinemachine;

public class NPCCloseUpTrigger : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera npcCamera;
    [SerializeField] private CinemachineVirtualCamera playerCamera;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        playerCamera.Priority = 0;
        npcCamera.Priority = 20;

        Invoke(nameof(ReturnCamera), 3f);
    }

    private void ReturnCamera()
    {
        npcCamera.Priority = 0;
        playerCamera.Priority = 20;
    }
}