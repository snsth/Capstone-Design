using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ClueItem : MonoBehaviour, IInteractable
{
    [Header("Item Info")]
    public string itemName = "알 수 없는 단서";
    [TextArea(2, 4)]
    public string description = "정체를 알 수 없는 물건이다...";
    public string itemType = "clue";
    public Sprite icon;

    [Header("Animation")]
    public bool enableFloat = true;
    public float floatSpeed = 1.5f;
    public float floatHeight = 0.15f;
    public float rotateSpeed = 45f;

    [Header("Glow")]
    public bool enableGlow = true;
    public Color glowColor = new Color(1f, 0.9f, 0.2f);
    public float glowIntensity = 1.2f;

    Vector3 startPosition;
    Renderer itemRenderer;

    void Start()
    {
        startPosition = transform.position;
        itemRenderer = GetComponent<Renderer>();

        if (enableGlow && itemRenderer != null)
        {
            itemRenderer.material.EnableKeyword("_EMISSION");
            itemRenderer.material.SetColor("_EmissionColor", glowColor * glowIntensity);
        }
    }

    void Update()
    {
        if (!enableFloat) return;
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    public string GetInteractionPrompt()
    {
        return $"[F] {itemName} 줍기";
    }

    public void Interact(InventorySystem inventory)
    {
        if (inventory == null) return;

        ItemData data = new ItemData(itemName, description, itemType, icon);
        if (inventory.AddItem(data))
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
        }
    }
}
