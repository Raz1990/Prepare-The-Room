using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUIPrompt : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerRaycaster raycaster;
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;

    void Awake()
    {
        if (raycaster == null) raycaster = FindFirstObjectByType<PlayerRaycaster>();
    }

    void Update()
    {
        UpdatePromptDisplay();
    }

    private void UpdatePromptDisplay()
    {
        if (raycaster == null || promptPanel == null || promptText == null) return;

        IPromptable target = raycaster.GetCurrentPromptable();

        if (target == null)
        {
            TogglePanelDisplay(false);
            return;
        }

        HandleTextDisplay(target);
    }

    private void HandleTextDisplay(IPromptable target)
    {
        string text = target.GetPromptText();

        if (string.IsNullOrEmpty(text))
        {
            TogglePanelDisplay(false);
            return;
        }

        TogglePanelDisplay(true);
        promptText.text = text;
    }

    private void TogglePanelDisplay(bool toggle)
    {
        if (promptPanel.activeSelf == toggle) return;
        promptPanel.SetActive(toggle);
    }
}