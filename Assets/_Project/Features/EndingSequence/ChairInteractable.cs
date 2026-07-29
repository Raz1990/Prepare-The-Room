using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Unity.Cinemachine;

public class ChairInteractable : MonoBehaviour, IInteractable, IHighlightable
{
    public static event Action OnPlayerSeated;

    [Header("Interaction Settings")]
    [SerializeField] private float range = 3f;
    [SerializeField] private ActionID sitActionID = ActionID.Sit;

    [Header("UI Prompt Colors")]
    [SerializeField] private Color inputColor = ColorsCenter.Gold;
    [SerializeField] private Color actionColor = ColorsCenter.LightGreen;
    [SerializeField] private Color objectColor = ColorsCenter.Teal;

    [Header("Cinemachine Setup (v3.0+)")]
    [SerializeField] private CinemachineCamera sitVirtualCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;

    [Header("Player References")]
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private MonoBehaviour mouseLookScript;

    [Header("Visual Feedback")]
    [SerializeField] private Behaviour outline;

    private bool canInteractWithChair = false;
    private bool hasSatDown = false;

    public TMP_SpriteAsset SpriteAsset => null;
    public float InteractionRange => range;
    public ItemType RequiredItem => ItemType.None;

    void OnEnable()
    {
        GameEvents.OnAllTasksCompleted += HandleInitialTasksCompleted;
    }

    void OnDisable()
    {
        GameEvents.OnAllTasksCompleted -= HandleInitialTasksCompleted;
    }

    void Start()
    {
        // Auto-grab CinemachineBrain from Main Camera if not manually assigned in Inspector
        if (cinemachineBrain == null && Camera.main != null)
        {
            cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        }
    }

    private void HandleInitialTasksCompleted()
    {
        canInteractWithChair = true;

        int interactableLayer = LayerMask.NameToLayer("Interactable");
        gameObject.layer = interactableLayer;

        Highlight();
    }

    #region IHighlightable Implementation

    public void Highlight()
    {
        if (!CanInteract()) return;

        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    public void Unhighlight()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    #endregion

    #region IInteractable Implementation

    public string GetPromptText()
    {
        if (!CanInteract()) return string.Empty;

        return PromptFormatter.BuildPrompt("LMB", "Sit down", "", inputColor, actionColor, objectColor);
    }

    public bool CanInteract()
    {
        return canInteractWithChair && !hasSatDown;
    }

    public void Interact(ItemType _)
    {
        if (!CanInteract()) return;

        ExecuteSit();
    }

    #endregion

    private void ExecuteSit()
    {
        hasSatDown = true;
        Unhighlight();

        StartCoroutine(SitSequenceRoutine());
    }

    private IEnumerator SitSequenceRoutine()
    {
        // 1. Lock player movement and look controls
        LockInputAndCamera();

        // 2. Notify TaskManager that sitting action is complete
        GameEvents.TriggerActionCompleted(sitActionID);

        // 3. Switch camera priority
        if (sitVirtualCamera != null)
        {
            sitVirtualCamera.Priority = 20;
        }

        // 4. Wait dynamically until Cinemachine FINISHES blending completely
        if (cinemachineBrain != null)
        {
            // Wait 1 frame so Cinemachine can detect the priority change and initialize the blend
            yield return null;

            // Loop until Cinemachine's active blend is completely finished
            while (cinemachineBrain.IsBlending)
            {
                yield return null;
            }
        }

        // 5. Camera is 100% locked into position! Notify EndingSequenceManager
        OnPlayerSeated?.Invoke();
    }

    private void LockInputAndCamera()
    {
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (mouseLookScript != null) mouseLookScript.enabled = false;
    }
}