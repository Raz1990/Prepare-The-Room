using UnityEngine;

[CreateAssetMenu(fileName = "NewCheckmarkData", menuName = "Game/Ending Sequence/Checkmark Data")]
public class CheckmarkSO : ScriptableObject
{
    [Header("Visual & Audio Data")]
    public Sprite checkmarkSprite;
    public AudioClip checkmarkSFX;
}