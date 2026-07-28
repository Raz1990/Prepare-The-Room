using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    private List<TaskProgress> activeTasks = new List<TaskProgress>();
    private List<TaskProgress> completedTasks = new List<TaskProgress>();
    private bool hasTriggeredAllTasksCompleted = false;

    void Awake()
    {
        // Manager starts clean. Tasks are dynamically supplied via TaskSequenceInjector or Quest Providers.
        activeTasks.Clear();
        completedTasks.Clear();
        hasTriggeredAllTasksCompleted = false;
    }

    void OnEnable()
    {
        GameEvents.OnItemPickedUp += HandleItemPickedUp;
        GameEvents.OnItemPlaced += HandleItemPlaced;
        GameEvents.OnActionCompleted += HandleActionCompleted;
    }

    void OnDisable()
    {
        GameEvents.OnItemPickedUp -= HandleItemPickedUp;
        GameEvents.OnItemPlaced -= HandleItemPlaced;
        GameEvents.OnActionCompleted -= HandleActionCompleted;
    }

    // Public getters for the UI controller when it initializes
    public List<TaskProgress> GetActiveTasks() => activeTasks;
    public List<TaskProgress> GetCompletedTasks() => completedTasks;

    /// <summary>
    /// Injects a single TaskSO into the active task queue.
    /// </summary>
    public void AddTask(TaskSO newTask)
    {
        if (newTask == null) return;

        activeTasks.Add(new TaskProgress(newTask));
        NotifyTaskCountChanged();

        Debug.Log($"<color=cyan>[TaskManager] Dynamically Injected Task:</color> '{newTask.taskName}'");
    }

    /// <summary>
    /// Injects a batch of TaskSO assets into the active task queue.
    /// </summary>
    public void AddTasks(List<TaskSO> newTasks)
    {
        if (newTasks == null) return;

        int addedCount = 0;
        foreach (TaskSO task in newTasks)
        {
            if (task != null)
            {
                activeTasks.Add(new TaskProgress(task));
                addedCount++;
            }
        }

        if (addedCount > 0)
        {
            NotifyTaskCountChanged();
            Debug.Log($"<color=cyan>[TaskManager] Dynamically Added {addedCount} Tasks.</color> Active total: {activeTasks.Count}");
        }
    }

    private void HandleItemPickedUp(ItemSO item)
    {
        EvaluateStepProgress(TaskActionType.PickupItem, item);
    }

    private void HandleItemPlaced(ItemSO item)
    {
        EvaluateStepProgress(TaskActionType.PlaceItem, item);
    }

    private void HandleActionCompleted(ActionID actionID)
    {
        EvaluateStepProgress(TaskActionType.Interaction, null, actionID);
    }

    private void EvaluateStepProgress(TaskActionType actionType, ItemSO item, ActionID actionID = default)
    {
        // Loop backward so we can safely move/remove items from activeTasks
        for (int i = activeTasks.Count - 1; i >= 0; i--)
        {
            TaskProgress progress = activeTasks[i];
            TaskStep currentStep = progress.CurrentStep;

            if (currentStep == null || currentStep.actionTypeToComplete != actionType) continue;

            bool stepMatched = false;

            switch (actionType)
            {
                case TaskActionType.PickupItem:
                case TaskActionType.PlaceItem:
                    if (currentStep.targetItem != null && currentStep.targetItem == item)
                    {
                        stepMatched = true;
                    }
                    break;

                case TaskActionType.Interaction:
                    if (currentStep.targetActionID == actionID)
                    {
                        stepMatched = true;
                    }
                    break;
            }

            if (stepMatched)
            {
                string completedStepDesc = currentStep.stepDescription;
                progress.AdvanceStep();

                Debug.Log($"<color=yellow>[TaskManager] STEP COMPLETED in '{progress.TaskData.taskName}':</color> {completedStepDesc}");

                if (progress.IsCompleted)
                {
                    // Move from active to completed list
                    activeTasks.RemoveAt(i);
                    completedTasks.Add(progress);

                    NotifyTaskCountChanged();

                    Debug.Log($"<color=green>[TaskManager] TASK COMPLETE:</color> '{progress.TaskData.taskName}'!");

                    GameEvents.TriggerTaskCompleted(progress.TaskData);

                    CheckAllTasksCompleted();
                }
                else
                {
                    Debug.Log($"[TaskManager] Next active step for '{progress.TaskData.taskName}': {progress.CurrentStep.stepDescription}");

                    GameEvents.TriggerTaskStepAdvanced(progress.TaskData, completedStepDesc, progress.CurrentStep.stepDescription);
                }
            }
        }
    }

    private void CheckAllTasksCompleted()
    {
        // Guard to not trigger further completed tasks event if the active tasks aren't part of the regular process 
        if (hasTriggeredAllTasksCompleted) return;

        if (activeTasks.Count == 0 && completedTasks.Count > 0)
        {
            hasTriggeredAllTasksCompleted = true;
            GameEvents.TriggerAllTasksCompleted();
            Debug.Log("<color=cyan><b>[TaskManager] ALL MISSIONS COMPLETED! Ready for Ending Sequence.</b></color>");
        }
    }

    private void NotifyTaskCountChanged()
    {
        GameEvents.TriggerActiveTasksAmountChanged(activeTasks.Count);
    }
}