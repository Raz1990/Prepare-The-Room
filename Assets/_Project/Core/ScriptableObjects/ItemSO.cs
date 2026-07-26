using UnityEngine;

[CreateAssetMenu(fileName = "New_Item", menuName = "Game/Item Data")]
public class ItemSO : ScriptableObject
{
    [Header("Item Details")]
    public ItemType itemID;
    public string itemName;
    public Sprite icon;

    [Header("Audio Clips")]
    public AudioClip pickupSound;
    public AudioClip placementSound;
}