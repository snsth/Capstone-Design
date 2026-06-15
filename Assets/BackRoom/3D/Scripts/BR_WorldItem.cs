using UnityEngine;

public enum BR_ItemType { Default, Flashlight }

public class BR_WorldItem : MonoBehaviour
{
    public string itemId = "item_01";
    public string itemName = "Item";
    public BR_ItemType itemType = BR_ItemType.Default;
    [Tooltip("Assign the prefab to spawn when this item is dropped from inventory")]
    public GameObject dropPrefab;

    public string ItemName => itemName;

    public void Pickup()
    {
        BR_Inventory.Instance?.Add(itemId, itemName, itemType, dropPrefab);
        Destroy(gameObject);
    }
}
