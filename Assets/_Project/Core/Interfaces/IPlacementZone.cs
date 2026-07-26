using System;
using UnityEngine;

public interface IPlacementZone : IPromptable, IRangeable
{
    ItemType TargetItemType { get; }
    bool IsFilled { get; }

    bool CanPlace(ItemType heldItem);
    void PlaceItem(GameObject itemObject, Action onComplete);
}