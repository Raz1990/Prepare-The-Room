using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (characterController == null) characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        CheckForMovement();
    }

    private void CheckForMovement()
    {
        if (animator == null || characterController == null) return;

        // Check actual ground movement speed (ignoring vertical Y velocity like falling/gravity)
        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
        // Checks squared speed instead of .magnitude to avoid CPU-heavy square root operations
        bool isMoving = horizontalVelocity.sqrMagnitude > 0.01f;

        animator.SetBool(IsWalkingHash, isMoving);
    }
}