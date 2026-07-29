using UnityEngine;
using UnityEngine.Playables;

public class EndingSequenceStarter : MonoBehaviour
{
    [Header("Cutscene References")]
    [SerializeField] private GameObject teacherGameObject;
    [Tooltip("The group of kids that should be deactivated when the cutscene starts.")]
    [SerializeField] private GameObject kidsGameObject;
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

        // 2. Deactivate the kids group
        DeactivateKids();

        // 3. Start cutscene timeline
        if (cutsceneDirector != null)
        {
            cutsceneDirector.Play();
        }

        Debug.Log("[EndingSequenceManager] Visual blink complete. Teacher active, Kids hidden & Cutscene running!");
    }

    private void DeactivateKids()
    {
        if (kidsGameObject != null)
        {
            kidsGameObject.SetActive(false);
        }
    }
}