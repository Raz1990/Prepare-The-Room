using UnityEngine;

public class PlayerRaycaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float rayDistance = 5f;
    [Tooltip("The radius around the raycast hit point to check for nearby interactable objects.")]
    [SerializeField] private float interactionRadius = 0.2f;

    [Header("Item Holding")]
    [SerializeField] private Transform handSocket;

    private IInteractable currentInteractable;
    private IPickupable currentPickupable;
    private IPlacementZone currentPlacementZone;
    private IHighlightable currentHighlightable;
    private IPromptable currentPromptable;

    void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    void Update()
    {
        PerformRaycast();
    }

    private void PerformRaycast()
    {
        // Viewport (0.5, 0.5) targets the center of the screen.
        // Viewport coordinates are normalized (0,0 is bottom-left, 1,1 is top-right).
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (GetInteractionHit(ray, out RaycastHit hit))
        {
            float hitDistance = hit.distance;

            // TryGetComponent checks for components without allocating garbage memory 
            // compared to standard GetComponent calls which cause GC spikes when missing.

            // 1. World Object Interaction Target (e.g., Sliding Window)
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (HandleInteractable(hit, hitDistance, interactable))
                {
                    // Guard Clause: return; exits immediately when a valid target is locked.
                    // This prevents reaching ClearAllTargets() at the bottom.
                    return;
                }
            }

            ItemType heldItem = GetCurrentlyHeldItem();

            // 2. Item Pickup Target (e.g., Book on desk)
            if (hit.collider.TryGetComponent(out IPickupable pickupable))
            {
                if (HandlePickupable(hit, hitDistance, pickupable, heldItem))
                {
                    return;
                }
            }

            // 3. Item Placement Target (e.g., Bookshelf slot)
            if (hit.collider.TryGetComponent(out IPlacementZone placementZone))
            {
                if (HandlePlaceable(hit, hitDistance, placementZone, heldItem))
                {
                    return;
                }
            }
        }

        // Catch-all: Runs only if raycast misses entirely, hits non-interactable geometry, 
        // or target fails range/CanInteract checks.
        ClearAllTargets();
    }

    /// <summary>
    /// Evaluates precision first (Raycast), then falls back to leeway radius (SphereCast).
    /// </summary>
    private bool GetInteractionHit(Ray ray, out RaycastHit hit)
    {
        // 1. Exact Pinpoint Raycast (Direct crosshair aim)
        if (Physics.Raycast(ray, out hit, rayDistance, interactableLayer))
        {
            return true;
        }

        // 2. Thick SphereCast Fallback (Gives crosshair leeway near small objects)
        if (interactionRadius > 0f && Physics.SphereCast(ray, interactionRadius, out hit, rayDistance, interactableLayer))
        {
            return true;
        }

        hit = default;

        return false;
    }

    // Called by PlayerInteractionInput on LMB press
    public void TryInteract()
    {
        if (currentInteractable == null)
        {
            return;
        }

        if (currentInteractable.CanInteract())
        {
            ItemType heldItem = GetCurrentlyHeldItem();
            currentInteractable.Interact(heldItem);
        }
        else
        {
            Debug.Log($"Missing required item: {currentInteractable.RequiredItem}");
        }
    }

    // Called by PlayerInteractionInput on E press
    public void TryPickupOrPlace()
    {
        ItemType heldItem = GetCurrentlyHeldItem();

        // 1. Handle Pickup
        if (currentPickupable != null)
        {
            PickUp(heldItem);
            return;
        }

        // 2. Handle Placement
        if (currentPlacementZone != null)
        {
            PlaceItem(heldItem);
        }
    }

    private void PickUp(ItemType heldItem)
    {
        if (currentPickupable.CanPickup(heldItem))
        {
            currentPickupable.Pickup(handSocket);
        }
    }

    private void PlaceItem(ItemType heldItem)
    {
        if (currentPlacementZone.CanPlace(heldItem))
        {
            if (handSocket != null && handSocket.childCount > 0)
            {
                // Retrieve the actual item GameObject sitting in the hand socket
                GameObject heldItemObject = handSocket.GetChild(0).gameObject;

                currentPlacementZone.PlaceItem(heldItemObject, HandlePlacementComplete);
            }
        }
    }

    private void HandlePlacementComplete()
    {
        if (ItemManager.Instance != null && ItemManager.Instance.CurrentHeldItemData != null)
        {
            GameEvents.TriggerItemPlaced(ItemManager.Instance.CurrentHeldItemData);
        }
    }

    public IPromptable GetCurrentPromptable()
    {
        return currentPromptable;
    }

    private ItemType GetCurrentlyHeldItem()
    {
        // Safe check against ItemManager instance and held ScriptableObject to retrieve itemID without null exceptions
        if (ItemManager.Instance != null && ItemManager.Instance.CurrentHeldItemData != null)
        {
            return ItemManager.Instance.CurrentHeldItemData.itemID;
        }

        return ItemType.None;
    }

    private bool HandleInteractable(RaycastHit hit, float hitDistance, IInteractable interactable)
    {
        if (hitDistance <= interactable.InteractionRange && interactable.CanInteract())
        {
            hit.collider.TryGetComponent(out IHighlightable highlightable);
            SetInteractableTarget(interactable, highlightable);
            return true;
        }

        return false;
    }

    private bool HandlePickupable(RaycastHit hit, float hitDistance, IPickupable pickupable, ItemType heldItem)
    {
        if (hitDistance <= pickupable.InteractionRange && pickupable.CanPickup(heldItem))
        {
            hit.collider.TryGetComponent(out IHighlightable highlightable);
            SetPickupableTarget(pickupable, highlightable);
            return true;
        }

        return false;
    }

    private bool HandlePlaceable(RaycastHit hit, float hitDistance, IPlacementZone placementZone, ItemType heldItem)
    {
        if (hitDistance <= placementZone.InteractionRange && placementZone.CanPlace(heldItem))
        {
            hit.collider.TryGetComponent(out IHighlightable highlightable);
            SetPlacementZoneTarget(placementZone, highlightable);
            return true;
        }

        return false;
    }

    private void SetInteractableTarget(IInteractable interactable, IHighlightable highlightable)
    {
        currentInteractable = interactable;
        currentPickupable = null;
        currentPlacementZone = null;
        currentPromptable = interactable;

        UpdateHighlightState(highlightable);
    }

    private void SetPickupableTarget(IPickupable pickupable, IHighlightable highlightable)
    {
        currentInteractable = null;
        currentPickupable = pickupable;
        currentPlacementZone = null;
        currentPromptable = pickupable;

        UpdateHighlightState(highlightable);
    }

    private void SetPlacementZoneTarget(IPlacementZone placementZone, IHighlightable highlightable)
    {
        currentInteractable = null;
        currentPickupable = null;
        currentPlacementZone = placementZone;
        currentPromptable = placementZone;

        UpdateHighlightState(highlightable);
    }

    private void UpdateHighlightState(IHighlightable newHighlightable)
    {
        // Avoid unhighlighting and re-highlighting if looking at the same object across frames
        if (currentHighlightable == newHighlightable)
        {
            return;
        }

        ClearCurrentHighlightable();

        if (newHighlightable != null)
        {
            currentHighlightable = newHighlightable;
            currentHighlightable.Highlight();
        }
    }

    private void ClearCurrentHighlightable()
    {
        if (currentHighlightable != null)
        {
            currentHighlightable.Unhighlight();
            currentHighlightable = null;
        }
    }

    private void ClearAllTargets()
    {
        currentInteractable = null;
        currentPickupable = null;
        currentPlacementZone = null;
        currentPromptable = null;

        ClearCurrentHighlightable();
    }
}