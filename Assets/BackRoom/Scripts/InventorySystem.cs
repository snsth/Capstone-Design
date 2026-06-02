using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [Header("Settings")]
    public int maxSlots = 8;

    readonly List<ItemData> items = new List<ItemData>();
    int selectedIndex = -1;

    public bool IsOpen { get; private set; }
    public IReadOnlyList<ItemData> Items => items;
    public int SelectedIndex => selectedIndex;
    public int MaxSlots => maxSlots;

    public System.Action<ItemData> OnItemAdded;
    public System.Action<int> OnItemUsed;
    public System.Action<bool> OnInventoryToggled;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
            ToggleInventory();

        if (IsOpen) return;

        // 숫자키 1~8로 빠른 슬롯 선택
        for (int i = 0; i < 8; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectItem(i);
        }

        // E키로 선택된 아이템 사용
        if (Input.GetKeyDown(KeyCode.E) && selectedIndex >= 0)
            UseItem(selectedIndex);
    }

    public bool AddItem(ItemData item)
    {
        if (items.Count >= maxSlots) return false;
        items.Add(item);
        OnItemAdded?.Invoke(item);
        return true;
    }

    public void UseItem(int index)
    {
        if (index < 0 || index >= items.Count) return;
        OnItemUsed?.Invoke(index);
    }

    public void RemoveItem(int index)
    {
        if (index < 0 || index >= items.Count) return;
        items.RemoveAt(index);
        if (selectedIndex >= items.Count)
            selectedIndex = items.Count - 1;
    }

    public void SelectItem(int index)
    {
        selectedIndex = (index >= 0 && index < items.Count) ? index : -1;
    }

    public void ToggleInventory()
    {
        IsOpen = !IsOpen;
        OnInventoryToggled?.Invoke(IsOpen);
        Cursor.lockState = IsOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsOpen;
    }

    public void CloseInventory()
    {
        if (!IsOpen) return;
        IsOpen = false;
        OnInventoryToggled?.Invoke(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
