using UnityEngine;
using StarterAssets;

public class BeginningSequenceUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject introPanel;

    [Header("Task System")]
    [SerializeField] private TaskProvider initialTaskProvider;

    [Header("Controls & Camera")]
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private MonoBehaviour mouseLookScript;
    [SerializeField] private StarterAssetsInputs starterInputs;

    private void Start()
    {
        InitializeIntroState();
    }

    private void InitializeIntroState()
    {
        // 1. Ensure intro panel is visible
        if (introPanel != null)
        {
            introPanel.SetActive(true);
        }

        // 2. Lock movement & camera controls
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (mouseLookScript != null) mouseLookScript.enabled = false;

        // 3. Unlock cursor for UI interaction
        if (starterInputs == null)
        {
            starterInputs = FindFirstObjectByType<StarterAssetsInputs>();
        }

        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = false;
            starterInputs.SetCursorState(false);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// Hook this function up to your "Let's do this" Button's OnClick event!
    /// </summary>
    public void OnClickStartGame()
    {
        // 1. Dismiss Intro UI paper
        if (introPanel != null)
        {
            introPanel.SetActive(false);
        }

        // 2. Restore full gameplay controls & lock cursor
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (mouseLookScript != null) mouseLookScript.enabled = true;

        if (starterInputs != null)
        {
            starterInputs.cursorInputForLook = true;
            starterInputs.SetCursorState(true);
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 3. Inject initial tasks into TaskManager
        if (initialTaskProvider != null)
        {
            initialTaskProvider.ProvideTasks();
        }
        else
        {
            Debug.LogWarning("[BeginningSequenceUI] Initial TaskProvider reference missing in Inspector!");
        }
    }
}