using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct InventoryEntry
{
    public string id;
    public string displayName;
    public BR_ItemType itemType;
    public GameObject dropPrefab;

    public InventoryEntry(string id, string displayName, BR_ItemType itemType, GameObject dropPrefab)
    {
        this.id = id;
        this.displayName = displayName;
        this.itemType = itemType;
        this.dropPrefab = dropPrefab;
    }
}

public class BR_Inventory : MonoBehaviour
{
    public static BR_Inventory Instance { get; private set; }

    [Header("References")]
    public BR_FlashlightController flashlight;

    readonly List<InventoryEntry> items = new List<InventoryEntry>();
    public IReadOnlyList<InventoryEntry> Items => items;

    public string EquippedItemId { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (flashlight == null)
            flashlight = FindAnyObjectByType<BR_FlashlightController>();
        if (flashlight == null)
            flashlight = new GameObject("[BR_FlashlightController]")
                             .AddComponent<BR_FlashlightController>();
    }

    // Returns the camera transform via the most reliable path available
    Transform GetCameraTransform()
    {
        var pc = FindAnyObjectByType<BR_PlayerController>();
        if (pc != null && pc.cameraTransform != null) return pc.cameraTransform;
        if (Camera.main != null) return Camera.main.transform;
        var anyCam = FindAnyObjectByType<Camera>();
        return anyCam != null ? anyCam.transform : null;
    }

    // Lazy-find flashlight so it always works even if setup was run before Play
    BR_FlashlightController GetFlashlight()
    {
        if (flashlight == null)
            flashlight = FindAnyObjectByType<BR_FlashlightController>();
        return flashlight;
    }

    public void Add(string id, string displayName, BR_ItemType itemType = BR_ItemType.Default, GameObject dropPrefab = null)
    {
        items.Add(new InventoryEntry(id, displayName, itemType, dropPrefab));
        BR_UIManager.Instance?.RefreshInventory(items);
    }

    public bool Has(string id)
    {
        foreach (var entry in items)
            if (entry.id == id) return true;
        return false;
    }

    public void Remove(string id)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == id)
            {
                items.RemoveAt(i);
                break;
            }
        }
        BR_UIManager.Instance?.RefreshInventory(items);
    }

    public void Equip(InventoryEntry entry)
    {
        if (EquippedItemId == entry.id)
        {
            UnequipCurrent();
            return;
        }

        UnequipCurrent();
        EquippedItemId = entry.id;

        if (entry.itemType == BR_ItemType.Flashlight)
            GetFlashlight()?.SetActive(true);

        BR_UIManager.Instance?.RefreshInventory(items);
    }

    public void UnequipCurrent()
    {
        if (string.IsNullOrEmpty(EquippedItemId)) return;

        foreach (var e in items)
        {
            if (e.id == EquippedItemId)
            {
                if (e.itemType == BR_ItemType.Flashlight)
                    GetFlashlight()?.SetActive(false);
                break;
            }
        }
        EquippedItemId = null;
        BR_UIManager.Instance?.RefreshInventory(items);
    }

    public void Drop(InventoryEntry entry)
    {
        if (EquippedItemId == entry.id) UnequipCurrent();

        // Find spawn position: raycast floor directly in front of the player
        Transform cam = GetCameraTransform();

        Vector3 spawnPos;
        if (cam != null)
        {
            // Project forward horizontally so we don't dig into walls above/below
            Vector3 forward = cam.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = cam.transform.parent != null
                ? cam.transform.parent.forward : Vector3.forward;
            forward.Normalize();

            Vector3 origin = cam.position + forward * 1.2f + Vector3.up * 0.5f;

            // Raycast down to find the actual floor
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 3f))
                spawnPos = hit.point + Vector3.up * 0.05f;
            else
                spawnPos = cam.position + forward * 1.2f + Vector3.down * 0.4f;
        }
        else
        {
            spawnPos = transform.position + Vector3.forward * 1.2f;
        }

        if (entry.dropPrefab != null)
        {
            Instantiate(entry.dropPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Fallback sphere so the item always appears physically
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = entry.displayName;
            go.transform.position = spawnPos;
            go.transform.localScale = Vector3.one * 0.18f;
            var rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.5f;
            var wi = go.AddComponent<BR_WorldItem>();
            wi.itemId = entry.id;
            wi.itemName = entry.displayName;
            wi.itemType = entry.itemType;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == entry.id) { items.RemoveAt(i); break; }
        }
        BR_UIManager.Instance?.RefreshInventory(items);
    }
}
