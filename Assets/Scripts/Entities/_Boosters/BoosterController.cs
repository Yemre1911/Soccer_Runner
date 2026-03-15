using System;
using System.Collections.Generic;
using UnityEngine;

public class BoosterController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerEffectContext effectContext;
    [SerializeField] private BoosterUIController boosterUI;

    private readonly List<ActiveBoosterEntry> _activeBoosters = new();

    public IReadOnlyList<ActiveBoosterEntry> ActiveBoosters => _activeBoosters;

    public event Action<ActiveBoosterEntry> OnBoosterActivated;
    public event Action<ActiveBoosterEntry> OnBoosterUpdated;
    public event Action<ActiveBoosterEntry> OnBoosterRemoved;

    private void Awake()
    {
        if (!effectContext) effectContext = GetComponent<PlayerEffectContext>();
        if (!boosterUI) boosterUI = GetComponentInChildren<BoosterUIController>(true);
    }

    private void Update()
    {
        if (_activeBoosters.Count == 0) return;

        for (int i = _activeBoosters.Count - 1; i >= 0; i--)
        {
            ActiveBoosterEntry entry = _activeBoosters[i];
            if (entry == null || entry.itemDefinition == null)
            {
                _activeBoosters.RemoveAt(i);
                continue;
            }

            entry.remainingTime -= Time.deltaTime;
            OnBoosterUpdated?.Invoke(entry);

            if (entry.remainingTime <= 0f)
            {
                RemoveBoosterAt(i);
            }
        }
    }

    public void ActivateBooster(ItemDefinitionSO itemDefinition)
    {
        if (itemDefinition == null)
        {
            Debug.LogWarning("[BoosterController] Null booster itemDefinition.", this);
            return;
        }

        string itemId = GetItemId(itemDefinition);
        int existingIndex = FindIndexByItemId(itemId);

        if (existingIndex >= 0)
        {
            RetriggerExisting(existingIndex, itemDefinition);
            return;
        }

        ActiveBoosterEntry entry = new ActiveBoosterEntry
        {
            itemDefinition = itemDefinition,
            totalDuration = Mathf.Max(0f, itemDefinition.duration),
            remainingTime = Mathf.Max(0f, itemDefinition.duration),
            runtimeEffect = CreateRuntimeEffect(itemDefinition)
        };

        if (entry.runtimeEffect != null && effectContext != null)
        {
            entry.runtimeEffect.OnApply(itemDefinition, effectContext);
        }

        _activeBoosters.Add(entry);

        OnBoosterActivated?.Invoke(entry);
        OnBoosterUpdated?.Invoke(entry);

        Debug.Log($"[BoosterController] Activated -> {GetLabel(itemDefinition)}", this);
    }

    private void RetriggerExisting(int index, ItemDefinitionSO newDefinition)
    {
        ActiveBoosterEntry entry = _activeBoosters[index];
        if (entry == null || entry.itemDefinition == null) return;

        switch (newDefinition.retriggerPolicy)
        {
            case RetriggerPolicy.RefreshDuration:
                entry.totalDuration = Mathf.Max(0f, newDefinition.duration);
                entry.remainingTime = Mathf.Max(0f, newDefinition.duration);
                break;

            case RetriggerPolicy.AddDuration:
                entry.totalDuration = Mathf.Max(entry.totalDuration, newDefinition.duration);
                entry.remainingTime += Mathf.Max(0f, newDefinition.duration);
                break;

            case RetriggerPolicy.IgnoreIfActive:
                break;
        }

        OnBoosterUpdated?.Invoke(entry);

        Debug.Log($"[BoosterController] Refreshed -> {GetLabel(newDefinition)}", this);
    }

    private void RemoveBoosterAt(int index)
    {
        ActiveBoosterEntry entry = _activeBoosters[index];

        if (entry != null && entry.runtimeEffect != null && effectContext != null)
        {
            entry.runtimeEffect.OnRemove(entry.itemDefinition, effectContext);
        }

        _activeBoosters.RemoveAt(index);
        OnBoosterRemoved?.Invoke(entry);

        if (entry != null && entry.itemDefinition != null)
        {
            Debug.Log($"[BoosterController] Removed -> {GetLabel(entry.itemDefinition)}", this);
        }
    }

    private int FindIndexByItemId(string itemId)
    {
        for (int i = 0; i < _activeBoosters.Count; i++)
        {
            if (_activeBoosters[i] == null) continue;
            if (_activeBoosters[i].ItemId == itemId)
                return i;
        }

        return -1;
    }

    private static string GetItemId(ItemDefinitionSO itemDefinition)
    {
        if (itemDefinition == null) return string.Empty;
        return string.IsNullOrWhiteSpace(itemDefinition.itemId)
            ? itemDefinition.name
            : itemDefinition.itemId;
    }

    private static string GetLabel(ItemDefinitionSO itemDefinition)
    {
        if (itemDefinition == null) return "NULL";
        return string.IsNullOrWhiteSpace(itemDefinition.displayName)
            ? itemDefinition.name
            : itemDefinition.displayName;
    }

    private IBoosterEffect CreateRuntimeEffect(ItemDefinitionSO itemDefinition)
    {
        // Şimdilik effect bağlamıyoruz.
        // Bir sonraki adımda effectDefinition'a göre gerçek runtime effect döndüreceğiz.
        return null;
    }
}