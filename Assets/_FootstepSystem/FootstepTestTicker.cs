using UnityEngine;

public class FootstepTestTicker : MonoBehaviour
{
    [SerializeField] private FootstepPlaybackAnchor targetAnchor;

    [Header("Simulation Timing")]
    [Tooltip("Time in seconds between footsteps (e.g., 0.5f for walking, 0.25f for sprinting)")]
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private int testIntensity = 1; // Default to Medium

    private float timer;

    private void Start()
    {
        if (targetAnchor == null)
            targetAnchor = GetComponent<FootstepPlaybackAnchor>();
    }

    private void Update()
    {
        if (targetAnchor == null) return;

        timer += Time.deltaTime;
        if (timer >= stepInterval)
        {
            timer = 0f;
            // Automatically fires the event exactly like a real animation timeline would!
            targetAnchor.TriggerFootstepEvent(testIntensity);
        }
    }
}