using UnityEngine;

[CreateAssetMenu(menuName = "SoccerRunner/Session Tuning", fileName = "SessionTuning")]
public class SessionTuning : ScriptableObject
{
    [Header("Level Length Override (optional)")]
    public bool overrideLevelLength = false;
    public float levelLengthMeters = 2000f;
    [Header("Phase Distances (meters)")]
    public float startingDistance = 25f;
    public float waveDistance = 400f;
    public float breakDistance = 100f;

    [Header("Opponent Spawning")]
    public OpponentSpawnTuning opponentSpawn = new OpponentSpawnTuning();

    [System.Serializable]
    public class OpponentSpawnTuning
    {
        [Header("Distance Frequency (meters)")]
        public float minGapMeters = 10f;
        public float maxGapMeters = 12f;

        [Header("Group / Team Feel")]
        public bool enableGroups = true;
        [Range(0f, 1f)] public float groupChance = 0.3f;
        public Vector2Int groupSizeRange = new Vector2Int(2, 3);
        public float groupZSpacing = 3f;

        [Header("Anti-Cluster")]
        public float minDistanceFromOtherOpponents = 8f;
        public int maxTries = 8;
    }
}