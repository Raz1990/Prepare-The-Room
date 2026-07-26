public interface IInteractable : IPromptable, IRangeable
{
    ItemType RequiredItem { get; }
    bool CanInteract();
    void Interact(ItemType heldItem);
}