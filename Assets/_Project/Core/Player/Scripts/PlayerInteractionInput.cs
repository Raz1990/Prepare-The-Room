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

    private void ExecuteInteraction()
    {
        raycaster.TryInteract();
    }
}