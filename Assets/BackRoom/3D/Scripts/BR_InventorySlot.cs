using UnityEngine;
using UnityEngine.EventSystems;

// Attached to each item row in the inventory list. Handles right-click context menu.
public class BR_InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public InventoryEntry entry;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            BR_UIManager.Instance?.ShowContextMenu(entry, Input.mousePosition);
    }
}
