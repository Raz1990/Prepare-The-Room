using System;

public static class GameEvents
{
    // Global event fired when any level task (window, board, AC, etc.) is finished
    public static event Action<TaskID> OnTaskCompleted;
    public static event Action<ItemSO> OnItemPickedUp;
    public static event Action<ItemSO> OnItemPlaced;

    // Passive interaction feedback event
    public static event Action<string> OnInteractionFeedback;

    public static void TriggerTaskCompleted(TaskID taskID)
    {
        OnTaskCompleted?.Invoke(taskID);
    }

    public static void TriggerItemPickedUp(ItemSO item)
    {
        OnItemPickedUp?.Invoke(item);
    }

    public static void TriggerItemPlaced(ItemSO item)
    {
        OnItemPlaced?.Invoke(item);
    }

    public static void TriggerInteractionFeedback(string message)
    {
        OnInteractionFeedback?.Invoke(message);
    }
}