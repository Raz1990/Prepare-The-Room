using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionInput : MonoBehaviour
{
    [SerializeField] private PlayerRaycaster raycaster;

    void Awake()
    {
        if (raycaster == null) raycaster = GetComponent<PlayerRaycaster>();
    }

    // Called automatically by Unity PlayerInput component on "Interact" action (LMB)
    public void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            ExecuteInteraction();
        }
    }

    // Called automatically by Unity PlayerInput component on "Pickup" / "SecondaryInteract" action (E key)
    public void OnPickup(InputValue value)
    {
        if (value.isPressed)
        {
            ExecutePickup();
        }
    }

    private void ExecuteInteraction()
    {
        if (raycaster != null)
        {
            raycaster.TryInteract();
        }
    }

    private void ExecutePickup()
    {
        if (raycaster != null)
        {
            raycaster.TryPickupOrPlace();
        }
    }
}