using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Break'e girince tetiklenip 4 ability itemini yan yana spawnlar.
/// Seçim UI'sini sonra bağlarız. Şimdilik sadece spawn.
/// </summary>
public class AbilityChoiceSpawnEvent : MonoBehaviour, ISpawnEvent
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Definitions (4 choices will be drawn from these)")]
    [SerializeField] private ItemDefinitionSO[] abilityItems; // category AbilityPickup olanlar
    [Min(1)] [SerializeField] private int choiceCount = 4;

    [Header("Placement")]
    [Min(0f)] [SerializeField] private float aheadMeters = 140f;
    [SerializeField] private float centerX = 0f;
    [Min(0f)] [SerializeField] private float spacingX = 6f;
    [SerializeField] private float spawnY = 0f;

    public void Trigger()
    {
        if (ItemPooler.Instance == null) return;
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }
        if (player == null) return;

        var picks = PickUnique(abilityItems, choiceCount);

        float startX = centerX - (spacingX * (choiceCount - 1) * 0.5f);
        float z = player.position.z + aheadMeters;

        for (int i = 0; i < picks.Count; i++)
        {
            var def = picks[i];
            if (def == null) continue;

            float x = startX + spacingX * i;
            Vector3 pos = new Vector3(x, spawnY, z);
            ItemPooler.Instance.Spawn(def.poolId, pos, Quaternion.identity);
        }
    }

    private static List<ItemDefinitionSO> PickUnique(ItemDefinitionSO[] items, int count)
    {
        var result = new List<ItemDefinitionSO>(count);
        if (items == null || items.Length == 0) return result;

        // basit unique pick
        int guard = 0;
        while (result.Count < count && guard++ < 200)
        {
            var it = items[Random.Range(0, items.Length)];
            if (it == null) continue;
            if (string.IsNullOrEmpty(it.poolId)) continue;
            if (result.Contains(it)) continue;
            result.Add(it);
        }
        return result;
    }
}