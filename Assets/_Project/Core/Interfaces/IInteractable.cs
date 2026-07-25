public interface IInteractable
{
    float InteractionRange { get; }
    ItemType RequiredItem { get; }

    string GetPromptText();
    bool CanInteract(ItemType heldItem);
    void Interact(ItemType heldItem);
}