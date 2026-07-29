using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class EndingSequenceStarter : MonoBehaviour
{
    [Header("Cutscene References")]
    [SerializeField] private GameObject teacherGameObject;
    [Tooltip("The group of kids that should be deactivated when the cutscene starts.")]
    [SerializeField] private GameObject kidsGameObject;
    [SerializeField] private PlayableDirector cutsceneDirector;

    [Header("Ambiance & Audio Control")]
    [Tooltip("Parent transform containing ambient audio sources (e.g. 'Ambiance' group).")]
    [SerializeField] private Transform ambianceGroupParent;

    [Tooltip("Standalone AudioSource references outside the main group (e.g. Wall Clock).")]
    [SerializeField] private List<AudioSource> standaloneAudioSources = new List<AudioSource>();

    void OnEnable()
    {
        ChairInteractable.OnPlayerSeated += HandlePlayerSeated;
    }

    void OnDisable()
    {
        ChairInteractable.OnPlayerSeated -= HandlePlayerSeated;
    }

    private void HandlePlayerSeated()
    {
        // Player is fully seated in the chair. Trigger the visual boundary blink!
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.TriggerBlink(OnScreenBlinkPeak);
        }
        else
        {
            OnScreenBlinkPeak();
        }
    }

    private void OnScreenBlinkPeak()
    {
        // 1. Activate pre-placed teacher while screen is black
        if (teacherGameObject != null)
        {
            teacherGameObject.SetActive(true);
        }

        // 2. Deactivate the kids group
        DeactivateKids();

        // 3. Silence all classroom ambient audio sources
        SilenceBackgroundAudio();

        // 4. Start cutscene timeline
        if (cutsceneDirector != null)
        {
            cutsceneDirector.Play();
        }

        Debug.Log("[EndingSequenceManager] Visual blink complete. Teacher active, Kids hidden, Ambiance silenced & Cutscene running!");
    }

    private void DeactivateKids()
    {
        if (kidsGameObject != null)
        {
            kidsGameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Silences both grouped ambient audio sources and any standalone audio sources (e.g., Wall Clock).
    /// </summary>
    private void SilenceBackgroundAudio()
    {
        // 1. Handle the child group (Ambiance + Murmuring, etc.)
        if (ambianceGroupParent != null)
        {
            AudioSource[] groupSources = ambianceGroupParent.GetComponentsInChildren<AudioSource>(true);
            foreach (AudioSource source in groupSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                }
            }
        }

        // 2. Handle standalone sources (Wall Clock, etc.)
        foreach (AudioSource standaloneSource in standaloneAudioSources)
        {
            if (standaloneSource != null && standaloneSource.isPlaying)
            {
                standaloneSource.Stop();
            }
        }
    }
}