using System;

public static class GameEvents
{
    // Global event fired when any level task (window, board, AC, etc.) is finished
    public static event Action<ActionID> OnActionCompleted;
    public static event Action<ItemSO> OnItemPickedUp;
    public static event Action<ItemSO> OnItemPlaced;

    public static event Action<TaskSO, string, string> OnTaskStepAdvanced;
    public static event Action<TaskSO> OnTaskCompleted;
    public static event Action OnAllTasksCompleted;
    public static event Action<int> OnActiveTasksAmountChanged;

    // Passive interaction feedback event
    public static event Action<string> OnInteractionFeedback;

    public static void TriggerActionCompleted(ActionID actionID)
    {
        OnActionCompleted?.Invoke(actionID);
    }

    public static void TriggerItemPickedUp(ItemSO item)
    {
        OnItemPickedUp?.Invoke(item);
    }

    public static void TriggerItemPlaced(ItemSO item)
    {
        OnItemPlaced?.Invoke(item);
    }

    public static void TriggerTaskStepAdvanced(TaskSO task, string oldStepDesc, string newStepDesc)
    {
        OnTaskStepAdvanced?.Invoke(task, oldStepDesc, newStepDesc);
    }

    public static void TriggerTaskCompleted(TaskSO task)
    {
        OnTaskCompleted?.Invoke(task);
    }

    public static void TriggerAllTasksCompleted()
    {
        OnAllTasksCompleted?.Invoke();
    }

    public static void TriggerActiveTasksAmountChanged(int count)
    {
        OnActiveTasksAmountChanged?.Invoke(count);
    }

    public static void TriggerInteractionFeedback(string message)
    {
        OnInteractionFeedback?.Invoke(message);
    }
}