using UnityEngine;

[CreateAssetMenu(menuName = "SoccerRunner/Spawning/Weighted Spawn Policy")]
public class WeightedSpawnPolicySO : ScriptableObject
{
    [Header("Phase")]
    public SpawnPhaseMask activeInPhases = SpawnPhaseMask.Wave | SpawnPhaseMask.Break;

    [Header("Distance Scheduling")]
    [Tooltip("Spawnlar arası mesafe: min/max. Random aralıkla seçilir.")]
    [Min(0f)] public float minGapMeters = 40f;
    [Min(0f)] public float maxGapMeters = 80f;

    [Header("Spawn Band Ahead of Player")]
    [Tooltip("Player'ın kaç metre önüne spawn edebilir? (min)")]
    [Min(0f)] public float minAheadMeters = 60f;

    [Tooltip("Player'ın kaç metre önüne spawn edebilir? (max)")]
    [Min(0f)] public float maxAheadMeters = 140f;

    [Tooltip("Hard limit: hiçbir koşulda player'dan daha ileri spawn yok.")]
    [Min(0f)] public float hardMaxAheadMeters = 190f;

    [Header("X Range")]
    public float minX = -8f;
    public float maxX = 8f;

    [Header("Y/Z")]
    public float spawnY = 0f;
    [Tooltip("Z'ye küçük jitter eklemek istersen (metre).")]
    [Min(0f)] public float zJitter = 0f;

    [Header("Global Separation")]
    [Tooltip("Herhangi bir item spawnlandıktan sonra, bu kadar metre boyunca başka item spawn etme.")]
    [Min(0f)] public float globalMinSeparationMeters = 25f;

    [Header("Items")]
    public ItemDefinitionSO[] items;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxGapMeters < minGapMeters) maxGapMeters = minGapMeters;
        if (maxAheadMeters < minAheadMeters) maxAheadMeters = minAheadMeters;
        if (hardMaxAheadMeters < maxAheadMeters) hardMaxAheadMeters = maxAheadMeters;
        if (maxX < minX) maxX = minX;
    }
#endif
}