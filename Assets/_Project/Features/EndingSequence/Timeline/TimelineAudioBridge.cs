using UnityEngine;

public class TimelineAudioBridge : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip cheersSFX;
    [SerializeField][Range(0f, 1f)] private float volume = 1f;

    /// <summary>
    /// Called by Timeline Signal Receiver to trigger the cheers sound globally.
    /// </summary>
    public void PlayCheersSound()
    {
        if (cheersSFX != null)
        {
            AudioManager.TriggerPlaySFX(cheersSFX, volume);
        }
    }
}