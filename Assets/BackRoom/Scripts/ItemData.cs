using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string itemName;
    public string description;
    public string itemType; // "clue", "key", "note", "artifact"
    public Sprite icon;

    public ItemData() { }

    public ItemData(string name, string desc, string type, Sprite icon = null)
    {
        itemName = name;
        description = desc;
        itemType = type;
        this.icon = icon;
    }
}
