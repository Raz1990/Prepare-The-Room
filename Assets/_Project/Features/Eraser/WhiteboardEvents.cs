using System;

public static class WhiteboardEvents
{
    public static event Action OnWhiteboardCleaningStarted;

    public static void TriggerWhiteboardCleaningStarted()
    {
        OnWhiteboardCleaningStarted?.Invoke();
    }
}