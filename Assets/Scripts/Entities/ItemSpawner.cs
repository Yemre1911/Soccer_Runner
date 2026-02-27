using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Pool Id")]
    [SerializeField] private string poolId = "coin";

    [Header("Spawn Band (Player'ın önünde)")]
    [SerializeField] private float minAheadZ = 80f;
    [SerializeField] private float maxAheadZ = 110f;
    [SerializeField] private float spawnY = 0.5f;

    [Header("X Range")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;

    [Header("Distance Frequency (meters)")]
    [SerializeField] private float minGapMeters = 6f;
    [SerializeField] private float maxGapMeters = 12f;

    [Header("Runtime")]
    [SerializeField] private bool spawningEnabled = true;

    private float _nextSpawnAtDistance;
    private bool _initialized;

    private void Start()
    {
        if (!playerTransform)
        {
            Debug.LogError("[ItemSpawner] playerTransform atanmadı!");
            enabled = false;
            return;
        }

        if (!ItemPooler.Instance)
        {
            Debug.LogError("[ItemSpawner] ItemPooler.Instance yok! Sahneye ItemPooler ekledin mi?");
            enabled = false;
            return;
        }

        ResetSchedule();
    }

    private void Update()
    {
        if (!spawningEnabled) return;
        if (GameManager.Instance == null) return;

        float d = GameManager.Instance.RunDistance;
        if (!_initialized) InitializeSchedule(d);

        if (d >= _nextSpawnAtDistance)
        {
            SpawnOne();
            _nextSpawnAtDistance = d + Random.Range(minGapMeters, maxGapMeters);
        }
    }

    public void SetSpawningEnabled(bool enabled)
    {
        spawningEnabled = enabled;
        if (spawningEnabled) ResetSchedule();
    }

    public void ResetSchedule()
    {
        if (GameManager.Instance == null) return;
        InitializeSchedule(GameManager.Instance.RunDistance);
    }

    private void InitializeSchedule(float currentDistance)
    {
        _initialized = true;
        _nextSpawnAtDistance = currentDistance + Random.Range(minGapMeters, maxGapMeters);
    }

    private void SpawnOne()
    {
        float x = Random.Range(minX, maxX);
        float z = playerTransform.position.z + Random.Range(minAheadZ, maxAheadZ);

        Vector3 pos = new Vector3(x, spawnY, z);
        ItemPooler.Instance.Spawn(poolId, pos, Quaternion.identity);
    }
}