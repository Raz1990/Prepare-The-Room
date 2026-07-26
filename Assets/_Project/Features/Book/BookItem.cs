using System.Collections;
using UnityEngine;

public class BookItem : MonoBehaviour, IPickupable, IHighlightable
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

        if (itemData != null && itemData.pickupSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(itemData.pickupSound);
        }

        GameEvents.TriggerItemPickedUp(itemData);
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
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
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