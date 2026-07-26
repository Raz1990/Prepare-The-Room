using System.Collections;
using UnityEngine;
using TMPro;

public class InteractionUIPrompt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRaycaster raycaster;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private float temporaryPromptTextDuration = 4.0f;

    private Coroutine feedbackCoroutine;
    private string activeFeedbackText;
    private IPromptable lastTarget;

    void Awake()
    {
        if (raycaster == null) raycaster = FindFirstObjectByType<PlayerRaycaster>();
    }

    void OnEnable()
    {
        GameEvents.OnInteractionFeedback += HandleInteractionFeedback;
    }

    void OnDisable()
    {
        GameEvents.OnInteractionFeedback -= HandleInteractionFeedback;
    }

    void Update()
    {
        UpdatePromptDisplay();
    }

    private void HandleInteractionFeedback(string message)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }

        feedbackCoroutine = StartCoroutine(TemporaryFeedbackRoutine(message, temporaryPromptTextDuration));
    }

    private IEnumerator TemporaryFeedbackRoutine(string message, float duration)
    {
        activeFeedbackText = message;
        yield return new WaitForSeconds(duration);
        activeFeedbackText = null;
        feedbackCoroutine = null;
    }

    private void UpdatePromptDisplay()
    {
        if (raycaster == null || promptPanel == null || promptText == null) return;

        IPromptable target = raycaster.GetCurrentPromptable();

        // Looking away or target switching immediately clears temporary feedback
        if (target != lastTarget)
        {
            if (feedbackCoroutine != null)
            {
                StopCoroutine(feedbackCoroutine);
                feedbackCoroutine = null;
                activeFeedbackText = null;
            }
            lastTarget = target;
        }

        if (target == null && string.IsNullOrEmpty(activeFeedbackText))
        {
            TogglePanelDisplay(false);
            return;
        }

        HandleTextDisplay(target);
    }

    private void HandleTextDisplay(IPromptable target)
    {
        // Active feedback temporarily overrides standard prompt display
        if (!string.IsNullOrEmpty(activeFeedbackText))
        {
            TogglePanelDisplay(true);
            promptText.text = activeFeedbackText;
            return;
        }

        if (target == null)
        {
            TogglePanelDisplay(false);
            return;
        }

        string text = target.GetPromptText();

        if (string.IsNullOrEmpty(text))
        {
            TogglePanelDisplay(false);
            return;
        }

        TogglePanelDisplay(true);
        promptText.spriteAsset = target.SpriteAsset;
        promptText.text = text;
    }

    private void TogglePanelDisplay(bool toggle)
    {
        if (promptPanel.activeSelf == toggle) return;
        promptPanel.SetActive(toggle);
    }
}