public interface IInteractable
{
    string GetInteractionPrompt();
    void Interact(InventorySystem inventory);
}
