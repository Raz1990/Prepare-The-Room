using UnityEngine;

public class SlidingWindow : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private float range = 3f;
    [SerializeField] private ItemType requiredItem = ItemType.None;

    [Header("UI Prompt Colors")]
    [SerializeField] private Color inputColor = new Color(1f, 0.843f, 0f); // Gold (#FFD700)
    [SerializeField] private Color actionColor = new Color(0.435f, 0.875f, 0.455f); // Brighter Green (#6FDF74)
    [SerializeField] private Color objectColor = new Color(0.705f, 1f, 1f); // Bright teal (#B4FFFF)


    [Header("Animation")]
    [SerializeField] private Animator windowAnimator;

    [Header("Audio")]
    [SerializeField] private AudioClip closeSFX;

    private bool isClosed = false;
    private string inputHex;
    private string actionHex;
    private string objectHex;
    private static readonly int CloseTriggerHash = Animator.StringToHash("Close");

    public float InteractionRange => range;
    public ItemType RequiredItem => requiredItem;

    void Awake()
    {
        if (windowAnimator == null) windowAnimator = GetComponent<Animator>();

        ConvertColorToHexString();
    }

    public string GetPromptText()
    {
        if (isClosed) return string.Empty;
        // Rich text coloring: Gold for LMB, Green for action
        return $"Press <color=#{inputHex}><b>[LMB]</b></color> to <color=#{actionHex}><b>Close</b></color> the <color=#{objectHex}><b>Window</b></color>";
    }

    public bool CanInteract(ItemType heldItem)
    {
        // One-way interaction: can only be interacted with if not already closed
        return !isClosed;
    }

    public void Interact(ItemType heldItem)
    {
        if (!CanInteract(heldItem)) return;

        ExecuteCloseWindow();
    }

    private void ConvertColorToHexString()
    {
        // Converts Unity Color to 6-digit RGB Hex string for TextMeshPro rich text
        inputHex = ColorUtility.ToHtmlStringRGB(inputColor);
        actionHex = ColorUtility.ToHtmlStringRGB(actionColor);
        objectHex = ColorUtility.ToHtmlStringRGB(objectColor);
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