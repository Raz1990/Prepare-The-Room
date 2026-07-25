using UnityEngine;
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

        string currentPrompt = raycaster.GetCurrentPromptText();

        if (string.IsNullOrEmpty(currentPrompt))
        {
            promptPanel.SetActive(false);
        }
        else
        {
            promptText.text = currentPrompt;
            promptPanel.SetActive(true);
        }
    }
}