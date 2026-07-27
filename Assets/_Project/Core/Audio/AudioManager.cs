using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Global Task Cues")]
    [Tooltip("Played whenever a step within an active task is completed.")]
    [SerializeField] private AudioClip stepCompletedSFX;

    [Tooltip("Played when a single task is fully completed.")]
    [SerializeField] private AudioClip taskCompletedSFX;

    [Tooltip("Played when all active tasks in the manager are cleared.")]
    [SerializeField] private AudioClip allTasksCompletedSFX;

    public static event Action<AudioClip, float> OnPlaySFXRequested;

    void OnEnable()
    {
        GameEvents.OnItemPickedUp += HandleItemPickedUp;
        GameEvents.OnItemPlaced += HandleItemPlaced;
        OnPlaySFXRequested += PlaySFX;

        GameEvents.OnTaskStepAdvanced += HandleStepAdvanced;
        GameEvents.OnTaskCompleted += HandleTaskCompleted;
        GameEvents.OnAllTasksCompleted += HandleAllTasksCompleted;
    }

    void OnDisable()
    {
        GameEvents.OnItemPickedUp -= HandleItemPickedUp;
        GameEvents.OnItemPlaced -= HandleItemPlaced;
        OnPlaySFXRequested -= PlaySFX;

        GameEvents.OnTaskStepAdvanced -= HandleStepAdvanced;
        GameEvents.OnTaskCompleted -= HandleTaskCompleted;
        GameEvents.OnAllTasksCompleted -= HandleAllTasksCompleted;
    }

    public static void TriggerPlaySFX(AudioClip clip, float volume = 1f)
    {
        OnPlaySFXRequested?.Invoke(clip, volume);
    }

    private void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // ==========================================
    // Event Handlers
    // ==========================================

    private void HandleItemPickedUp(ItemSO item)
    {
        if (item != null && item.pickupSound != null)
        {
            PlaySFX(item.pickupSound);
        }
    }

    private void HandleItemPlaced(ItemSO item)
    {
        if (item != null && item.placementSound != null)
        {
            PlaySFX(item.placementSound);
        }
    }

    private void HandleStepAdvanced(TaskSO _, string __, string ___)
    {
        PlaySFX(stepCompletedSFX);
    }

    private void HandleTaskCompleted(TaskSO _)
    {
        PlaySFX(taskCompletedSFX);
    }

    private void HandleAllTasksCompleted()
    {
        PlaySFX(allTasksCompletedSFX);
    }
}