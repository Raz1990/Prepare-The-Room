public class TaskProgress
{
    public TaskSO TaskData { get; private set; }
    public int CurrentStepIndex { get; private set; }
    public bool IsCompleted { get; private set; }

    public TaskStep CurrentStep => (TaskData != null && CurrentStepIndex < TaskData.steps.Count)
        ? TaskData.steps[CurrentStepIndex]
        : null;

    public TaskProgress(TaskSO taskData)
    {
        TaskData = taskData;
        CurrentStepIndex = 0;
        IsCompleted = false;
    }

    public void AdvanceStep()
    {
        CurrentStepIndex++;
        if (CurrentStepIndex >= TaskData.steps.Count)
        {
            IsCompleted = true;
        }
    }
}
