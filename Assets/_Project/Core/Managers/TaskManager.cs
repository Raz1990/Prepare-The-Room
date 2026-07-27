using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [Header("Tasks Configuration")]
    [SerializeField] private List<TaskSO> tasks = new List<TaskSO>();

    private List<TaskProgress> activeTasks = new List<TaskProgress>();
    private List<TaskProgress> completedTasks = new List<TaskProgress>();

    void Awake()
    {
        InitializeTasks();
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

    private void InitializeTasks()
    {
        activeTasks.Clear();
        completedTasks.Clear();

        foreach (TaskSO task in tasks)
        {
            if (task != null)
            {
                activeTasks.Add(new TaskProgress(task));
            }
        }

        NotifyTaskCountChanged();

        Debug.Log($"[TaskManager] Initialized {activeTasks.Count} active tasks.");
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
        if (activeTasks.Count == 0 && completedTasks.Count > 0)
        {
            GameEvents.TriggerAllTasksCompleted();
            Debug.Log("<color=cyan><b>[TaskManager] ALL MISSIONS COMPLETED! Ready for Ending Sequence.</b></color>");
        }
    }

    private void NotifyTaskCountChanged()
    {
        GameEvents.TriggerActiveTasksAmountChanged(activeTasks.Count);
    }
}