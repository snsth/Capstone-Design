using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    [Header("Settings")]
    public Camera playerCamera;
    public float interactionRange = 3f;
    public LayerMask interactionMask = -1;

    InventorySystem inventory;
    IInteractable currentTarget;

    public IInteractable CurrentTarget => currentTarget;

    void Awake()
    {
        inventory = GetComponent<InventorySystem>();
    }

    void Update()
    {
        if (inventory != null && inventory.IsOpen) return;

        ScanForInteractable();

        if (currentTarget != null && Input.GetKeyDown(KeyCode.F))
            currentTarget.Interact(inventory);
    }

    void ScanForInteractable()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask))
        {
            currentTarget = hit.collider.GetComponentInParent<IInteractable>();
        }
        else
        {
            currentTarget = null;
        }
    }

    public string GetCurrentPrompt()
    {
        return currentTarget?.GetInteractionPrompt();
    }
}
