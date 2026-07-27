using TMPro;
using UnityEngine;

public class Whiteboard : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float range = 3f;
    [SerializeField] private ItemType requiredItem = ItemType.Eraser;

    [Header("UI Prompt Colors")]
    [SerializeField] private Color inputColor = ColorsCenter.Gold;
    [SerializeField] private Color actionColor = ColorsCenter.LightGreen;
    [SerializeField] private Color objectColor = ColorsCenter.Teal;

    [Header("Animation")]
    [SerializeField] private Animator eraserAnimator;

    [Header("Audio")]
    [SerializeField] private AudioClip wipeSFX;

    private bool isCleaned = false;

    private static readonly int CleanTriggerHash = Animator.StringToHash("Clean");

    public TMP_SpriteAsset SpriteAsset => null;
    public float InteractionRange => range;
    public ItemType RequiredItem => requiredItem;

    public string GetPromptText()
    {
        if (isCleaned) return string.Empty;

        return PromptFormatter.BuildPrompt("LMB", "Clean", "Whiteboard", inputColor, actionColor, objectColor);
    }

    public bool CanInteract()
    {
        return !isCleaned;
    }

    public void Interact(ItemType currentItem)
    {
        if (!CanInteract()) return;

        bool hasEraser = ItemManager.Instance != null &&
                         ItemManager.Instance.CurrentHeldItemData != null &&
                         ItemManager.Instance.CurrentHeldItemData.itemID == requiredItem;

        if (!hasEraser)
        {
            // Broadcast domain event — Whiteboard knows nothing about UI
            GameEvents.TriggerInteractionFeedback("I'll need an eraser for that");
            return;
        }

        ExecuteCleanBoard();
    }

    private void ExecuteCleanBoard()
    {
        isCleaned = true;

        if (eraserAnimator != null)
        {
            eraserAnimator.enabled = true; // Turn it on ONLY when animation needs to play!
            eraserAnimator.SetTrigger(CleanTriggerHash);
        }

        AudioManager.TriggerPlaySFX(wipeSFX);

        WhiteboardEvents.TriggerWhiteboardCleaningStarted();
    }

    // Invoked via Animation Event on final keyframe of wiping clip
    public void OnCleaningAnimationEnd()
    {
        if (eraserAnimator != null)
        {
            eraserAnimator.enabled = false;
        }

        GameEvents.TriggerActionCompleted(ActionID.CleanWhiteboard);
    }
}