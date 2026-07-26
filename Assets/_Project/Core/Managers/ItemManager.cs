using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    public ItemSO CurrentHeldItemData { get; private set; }

    void Awake()
    {
        EnsureSingleInstance();
    }

    void OnEnable()
    {
        GameEvents.OnItemPickedUp += HandleItemPickedUp;
        GameEvents.OnItemPlaced += HandleItemPlaced;
    }

    void OnDisable()
    {
        GameEvents.OnItemPickedUp -= HandleItemPickedUp;
        GameEvents.OnItemPlaced -= HandleItemPlaced;
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

    private void HandleItemPickedUp(ItemSO item)
    {
        if (item != null)
        {
            CurrentHeldItemData = item;
        }
    }

    private void HandleItemPlaced(ItemSO item)
    {
        CurrentHeldItemData = null;
    }
}