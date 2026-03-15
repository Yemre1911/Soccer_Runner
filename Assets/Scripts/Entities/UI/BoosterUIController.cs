using UnityEngine;

public class BoosterUIController : MonoBehaviour
{
    [SerializeField] private BoosterController boosterController;
    [SerializeField] private BoosterUISlot[] slots;

    private void Awake()
    {
        if (!boosterController)
            boosterController = GetComponentInParent<BoosterController>();

        ClearAll();
    }

    private void OnEnable()
    {
        if (!boosterController) return;

        boosterController.OnBoosterActivated += HandleActivated;
        boosterController.OnBoosterUpdated += HandleUpdated;
        boosterController.OnBoosterRemoved += HandleRemoved;
    }

    private void OnDisable()
    {
        if (!boosterController) return;

        boosterController.OnBoosterActivated -= HandleActivated;
        boosterController.OnBoosterUpdated -= HandleUpdated;
        boosterController.OnBoosterRemoved -= HandleRemoved;
    }

    private void HandleActivated(ActiveBoosterEntry entry)
    {
        BoosterUISlot slot = FindSlotByItemId(entry.ItemId);
        if (slot != null)
        {
            slot.Bind(entry);
            return;
        }

        slot = FindFirstEmptySlot();
        if (slot != null)
        {
            slot.Bind(entry);
            return;
        }

        Debug.LogWarning($"[BoosterUI] No empty UI slot for booster: {entry.ItemId}", this);
    }

    private void HandleUpdated(ActiveBoosterEntry entry)
    {
        BoosterUISlot slot = FindSlotByItemId(entry.ItemId);
        if (slot == null) return;

        slot.SetFill(entry.NormalizedRemaining);
    }

    private void HandleRemoved(ActiveBoosterEntry entry)
    {
        BoosterUISlot slot = FindSlotByItemId(entry.ItemId);
        if (slot == null) return;

        slot.Hide();
    }

    private BoosterUISlot FindSlotByItemId(string itemId)
    {
        if (slots == null) return null;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            if (slots[i].BoundItemId == itemId)
                return slots[i];
        }

        return null;
    }

    private BoosterUISlot FindFirstEmptySlot()
    {
        if (slots == null) return null;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            if (!slots[i].IsOccupied)
                return slots[i];
        }

        return null;
    }

    private void ClearAll()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].Hide();
        }
    }
}