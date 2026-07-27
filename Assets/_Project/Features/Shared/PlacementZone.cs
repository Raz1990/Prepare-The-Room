using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlacementZone : MonoBehaviour, IPlacementZone, IHighlightable
{
    [Header("Slot Settings")]
    [SerializeField] private ItemSO targetItemData;
    [SerializeField] private float range = 2.0f;
    [SerializeField] private Transform targetPlacementTransform;

    [Header("Activation Settings")]
    [Tooltip("Action required to UNLOCK this zone")]
    [SerializeField] private ActionID prerequisiteActionID = ActionID.None;
    [Tooltip("Action broadcasted when item is PLACED here")]
    [SerializeField] private ActionID completedActionID = ActionID.None;

    [Header("Action")]
    [SerializeField] private string actionToPerform = "Place";

    [Header("Ghost Visual Reference")]
    [SerializeField] private GameObject ghostObject;

    [Header("UI Colors")]
    [SerializeField] private Color inputColor = ColorsCenter.Gold;
    [SerializeField] private Color actionColor = ColorsCenter.LightGreen;
    [SerializeField] private Color itemColor = ColorsCenter.Teal;

    [Header("References")]
    [SerializeField] private Outline outline;

    private bool isFilled = false;
    private bool isBeaconActive = false;
    private bool isUnlocked = true;
    private bool RequiresPrerequisite => prerequisiteActionID != ActionID.None;

    public TMP_SpriteAsset SpriteAsset => targetItemData != null ? targetItemData.spriteAsset : null;

    public float InteractionRange => range;

    public ItemType TargetItemType => targetItemData != null ? targetItemData.itemID : ItemType.None;

    public bool IsFilled => isFilled;

    void Awake()
    {
        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }

        if (targetPlacementTransform == null)
        {
            targetPlacementTransform = transform;
        }

        // Hide ghost mesh by default on start until player holds the item
        if (ghostObject != null)
        {
            ghostObject.SetActive(false);
        }

        // Lock placement zone by default if it depends on a prerequisite task
        if (RequiresPrerequisite)
        {
            isUnlocked = false;
        }
    }

    void OnEnable()
    {
        GameEvents.OnItemPickedUp += HandleItemPickedUp;
        GameEvents.OnItemPlaced += HandleItemPlaced;
        GameEvents.OnActionCompleted += HandleRequiredActionCompleted;
    }

    void OnDisable()
    {
        GameEvents.OnItemPickedUp -= HandleItemPickedUp;
        GameEvents.OnItemPlaced -= HandleItemPlaced;
        GameEvents.OnActionCompleted -= HandleRequiredActionCompleted;
    }

    private void HandleItemPickedUp(ItemSO item)
    {
        // Don't activate beacon if the zone is still locked by a prerequisite task
        if (!isUnlocked || isFilled || item == null || targetItemData == null) return;

        if (item.itemID == targetItemData.itemID)
        {
            ActivateBeacon();
        }
    }

    private void ActivateBeacon()
    {
        isBeaconActive = true;

        if (ghostObject != null)
        {
            ghostObject.SetActive(true);
        }

        Highlight();
    }

    private void HandleItemPlaced(ItemSO item)
    {
        if (item != null && targetItemData != null && item.itemID == targetItemData.itemID)
        {
            isBeaconActive = false;
            Unhighlight();

            if (ghostObject != null)
            {
                ghostObject.SetActive(false);
            }
        }
    }

    private void HandleRequiredActionCompleted(ActionID completedActionID)
    {
        if (!RequiresPrerequisite || completedActionID != prerequisiteActionID) return;

        isUnlocked = true;

        if (ItemManager.Instance != null &&
            ItemManager.Instance.CurrentHeldItemData != null &&
            targetItemData != null &&
            ItemManager.Instance.CurrentHeldItemData.itemID == targetItemData.itemID)
        {
            ActivateBeacon();
        }
    }

    public string GetPromptText()
    {
        if (!isUnlocked || isFilled) return string.Empty;

        string displayName = targetItemData != null ? targetItemData.itemName : "Item";
        Sprite icon = targetItemData != null ? targetItemData.icon : null;

        return PromptFormatter.BuildPrompt("E", actionToPerform, displayName, inputColor, actionColor, itemColor, icon);
    }

    public bool CanPlace(ItemType heldItem)
    {
        if (!isUnlocked || isFilled || targetItemData == null) return false;

        return heldItem == targetItemData.itemID;
    }

    public void PlaceItem(GameObject itemObject, Action onComplete)
    {
        if (!isUnlocked || isFilled) return;

        isFilled = true;
        isBeaconActive = false;

        GameEvents.TriggerItemPickedUp(ItemManager.Instance.CurrentHeldItemData);

        if (ghostObject != null)
        {
            ghostObject.SetActive(false);
        }

        Unhighlight();

        StartCoroutine(AnimateToSlot(itemObject, onComplete));
    }

    private IEnumerator AnimateToSlot(GameObject itemObject, Action onComplete)
    {
        itemObject.transform.SetParent(null);

        float duration = 0.25f;
        float elapsed = 0f;

        itemObject.transform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPlacementTransform.position, t);
            Quaternion currentRot = Quaternion.Lerp(startRot, targetPlacementTransform.rotation, t);

            itemObject.transform.SetPositionAndRotation(currentPos, currentRot);

            yield return null;
        }

        itemObject.transform.SetParent(targetPlacementTransform);
        itemObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        onComplete?.Invoke();

        if (completedActionID != ActionID.None)
        {
            GameEvents.TriggerActionCompleted(completedActionID);
        }
    }

    public void Highlight()
    {
        if (isFilled) return;

        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    public void Unhighlight()
    {
        if (isBeaconActive) return;

        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}