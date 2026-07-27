public enum TaskActionType
{
    PickupItem,        // Matches when targetItem is picked up
    PlaceItem,         // Matches when targetItem is placed
    Interaction        // Matches when targetTaskID is triggered (e.g. BoardWiped, WindowClosed)
}