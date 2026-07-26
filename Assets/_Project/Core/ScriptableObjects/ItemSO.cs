using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "New_Item", menuName = "Game/Item Data")]
public class ItemSO : ScriptableObject
{
    [Header("Item Details")]
    public ItemType itemID;
    public string itemName;
    public Sprite icon;
    public TMP_SpriteAsset spriteAsset;

    [Header("Audio Clips")]
    public AudioClip pickupSound;
    public AudioClip placementSound;

    [Header("Hold Socket Offset")]
    public Vector3 holdPositionOffset = Vector3.zero;
    public Vector3 holdRotationOffset = Vector3.zero;
}