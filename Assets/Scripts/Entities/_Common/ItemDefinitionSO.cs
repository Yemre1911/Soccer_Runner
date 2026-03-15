/*using UnityEngine;

[CreateAssetMenu(menuName = "SoccerRunner/Items/Item Definition")]
public class ItemDefinitionSO : ScriptableObject
{
    [Header("Pool / Identity")]
    public string poolId;                 // ItemPooler id
    public ItemCategory category;

    [Header("Spawn Rules")]
    public SpawnPhaseMask allowedPhases = SpawnPhaseMask.Any;
    [Min(0f)] public float weight = 1f;

    [Tooltip("Aynı item tekrar çıkmadan önce minimum kaç metre geçsin?")]
    [Min(0f)] public float cooldownMeters = 0f;

    [Tooltip("Bu item global min separation'a ek olarak kendi separation ister mi?")]
    [Min(0f)] public float minSeparationMeters = 0f;
}*/

using UnityEngine;

[CreateAssetMenu(menuName = "SoccerRunner/Items/Item Definition")]
public class ItemDefinitionSO : ScriptableObject
{
    [Header("Pool / Identity")]
    public string poolId;                 // ItemPooler id
    public ItemCategory category;

    [Header("Spawn Rules")]
    public SpawnPhaseMask allowedPhases = SpawnPhaseMask.Any;
    [Min(0f)] public float weight = 1f;

    [Tooltip("Aynı item tekrar çıkmadan önce minimum kaç metre geçsin?")]
    [Min(0f)] public float cooldownMeters = 0f;

    [Tooltip("Bu item global min separation'a ek olarak kendi separation ister mi?")]
    [Min(0f)] public float minSeparationMeters = 0f;

    [Header("Pickup / UI Identity")]
    public string itemId;
    public string displayName;
    public Sprite icon;

    [Header("Gameplay Type")]
    public ItemUseType useType = ItemUseType.PassiveWorldItem;
    public ItemActivationMode activationMode = ItemActivationMode.Immediate;

    [Header("Duration")]
    public bool isDurationBased = true;
    [Min(0f)] public float duration = 5f;

    [Header("Retrigger")]
    public RetriggerPolicy retriggerPolicy = RetriggerPolicy.RefreshDuration;

    [Header("Effect")]
    public EffectDefinitionSO effectDefinition;
}