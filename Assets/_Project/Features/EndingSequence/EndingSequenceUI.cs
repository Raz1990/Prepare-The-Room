using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Playables;
using TMPro;
using System.Linq;

public class EndingSequenceUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private PlayableDirector cutsceneDirector;

    [Header("Camera & Controls")]
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private MonoBehaviour mouseLookScript;

    [Tooltip("Reference to the URP Volume component that controls post-processing effects. Used for the blur")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField][Range(0f, 1f)] private float targetBlurIntensity = 0.3f;

    [Header("UI Panels & Containers")]
    [SerializeField] private CanvasGroup reportCardCanvasGroup;
    [Tooltip("Drag your ReportTaskCardUI Prefab Variant here!")]
    [SerializeField] private TaskCardUI taskCardPrefab;
    [SerializeField] private Transform completedTasksContainer;
    [SerializeField] private Image aPlusStampImage;
    [SerializeField] private GameObject buttonsPanel;

    [Header("Checkmark Data & Audio Clips")]
    [SerializeField] private List<CheckmarkSO> checkmarkDataList = new List<CheckmarkSO>();
    [SerializeField] private AudioClip stampSFX;
    [SerializeField] private AudioSource cheersAudioSource;
    [SerializeField][Range(0f, 1f)] private float cheersReducedVolume = 0.2f;

    [Header("Sequence Timing")]
    [SerializeField] private float delayBetweenCheckmarks = 0.5f;
    [SerializeField] private float delayBeforeStamp = 3.0f;
    [SerializeField] private float delayBeforeButtons = 1.0f;

    private List<TaskCardUI> spawnedCompletedCards = new List<TaskCardUI>();

    void Awake()
    {
        if (taskManager == null)
        {
            taskManager = FindFirstObjectByType<TaskManager>();
        }

        InitializeUIState();
    }

    void OnEnable()
    {
        if (cutsceneDirector != null)
        {
            cutsceneDirector.stopped += HandleCutsceneStopped;
        }
    }

    void OnDisable()
    {
        if (cutsceneDirector != null)
        {
            cutsceneDirector.stopped -= HandleCutsceneStopped;
        }
    }

    private void HandleCutsceneStopped(PlayableDirector director)
    {
        StartEndingSequence();
    }

    /// <summary>
    /// Master entry point called when the cutscene hands control over to the end sequence.
    /// </summary>
    public void StartEndingSequence()
    {
        StartCoroutine(EndingSequenceRoutine());
    }

    private IEnumerator EndingSequenceRoutine()
    {
        UnlockCursor();
        LockInputAndCamera();
        ApplyScreenBlur(targetBlurIntensity);

        // Instantiate all task line variants at once with text ready
        PopulateCompletedTasksPanel();

        // Soften background audio
        LowerCheersAudioVolume();

        // Turn on CanvasGroup and GameObject
        ShowReportCard();

        yield return new WaitForSeconds(0.5f);

        // Reveal checkmarks sequentially on each spawned variant card
        yield return StartCoroutine(AnimateCheckmarksSequenceRoutine());

        yield return new WaitForSeconds(delayBeforeStamp);

        ApplyAPlusStamp();

        yield return new WaitForSeconds(delayBeforeButtons);

        ShowButtonsPanel();
    }

    // ==========================================
    // Modular Step Functions
    // ==========================================

    private void InitializeUIState()
    {
        if (reportCardCanvasGroup != null)
        {
            reportCardCanvasGroup.alpha = 0f;
            reportCardCanvasGroup.blocksRaycasts = false;
            reportCardCanvasGroup.gameObject.SetActive(false);
        }

        if (aPlusStampImage != null) aPlusStampImage.gameObject.SetActive(false);
        if (buttonsPanel != null) buttonsPanel.SetActive(false);
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Permanently disable cursor relocking in StarterAssets
        StarterAssets.StarterAssetsInputs starterInputs = FindFirstObjectByType<StarterAssets.StarterAssetsInputs>();
        if (starterInputs != null)
        {
            starterInputs.allowCursorLock = false;
            starterInputs.SetCursorState(false);
        }
    }

    private void LockInputAndCamera()
    {
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (mouseLookScript != null) mouseLookScript.enabled = false;
    }

    private void ApplyScreenBlur(float intensity)
    {
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            if (postProcessVolume.profile.TryGet(out DepthOfField dof))
            {
                dof.active = true;
                dof.gaussianStart.value = 0.1f * (1f - intensity);
                dof.gaussianEnd.value = 10f * (1f - intensity);
            }
        }
    }

    private void PopulateCompletedTasksPanel()
    {
        ClearExistingCards();

        if (taskManager == null)
        {
            taskManager = FindFirstObjectByType<TaskManager>();
            if (taskManager == null) return;
        }

        // Get completed tasks and filter out any task where showInSummary is false
        List<TaskProgress> completedTaskProgressList = taskManager.GetCompletedTasks()
            ?.Where(t => t != null && t.TaskData != null && t.TaskData.showInSummary)
            .ToList();

        if (completedTaskProgressList == null || completedTasksContainer == null || taskCardPrefab == null) return;

        foreach (TaskProgress progress in completedTaskProgressList)
        {
            if (progress == null || progress.TaskData == null) continue;

            TaskCardUI newCard = Instantiate(taskCardPrefab, completedTasksContainer);
            SetupCompletedTaskCard(newCard, progress.TaskData);
            spawnedCompletedCards.Add(newCard);
        }
    }

    private void SetupCompletedTaskCard(TaskCardUI card, TaskSO taskData)
    {
        if (card == null || taskData == null) return;

        // Set task Title text
        TextMeshProUGUI titleText = card.GetComponentInChildren<TextMeshProUGUI>();
        if (titleText != null)
        {
            titleText.text = taskData.taskName;
        }

        // Deactivate the Checkmark child on the variant initially
        Image checkmarkImage = GetCardCheckmarkImage(card);
        if (checkmarkImage != null)
        {
            checkmarkImage.gameObject.SetActive(false);
        }
    }

    private void ClearExistingCards()
    {
        foreach (TaskCardUI card in spawnedCompletedCards)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        spawnedCompletedCards.Clear();
    }

    private void ShowReportCard()
    {
        if (reportCardCanvasGroup != null)
        {
            reportCardCanvasGroup.gameObject.SetActive(true);
            reportCardCanvasGroup.alpha = 1f;
            reportCardCanvasGroup.blocksRaycasts = true;
        }
    }

    private IEnumerator AnimateCheckmarksSequenceRoutine()
    {
        for (int i = 0; i < spawnedCompletedCards.Count; i++)
        {
            DisplayCheckmarkForCard(i);
            yield return new WaitForSeconds(delayBetweenCheckmarks);
        }
    }

    private void DisplayCheckmarkForCard(int index)
    {
        if (index < 0 || index >= spawnedCompletedCards.Count) return;

        TaskCardUI card = spawnedCompletedCards[index];
        if (card == null) return;

        CheckmarkSO data = GetCheckmarkDataSafely(index);
        Image checkmarkImage = GetCardCheckmarkImage(card);

        if (checkmarkImage != null)
        {
            if (data != null && data.checkmarkSprite != null)
            {
                checkmarkImage.sprite = data.checkmarkSprite;
            }

            checkmarkImage.gameObject.SetActive(true);
        }

        if (data != null && data.checkmarkSFX != null)
        {
            AudioManager.TriggerPlaySFX(data.checkmarkSFX);
        }
    }

    /// <summary>
    /// Locates the Checkmark child Image inside the ReportTaskCardUI Prefab Variant.
    /// </summary>
    private Image GetCardCheckmarkImage(TaskCardUI card)
    {
        if (card == null) return null;

        // 1. First look for child object explicitly named "Checkmark"
        Transform checkmarkTransform = card.transform.Find("Checkmark");
        if (checkmarkTransform != null)
        {
            return checkmarkTransform.GetComponent<Image>();
        }

        // 2. Fallback: Search child images and skip the root card background image
        Image[] childImages = card.GetComponentsInChildren<Image>(true);
        Image rootCardImage = card.GetComponent<Image>();

        foreach (Image img in childImages)
        {
            if (img != rootCardImage)
            {
                return img;
            }
        }

        return null;
    }

    private CheckmarkSO GetCheckmarkDataSafely(int index)
    {
        if (checkmarkDataList == null || checkmarkDataList.Count == 0) return null;

        int safeIndex = Mathf.Clamp(index, 0, checkmarkDataList.Count - 1);
        return checkmarkDataList[safeIndex];
    }

    private void ApplyAPlusStamp()
    {
        if (aPlusStampImage != null)
        {
            aPlusStampImage.gameObject.SetActive(true);
        }

        if (stampSFX != null)
        {
            AudioManager.TriggerPlaySFX(stampSFX);

            if (cheersAudioSource != null)
            {
                cheersAudioSource.PlayOneShot(stampSFX);
            }
        }
    }

    private void LowerCheersAudioVolume()
    {
        if (cheersAudioSource != null)
        {
            cheersAudioSource.volume = cheersReducedVolume;
        }
    }

    private void ShowButtonsPanel()
    {
        if (buttonsPanel != null)
        {
            buttonsPanel.SetActive(true);
        }
    }

    // ==========================================
    // UI Callbacks
    // ==========================================

    public void OnClickRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}