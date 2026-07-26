using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;

public class SlidingWindow : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float range = 3f;
    [SerializeField] private ItemType requiredItem = ItemType.None;

    [Header("UI Prompt Colors")]
    [SerializeField] private Color inputColor = ColorsCenter.Gold;
    [SerializeField] private Color actionColor = ColorsCenter.LightGreen;
    [SerializeField] private Color objectColor = ColorsCenter.Teal;


    [Header("Animation")]
    [SerializeField] private Animator windowAnimator;

    [Header("Audio")]
    [SerializeField] private AudioClip closeSFX;

    private bool isClosed = false;
    private static readonly int CloseTriggerHash = Animator.StringToHash("Close");

    public TMP_SpriteAsset SpriteAsset => null;
    public float InteractionRange => range;
    public ItemType RequiredItem => requiredItem;

    void Awake()
    {
        if (windowAnimator == null) windowAnimator = GetComponent<Animator>();
    }

    public string GetPromptText()
    {
        if (isClosed) return string.Empty;

        return PromptFormatter.BuildPrompt("LMB", "Close", "Window", inputColor, actionColor, objectColor);
    }

    public bool CanInteract(ItemType _)
    {
        // One-way interaction: can only be interacted with if not already closed
        return !isClosed;
    }

    public void Interact(ItemType _)
    {
        ExecuteCloseWindow();
    }

    private void ExecuteCloseWindow()
    {
        isClosed = true;

        if (windowAnimator != null)
        {
            windowAnimator.SetTrigger(CloseTriggerHash);
        }

        if (closeSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(closeSFX);
        }
    }

    // Callback method invoked by Unity Animation Event on the last keyframe of the closing animation
    public void OnWindowClosedAnimationEnd()
    {
        GameEvents.TriggerTaskCompleted();
    }
}