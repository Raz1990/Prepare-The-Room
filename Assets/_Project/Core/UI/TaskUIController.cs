using System.Collections.Generic;
using UnityEngine;

public class TaskUIController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;

    [Header("UI Setup")]
    [SerializeField] private GameObject tasksPanel;
    [SerializeField] private TaskCardUI taskCardPrefab;
    [SerializeField] private Transform cardContainer;

    private List<TaskCardUI> spawnedCards = new List<TaskCardUI>();

    void Awake()
    {
        if (taskManager == null)
        {
            taskManager = FindFirstObjectByType<TaskManager>();
        }
    }

    void Start()
    {
        InitializeCards();
    }

    void OnEnable()
    {
        GameEvents.OnTaskStepAdvanced += HandleStepAdvanced;
        GameEvents.OnTaskCompleted += HandleTaskCompleted;
        GameEvents.OnActiveTasksAmountChanged += HandleActiveTasksAmountChanged;
    }

    void OnDisable()
    {
        GameEvents.OnTaskStepAdvanced -= HandleStepAdvanced;
        GameEvents.OnTaskCompleted -= HandleTaskCompleted;
        GameEvents.OnActiveTasksAmountChanged -= HandleActiveTasksAmountChanged;

        // Unsubscribe from any remaining card events
        foreach (TaskCardUI card in spawnedCards)
        {
            if (card != null)
            {
                card.OnCardAnimationComplete -= HandleCardAnimationComplete;
            }
        }
    }

    public void InitializeCards()
    {
        foreach (TaskCardUI card in spawnedCards)
        {
            if (card != null)
            {
                card.OnCardAnimationComplete -= HandleCardAnimationComplete;
                Destroy(card.gameObject);
            }
        }

        spawnedCards.Clear();

        // Extra guard in case the TaskManager reference is not set in the inspector or found in Awake
        if (taskManager == null)
        {
            taskManager = FindFirstObjectByType<TaskManager>();

            if (taskManager == null)
            {
                Debug.LogWarning("[TaskUIController] TaskManager reference is missing!");
                return;
            }
        }

        List<TaskProgress> activeTasks = taskManager.GetActiveTasks();

        // Initial panel state on scene load based on starting tasks
        if (tasksPanel != null)
        {
            tasksPanel.SetActive(activeTasks.Count > 0);
        }

        foreach (TaskProgress progress in activeTasks)
        {
            CreateTaskCard(progress);
        }
    }
    private void CreateTaskCard(TaskProgress progress)
    {
        TaskCardUI newCard = Instantiate(taskCardPrefab, cardContainer);
        newCard.SetupCard(progress);
        newCard.OnCardAnimationComplete += HandleCardAnimationComplete;
        spawnedCards.Add(newCard);
    }

    private void HandleCardAnimationComplete()
    {
        // When a card finishes animating and destroying itself, check if all active cards are gone
        if (spawnedCards.Count == 0 && tasksPanel != null)
        {
            tasksPanel.SetActive(false);
        }
    }

    private void HandleActiveTasksAmountChanged(int count)
    {
        if (tasksPanel == null) return;
        
        bool hasTasks = count > 0;

        if (hasTasks)
        {
            tasksPanel.SetActive(true);

            if (spawnedCards.Count == 0)
            {
                InitializeCards();
            }
        }
    }

    private void HandleStepAdvanced(TaskSO task, string oldStepDesc, string newStepDesc)
    {
        TaskCardUI card = GetCardForTask(task);
        if (card != null)
        {
            card.AnimateStepComplete(oldStepDesc, newStepDesc);
        }
    }

    private void HandleTaskCompleted(TaskSO task)
    {
        TaskCardUI card = GetCardForTask(task);
        if (card != null)
        {
            // Remove from active tracking immediately so count reflects remaining active cards
            spawnedCards.Remove(card);
            card.AnimateTaskComplete();
        }
    }

    private TaskCardUI GetCardForTask(TaskSO task)
    {
        foreach (TaskCardUI card in spawnedCards)
        {
            if (card != null && card.AssociatedTask == task)
            {
                return card;
            }
        }
        return null;
    }
}