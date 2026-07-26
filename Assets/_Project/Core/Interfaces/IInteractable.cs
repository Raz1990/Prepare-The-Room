public interface IInteractable : IPromptable, IRangeable
{
    ItemType RequiredItem { get; }
    bool CanInteract(ItemType heldItem);
    void Interact(ItemType heldItem);
}