using UnityEngine;

public interface IPickupable : IPromptable, IRangeable
{
    ItemSO ItemData { get; }
    bool CanPickup(ItemType heldItem);
    void Pickup(Transform handSocket);
}