using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using UnityEngine;

public class PlacementZone : MonoBehaviour, IPlacementZone, IHighlightable
{
    [Header("Slot Settings")]
    [SerializeField] private ItemSO targetItemData;
    [SerializeField] private float range = 2.0f;
    [SerializeField] private Transform targetPlacementTransform;

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

    public float InteractionRange
    {
        get
        {
            return range;
        }
    }

    public ItemType TargetItemType
    {
        get
        {
            return targetItemData != null ? targetItemData.itemID : ItemType.None;
        }
    }

    public bool IsFilled
    {
        get
        {
            return isFilled;
        }
    }

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
    }

    void OnEnable()
    {
        GameEvents.OnItemPickedUp += HandleItemPickedUp;
        GameEvents.OnItemPlaced += HandleItemPlaced;
    }

    void OnDisable()
    {
        GameEvents.OnItemPickedUp -= HandleItemPickedUp;
        GameEvents.OnItemPlaced -= HandleItemPlaced;
    }

    private void HandleItemPickedUp(ItemSO item)
    {
        // Automatically highlight this slot as a beacon across the room when player picks up the required item
        if (!isFilled && item != null && item.itemID == targetItemData.itemID)
        {
            isBeaconActive = true;

            // Reveal ghost mesh and activate highlight beacon across the room
            if (ghostObject != null)
            {
                ghostObject.SetActive(true);
            }

            Highlight();
        }
    }

    private void HandleItemPlaced(ItemSO item)
    {
        if (item != null && item.itemID == targetItemData.itemID)
        {
            isBeaconActive = false;
            Unhighlight();

            if (ghostObject != null)
            {
                ghostObject.SetActive(false);
            }
        }
    }

    public string GetPromptText()
    {
        if (isFilled)
        {
            return string.Empty;
        }

        string displayName = targetItemData != null ? targetItemData.itemName : "Item";

        return PromptFormatter.BuildPrompt("E", actionToPerform, displayName, inputColor, actionColor, itemColor, targetItemData.icon);
    }

    public bool CanPlace(ItemType heldItem)
    {
        if (isFilled)
        {
            return false;
        }

        if (heldItem != targetItemData.itemID)
        {
            return false;
        }

        return true;
    }

    public void PlaceItem(GameObject itemObject, Action onComplete)
    {
        if (isFilled)
        {
            return;
        }

        isFilled = true;
        isBeaconActive = false;

        // Play placement sound from the currently held ItemSO before clearing item data
        if (ItemManager.Instance != null && ItemManager.Instance.CurrentHeldItemData != null)
        {
            AudioClip placeSound = ItemManager.Instance.CurrentHeldItemData.placementSound;

            if (placeSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(placeSound);
            }
        }

        // Turn off ghost mesh and outline when real book arrives
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

        // GetPositionAndRotation retrieves position and rotation in a single native C++ call
        itemObject.transform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPlacementTransform.position, t);
            Quaternion currentRot = Quaternion.Lerp(startRot, targetPlacementTransform.rotation, t);

            // SetPositionAndRotation updates both transform vectors in a single native engine call
            itemObject.transform.SetPositionAndRotation(currentPos, currentRot);

            yield return null;
        }

        itemObject.transform.SetParent(targetPlacementTransform);
        itemObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        if (onComplete != null)
        {
            onComplete.Invoke();
        }

        GameEvents.TriggerTaskCompleted();
    }

    public void Highlight()
    {
        if (isFilled)
        {
            return;
        }

        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    public void Unhighlight()
    {
        // If the player is currently holding the item, keep the beacon active even if crosshair looks away
        if (isBeaconActive)
        {
            return;
        }

        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}