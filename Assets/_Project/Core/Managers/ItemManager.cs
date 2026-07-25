using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    public ItemType CurrentHeldItem { get; private set; } = ItemType.None;

    void Awake()
    {
        EnsureSingleInstance();
    }

    private void EnsureSingleInstance()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetHeldItem(ItemType newItem)
    {
        CurrentHeldItem = newItem;
    }
}