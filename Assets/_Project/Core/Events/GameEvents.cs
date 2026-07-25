using System;

public static class GameEvents
{
    // Global event fired when any level task (window, board, AC, etc.) is finished
    public static event Action OnTaskCompleted;

    public static void TriggerTaskCompleted()
    {
        OnTaskCompleted?.Invoke();
    }
}