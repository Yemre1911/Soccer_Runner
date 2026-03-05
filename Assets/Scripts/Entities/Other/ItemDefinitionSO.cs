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
}