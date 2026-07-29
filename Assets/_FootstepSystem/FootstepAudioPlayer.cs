using UnityEngine;

public class FootstepAudioPlayer : MonoBehaviour
{
    [SerializeField] private FootstepPlaybackAnchor targetAnchor;

    public void PlayFootstepSound()
    {
        targetAnchor.TriggerFootstepEvent();
    }
}
