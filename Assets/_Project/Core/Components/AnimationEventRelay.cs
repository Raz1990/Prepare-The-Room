using UnityEngine;
using UnityEngine.Events;

public class AnimationEventRelay : MonoBehaviour
{
    [Header("Event Relay")]
    // using UnityEvent to have a serialized event that can be assigned in the inspector
    [SerializeField] private UnityEvent onAnimationEnd;

    // Called by the Unity Animation Event at the end of the clip
    public void TriggerAnimationEnd()
    {
        onAnimationEnd?.Invoke();
    }
}