using System;

public static class GameEvents
{
    // Global event fired when any level task (window, board, AC, etc.) is finished
    public static event Action OnTaskCompleted;
    public static event Action<ItemSO> OnItemPickedUp;
    public static event Action<ItemSO> OnItemPlaced;

    public static void TriggerTaskCompleted()
    {
        OnTaskCompleted?.Invoke();
    }

    public static void TriggerItemPickedUp(ItemSO item)
    {
        OnItemPickedUp?.Invoke(item);
    }

    public static void TriggerItemPlaced(ItemSO item)
    {
        OnItemPlaced?.Invoke(item); 
    }
}