using System.Collections;
using TMPro;
using UnityEngine;

public class PickupItem : MonoBehaviour, IPickupable, IHighlightable
{
    [Header("Item Data")]
    [SerializeField] private ItemSO itemData;
    [SerializeField] private float range = 1.5f;

    [Header("UI Colors")]
    [SerializeField] private Color inputColor = ColorsCenter.Gold;
    [SerializeField] private Color actionColor = ColorsCenter.LightGreen;
    [SerializeField] private Color itemColor = ColorsCenter.Teal;

    [Header("References")]
    [SerializeField] private Collider itemCollider;
    [SerializeField] private Outline outline;

    private bool isPickedUp = false;
    private Animator anim;
    private int originalLayer;

    public TMP_SpriteAsset SpriteAsset => itemData != null ? itemData.spriteAsset : null;

    public float InteractionRange
    {
        get
        {
            return range;
        }
    }

    public ItemSO ItemData
    {
        get
        {
            return itemData;
        }
    }

    void Awake()
    {
        if (itemCollider == null)
        {
            itemCollider = GetComponent<Collider>();
        }

        if (outline == null)
        {
            outline = GetComponent<Outline>();
        }

        anim = GetComponentInChildren<Animator>();

        // Cache the original scene layer (e.g., Interactable)
        originalLayer = gameObject.layer;
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

    public string GetPromptText()
    {
        if (isPickedUp)
        {
            return string.Empty;
        }

        return PromptFormatter.BuildPrompt("E", "Pick up", itemData.itemName, inputColor, actionColor, itemColor, itemData.icon);
    }

    public bool CanPickup(ItemType heldItem)
    {
        if (isPickedUp)
        {
            return false;
        }

        if (heldItem != ItemType.None)
        {
            return false;
        }

        return true;
    }

    public void Pickup(Transform handSocket)
    {
        if (!CanPickup(ItemType.None))
        {
            return;
        }

        isPickedUp = true;

        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }

        Unhighlight();

        GameEvents.TriggerItemPickedUp(itemData);

        // Turn off the Animator so it won't override localPosition
        if (anim != null)
        {
            anim.enabled = false;
        }

        StartCoroutine(AnimateToHand(handSocket));
    }

    private IEnumerator AnimateToHand(Transform handSocket)
    {
        float duration = 0.25f;
        float elapsed = 0f;

        // Using GetPositionAndRotation retrieves both position and rotation in a single native C++ call.
        // Reading transform.position and transform.rotation individually forces Unity to cross the C#/C++ engine 
        // boundary twice and perform redundant matrix evaluations internally.
        transform.GetPositionAndRotation(out Vector3 startPos, out Quaternion startRot);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(startPos, handSocket.position, t);
            Quaternion currentRot = Quaternion.Lerp(startRot, handSocket.rotation, t);

            // SetPositionAndRotation updates both values simultaneously in native code.
            // Setting position and rotation separately forces Unity to invalidate and recalculate 
            // the transform matrix hierarchy twice per frame instead of once.
            transform.SetPositionAndRotation(currentPos, currentRot);

            yield return null;
        }

        transform.SetParent(handSocket);

        if (itemData != null)
        {
            // Apply per-item offsets
            transform.SetLocalPositionAndRotation(itemData.holdPositionOffset, Quaternion.Euler(itemData.holdRotationOffset));
        }
        else
        {
            // Default zero alignment if no SO exists
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void HandleItemPickedUp(ItemSO pickedUpItem)
    {
        // Guard clause: only run if THIS instance matches the picked-up item
        if (pickedUpItem == null || pickedUpItem != itemData) return;

        int heldItemLayer = LayerMask.NameToLayer("HeldItem");
        if (heldItemLayer != -1)
        {
            SetLayerRecursively(gameObject, heldItemLayer);
        }
    }

    private void HandleItemPlaced(ItemSO placedItem)
    {
        // Guard clause: only run if THIS instance matches the placed item
        if (placedItem == null || placedItem != itemData) return;

        SetLayerRecursively(gameObject, originalLayer);
    }

    public void Highlight()
    {
        if (isPickedUp)
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
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}