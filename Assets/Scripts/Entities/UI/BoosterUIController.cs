/*using UnityEngine;

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
}*/

using UnityEngine;
using UnityEngine.UI;

public class BoosterUIController : MonoBehaviour
{
    [SerializeField] private BoosterController boosterController;
    [SerializeField] private BoosterUISlot[] slots;

    [Header("Optional Layout Root")]
    [SerializeField] private RectTransform layoutRoot;

    private void Awake()
    {
        if (!boosterController)
            boosterController = GetComponentInParent<BoosterController>();

        if (!layoutRoot)
            layoutRoot = transform as RectTransform;

        ClearAllImmediate();
    }

    private void OnEnable()
    {
        if (!boosterController) return;

        boosterController.OnBoosterActivated += HandleBoosterChanged;
        boosterController.OnBoosterUpdated += HandleBoosterChanged;
        boosterController.OnBoosterRemoved += HandleBoosterChanged;

        RefreshFromController();
    }

    private void OnDisable()
    {
        if (!boosterController) return;

        boosterController.OnBoosterActivated -= HandleBoosterChanged;
        boosterController.OnBoosterUpdated -= HandleBoosterChanged;
        boosterController.OnBoosterRemoved -= HandleBoosterChanged;
    }

    private void HandleBoosterChanged(ActiveBoosterEntry _)
    {
        RefreshFromController();
    }

    private void RefreshFromController()
    {
        ClearAllImmediate();

        if (boosterController == null || slots == null)
        {
            ForceRefreshLayout();
            return;
        }

        var active = boosterController.ActiveBoosters;
        int slotIndex = 0;

        for (int i = 0; i < active.Count; i++)
        {
            ActiveBoosterEntry entry = active[i];
            if (entry == null || entry.itemDefinition == null) continue;

            if (slotIndex >= slots.Length) break;

            if (slots[slotIndex] != null)
                slots[slotIndex].Bind(entry);

            slotIndex++;
        }

        ForceRefreshLayout();
    }

    private void ClearAllImmediate()
    {
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
                slots[i].Hide();
        }
    }

    private void ForceRefreshLayout()
    {
        Canvas.ForceUpdateCanvases();

        if (layoutRoot != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRoot);

        RectTransform selfRt = transform as RectTransform;
        if (selfRt != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(selfRt);

        Canvas.ForceUpdateCanvases();
    }
}