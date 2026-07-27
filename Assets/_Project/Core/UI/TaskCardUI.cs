using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TaskCardUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI stepText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform stepRectTransform;

    [Header("Animation Timings & Tweaks")]
    [Tooltip("How long the crossed-out step text stays visible before sliding off-screen.")]
    [SerializeField] private float stepStrikethroughDelay = 0.5f;

    [Tooltip("Duration of the text slide animation during step swaps.")]
    [SerializeField] private float stepSlideDuration = 0.25f;

    [Tooltip("Horizontal distance (in UI pixels) text slides out and in.")]
    [SerializeField] private float stepSlideDistance = 60f;

    [Tooltip("How long a fully completed task stays on screen before fading and sliding out.")]
    [SerializeField] private float taskCompleteDisplayDuration = 1f;

    [Tooltip("Duration of the entire card sliding off-screen upon task completion.")]
    [SerializeField] private float taskSlideOutDuration = 0.35f;

    [Tooltip("Horizontal distance the card slides left when exiting.")]
    [SerializeField] private float taskSlideOutDistance = 400f;

    // Event fired when the card finishes its exit animation
    public event Action OnCardAnimationComplete;

    private TaskSO associatedTask;
    private Coroutine currentStepCoroutine;
    private RectTransform cardRectTransform;
    private Vector2 defaultStepLocalPos;

    void Awake()
    {
        cardRectTransform = GetComponent<RectTransform>();
        if (stepRectTransform != null)
        {
            defaultStepLocalPos = stepRectTransform.anchoredPosition;
        }
    }

    public TaskSO AssociatedTask => associatedTask;

    public void SetupCard(TaskProgress progress)
    {
        associatedTask = progress.TaskData;
        titleText.text = associatedTask.taskName;

        if (progress.CurrentStep != null)
        {
            stepText.text = $"- {progress.CurrentStep.stepDescription}";
        }
        else
        {
            stepText.text = string.Empty;
        }
    }

    public void AnimateStepComplete(string oldStepDesc, string newStepDesc)
    {
        if (currentStepCoroutine != null)
        {
            StopCoroutine(currentStepCoroutine);
        }
        currentStepCoroutine = StartCoroutine(StepTransitionRoutine(oldStepDesc, newStepDesc));
    }

    public void AnimateTaskComplete()
    {
        StartCoroutine(TaskCompleteRoutine());
    }

    private IEnumerator StepTransitionRoutine(string oldStepDesc, string newStepDesc)
    {
        // 1. Cross out the step description that was just completed
        stepText.text = $"- <s>{oldStepDesc}</s>";
        yield return new WaitForSeconds(stepStrikethroughDelay);

        // 2. Slide old text out to the left
        Vector2 startPos = defaultStepLocalPos;
        Vector2 leftExitPos = startPos - new Vector2(stepSlideDistance, 0f);

        float elapsed = 0f;
        while (elapsed < stepSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / stepSlideDuration;
            stepRectTransform.anchoredPosition = Vector2.Lerp(startPos, leftExitPos, t);
            yield return null;
        }

        // 3. Swap text to new step description & position it off-screen to the right
        stepText.text = $"- {newStepDesc}";
        Vector2 rightEntryPos = startPos + new Vector2(stepSlideDistance, 0f);
        stepRectTransform.anchoredPosition = rightEntryPos;

        // 4. Slide new text smoothly into default position
        elapsed = 0f;
        while (elapsed < stepSlideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / stepSlideDuration;
            stepRectTransform.anchoredPosition = Vector2.Lerp(rightEntryPos, startPos, t);
            yield return null;
        }

        stepRectTransform.anchoredPosition = defaultStepLocalPos;
    }

    private IEnumerator TaskCompleteRoutine()
    {
        // 1. Cross out both headline and remaining step text
        titleText.text = $"<s>{associatedTask.taskName}</s>";
        if (!stepText.text.StartsWith("<s>"))
        {
            stepText.text = $"<s>{stepText.text}</s>";
        }

        yield return new WaitForSeconds(taskCompleteDisplayDuration);

        // 2. Slide entire card out to the left while fading alpha
        Vector2 startPos = cardRectTransform.anchoredPosition;
        Vector2 targetPos = startPos - new Vector2(taskSlideOutDistance, 0f);

        float elapsed = 0f;
        while (elapsed < taskSlideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / taskSlideOutDuration;

            cardRectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }
            yield return null;
        }

        // 3. Notify UI Controller that exit animation is complete before destruction
        OnCardAnimationComplete?.Invoke();

        // 4. Destroy card; ContentSizeFitter + VerticalLayoutGroup on parent shrinks panel automatically
        Destroy(gameObject);
    }
}