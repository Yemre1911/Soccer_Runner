using UnityEngine;
using System.Collections;

public class OpponentSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Spawn Band (Player'ın önünde)")]
    [SerializeField] private float minAheadZ = 100f;
    [SerializeField] private float maxAheadZ = 105f;
    [SerializeField] private float spawnY = 0f;

    [Header("X Range")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;

    [Header("Distance Frequency (meters)")]
    [SerializeField] private float minGapMeters = 10f;
    [SerializeField] private float maxGapMeters = 12f;

    [Header("Group / Team Feel")]
    [SerializeField] private bool enableGroups = true;
    [SerializeField, Range(0f, 1f)] private float groupChance = 0.33f;
    [SerializeField] private Vector2Int groupSizeRange = new Vector2Int(2, 3);
    [SerializeField] private float groupZSpacing = 3f;

    [Header("Anti-Cluster (optional)")]
    [SerializeField] private float minDistanceFromOtherOpponents = 6f;
    [SerializeField] private int maxTries = 8;

    [Header("Rotation")]
    [SerializeField] private bool matchSpawnerRotation = false;

    [Header("Runtime")]
    [SerializeField] private bool spawningEnabled = true;

    private float _nextSpawnAtDistance;
    private bool _initialized;

    // GameOver subscribe güvenliği
    private Coroutine _subscribeRoutine;
    private bool _subscribed;

    private void Start()
    {
        if (!playerTransform)
        {
            Debug.LogError("[OpponentSpawner] playerTransform atanmadı!");
            enabled = false;
            return;
        }

        if (!ObjectPooler.Instance)
        {
            Debug.LogError("[OpponentSpawner] ObjectPooler.Instance yok! Sahneye PoolManager ekledin mi?");
            enabled = false;
            return;
        }

        InitializeSchedule();
    }

    private void Update()
    {
        if (!spawningEnabled) return;

        // Director gelene kadar pratik: RunDistance’ı GameManager’dan oku.
        float d = GetRunDistance();
        if (!_initialized) InitializeSchedule(d);

        if (d >= _nextSpawnAtDistance)
        {
            SpawnWave();
            _nextSpawnAtDistance = d + Random.Range(minGapMeters, maxGapMeters);
        }
    }

    // -------------------- Public API (Director için) --------------------

    public void SetSpawningEnabled(bool enabled)
    {
        spawningEnabled = enabled;

        // tekrar açılınca bir sonraki spawn'ı ileri at, aniden patlamasın
        if (spawningEnabled)
        {
            float d = GetRunDistance();
            InitializeSchedule(d);
        }
    }

    public void ConfigureGaps(float minMeters, float maxMeters)
    {
        minGapMeters = Mathf.Max(0.05f, minMeters);
        maxGapMeters = Mathf.Max(minGapMeters, maxMeters);
    }

    public void ResetSchedule()
    {
        InitializeSchedule(GetRunDistance());
    }

    // -------------------- Schedule --------------------

    private void InitializeSchedule()
    {
        InitializeSchedule(GetRunDistance());
    }

    private void InitializeSchedule(float currentDistance)
    {
        _initialized = true;
        _nextSpawnAtDistance = currentDistance + Random.Range(minGapMeters, maxGapMeters);
    }

    private float GetRunDistance()
    {
        // Burayı senin GameManager’ına göre uyarlayabilirsin.
        // Örn: GameManager.Instance.RunDistance
        if (GameManager.Instance != null)
            return GameManager.Instance.RunDistance;

        // fallback: distance yoksa time ile taklit (çok nadir)
        return Time.time * 10f;
    }

    // -------------------- Spawn --------------------

    private void SpawnWave()
    {
        int count = 1;
        if (enableGroups && Random.value < groupChance)
            count = Random.Range(groupSizeRange.x, groupSizeRange.y + 1);

        float baseZ = playerTransform.position.z + Random.Range(minAheadZ, maxAheadZ);

        for (int i = 0; i < count; i++)
            SpawnOneAtZ(baseZ + i * groupZSpacing);
    }

    private void SpawnOneAtZ(float z)
    {
        Vector3 spawnPos = Vector3.zero;

        for (int i = 0; i < maxTries; i++)
        {
            float x = Random.Range(minX, maxX);
            spawnPos = new Vector3(x, spawnY, z);

            if (!IsTooCloseToOtherOpponents(spawnPos))
                break;
        }

        Quaternion rot = matchSpawnerRotation ? transform.rotation : Quaternion.identity;
        ObjectPooler.Instance.SpawnRandom(spawnPos, rot, playerTransform);
    }

    private bool IsTooCloseToOtherOpponents(Vector3 pos)
    {
        if (minDistanceFromOtherOpponents <= 0f) return false;

        var activeList = ObjectPooler.Instance.ActiveOpponents;
        if (activeList == null) return false;

        for (int i = 0; i < activeList.Count; i++)
        {
            var opp = activeList[i];
            if (!opp || !opp.gameObject.activeInHierarchy) continue;

            if (Vector3.Distance(opp.transform.position, pos) < minDistanceFromOtherOpponents)
                return true;
        }

        return false;
    }

    // -------------------- GameOver Handling --------------------

    private void OnEnable()
    {
        if (GameOver.Instance != null && GameOver.Instance.IsGameOver)
        {
            SetSpawningEnabled(false);
            return;
        }

        if (_subscribeRoutine != null) StopCoroutine(_subscribeRoutine);
        _subscribeRoutine = StartCoroutine(SubscribeGameOverWhenReady());
    }

    private IEnumerator SubscribeGameOverWhenReady()
    {
        while (GameOver.Instance == null)
            yield return null;

        if (GameOver.Instance.IsGameOver)
        {
            SetSpawningEnabled(false);
            yield break;
        }

        if (!_subscribed)
        {
            GameOver.Instance.OnGameLost -= HandleGameLost;
            GameOver.Instance.OnGameLost += HandleGameLost;
            _subscribed = true;
        }
    }

    private void OnDisable()
    {
        if (_subscribeRoutine != null)
        {
            StopCoroutine(_subscribeRoutine);
            _subscribeRoutine = null;
        }

        if (GameOver.Instance != null && _subscribed)
        {
            GameOver.Instance.OnGameLost -= HandleGameLost;
            _subscribed = false;
        }
    }

    private void HandleGameLost()
    {
        SetSpawningEnabled(false);
        Debug.Log("[OpponentSpawner] GameOver -> Spawning disabled.");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxAheadZ < minAheadZ) maxAheadZ = minAheadZ;
        if (maxX < minX) maxX = minX;

        minGapMeters = Mathf.Max(0.05f, minGapMeters);
        maxGapMeters = Mathf.Max(minGapMeters, maxGapMeters);

        maxTries = Mathf.Clamp(maxTries, 1, 100);
        groupSizeRange.x = Mathf.Clamp(groupSizeRange.x, 1, 20);
        groupSizeRange.y = Mathf.Clamp(groupSizeRange.y, groupSizeRange.x, 20);
        groupZSpacing = Mathf.Max(0.1f, groupZSpacing);
    }
#endif
}