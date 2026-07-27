using UnityEngine;

[System.Serializable]
public class TaskStep
{
    [Header("UI Display")]
    public string stepDescription; // e.g. "Pick up the metal can"

    [Header("Completion Logic")]
    public TaskActionType actionTypeToComplete; // e.g "Close the Window"

    [Header("Target Match")]
    [Tooltip("Required if actionTypeToComplete is PickupItem or PlaceItem")]
    public ItemSO targetItem;

    [Tooltip("Required if actionTypeToComplete is Interaction")]
    public ActionID targetActionID;
}