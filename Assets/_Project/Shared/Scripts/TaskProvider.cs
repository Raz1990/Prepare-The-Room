using System.Collections.Generic;
using UnityEngine;

public class TaskProvider : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TaskManager taskManager;

    [Header("Tasks Configuration")]
    [Tooltip("List of TaskSO assets to provide to the TaskManager.")]
    [SerializeField] private List<TaskSO> tasksToProvide = new List<TaskSO>();

    [Header("Trigger Options")]
    [Tooltip("If checked, listens to GameEvents.OnAllTasksCompleted and automatically provides tasks.")]
    [SerializeField] private bool autoProvideOnAllCompleted = false;

    [Header("Audio (Optional)")]
    [Tooltip("Optional sound effect to play when these tasks are injected via AudioManager.")]
    [SerializeField] private AudioClip provideTaskSound;
    [SerializeField][Range(0f, 1f)] private float provideTaskSoundVolume = 1f;

    private bool hasProvided = false;

    void OnEnable()
    {
        if (autoProvideOnAllCompleted)
        {
            GameEvents.OnAllTasksCompleted += HandleAllTasksCompleted;
        }
    }

    void OnDisable()
    {
        if (autoProvideOnAllCompleted)
        {
            GameEvents.OnAllTasksCompleted -= HandleAllTasksCompleted;
        }
    }

    private void HandleAllTasksCompleted()
    {
        ProvideTasks();
    }

    /// <summary>
    /// Provides configured TaskSO assets to the TaskManager.
    /// Can be called directly by Interactables, Dialogue events, Timeline Signals, or GameEvents.
    /// </summary>
    public void ProvideTasks()
    {
        if (hasProvided) return;

        if (taskManager == null)
        {
            taskManager = Object.FindFirstObjectByType<TaskManager>();
        }

        if (taskManager == null)
        {
            Debug.LogError($"[TaskProvider] No TaskManager found in scene by '{gameObject.name}'!");
            return;
        }

        if (tasksToProvide == null || tasksToProvide.Count == 0)
        {
            Debug.LogWarning($"[TaskProvider] No TaskSO assets configured to provide on '{gameObject.name}'.");
            return;
        }

        taskManager.AddTasks(tasksToProvide);
        hasProvided = true;

        PlayProvideSound();
    }

    private void PlayProvideSound()
    {
        if (provideTaskSound == null) return;

        AudioManager.TriggerPlaySFX(provideTaskSound, provideTaskSoundVolume);
    }
}