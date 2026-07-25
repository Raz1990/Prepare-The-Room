using UnityEngine;

public class PlayerRaycaster : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float rayDistance = 5f;

    private IInteractable currentInteractable;
    private IHighlightable currentHighlightable;

    void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
    }

    void Update()
    {
        PerformRaycast();
    }

    public void TryInteract()
    {
        if (currentInteractable == null) return;

        ItemType heldItem = GetCurrentlyHeldItem();

        if (currentInteractable.CanInteract(heldItem))
        {
            currentInteractable.Interact(heldItem);
        }
        else
        {
            Debug.Log($"Missing required item: {currentInteractable.RequiredItem}");
        }
    }

    public string GetCurrentPromptText()
    {
        if (currentInteractable == null) return string.Empty;
        return currentInteractable.GetPromptText();
    }

    private ItemType GetCurrentlyHeldItem()
    {
        return ItemManager.Instance != null ? ItemManager.Instance.CurrentHeldItem : ItemType.None;
    }

    private void PerformRaycast()
    {
        // Viewport (0.5, 0.5) targets the center of the screen
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayer))
        {
            // TryGetComponent avoids garbage allocation compared to standard GetComponent checks
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                // Must be in range AND currently interactable
                if (hit.distance <= interactable.InteractionRange && interactable.CanInteract(GetCurrentlyHeldItem()))
                {
                    currentInteractable = interactable;

                    // Check for highlight capability
                    if (hit.collider.TryGetComponent(out IHighlightable highlightable))
                    {
                        SetCurrentHighlightable(highlightable);
                    }
                    else
                    {
                        ClearCurrentHighlightable();
                    }

                    return; // Valid interactable target found in range; skip clearing
                }
            }
        }

        ClearCurrentTarget();
    }

    private void SetCurrentHighlightable(IHighlightable newHighlightable)
    {
        if (currentHighlightable == newHighlightable) return;

        ClearCurrentHighlightable();

        currentHighlightable = newHighlightable;
        currentHighlightable.Highlight();
    }

    private void ClearCurrentHighlightable()
    {
        if (currentHighlightable != null)
        {
            currentHighlightable.Unhighlight();
            currentHighlightable = null;
        }
    }

    private void ClearCurrentTarget()
    {
        currentInteractable = null;
        ClearCurrentHighlightable();
    }
}