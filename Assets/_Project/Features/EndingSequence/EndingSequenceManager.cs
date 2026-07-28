using UnityEngine;
using UnityEngine.Playables;

public class EndingSequenceManager : MonoBehaviour
{
    [Header("Cutscene References")]
    [SerializeField] private GameObject teacherGameObject;
    [SerializeField] private PlayableDirector cutsceneDirector;

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

        // 2. Start cutscene timeline
        if (cutsceneDirector != null)
        {
            cutsceneDirector.Play();
        }

        Debug.Log("[EndingSequenceManager] Visual blink complete. Teacher active & Cutscene running!");
    }
}