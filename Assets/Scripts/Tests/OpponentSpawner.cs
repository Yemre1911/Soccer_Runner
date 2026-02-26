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

    [Header("Frequency (seconds)")]
    private float minInterval = 0.5f;
    private float maxInterval = 1f;

    [Header("Anti-Cluster (opsiyonel)")]
    [SerializeField] private float minDistanceFromOtherOpponents = 6f;
    [SerializeField] private int maxTries = 8;

    [Header("Rotation")]
    [SerializeField] private bool matchSpawnerRotation = false;

    private float nextSpawnTime;

    // GameOver subscribe güvenliği
    private Coroutine _subscribeRoutine;
    private bool _stopped;

    private void Start()
    {
        if (!playerTransform)
        {
            Debug.LogError("[Spawner] playerTransform atanmadı!");
            enabled = false;
            return;
        }

        if (!ObjectPooler.Instance)
        {
            Debug.LogError("[Spawner] ObjectPooler.Instance yok! Sahneye PoolManager ekledin mi?");
            enabled = false;
            return;
        }

        ScheduleNext();
    }

    private void Update()
    {
        if (_stopped) return;

        if (Time.time >= nextSpawnTime)
        {
            SpawnOne();
            ScheduleNext();
        }
    }

    private void ScheduleNext()
    {
        float t = Random.Range(minInterval, maxInterval);
        nextSpawnTime = Time.time + Mathf.Max(0.05f, t);
    }

    private void SpawnOne()
    {
        Vector3 spawnPos = Vector3.zero;

        for (int i = 0; i < maxTries; i++)
        {
            float x = Random.Range(minX, maxX);
            float z = playerTransform.position.z + Random.Range(minAheadZ, maxAheadZ);
            spawnPos = new Vector3(x, spawnY, z);

            if (!IsTooCloseToOtherOpponents(spawnPos))
                break;
        }

        Quaternion rot = matchSpawnerRotation ? transform.rotation : Quaternion.identity;

        // Tek satır: pooler hangi prefab olacağını kendi seçer (weighted)
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

            float d = Vector3.Distance(opp.transform.position, pos);
            if (d < minDistanceFromOtherOpponents)
                return true;
        }

        return false;
    }

    // ---------------- GameOver Handling ----------------

    private void OnEnable()
    {
        _stopped = false;

        if (GameOver.Instance != null && GameOver.Instance.IsGameOver)
        {
            StopSpawning();
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
            StopSpawning();
            yield break;
        }

        GameOver.Instance.OnGameLost -= StopSpawning;
        GameOver.Instance.OnGameLost += StopSpawning;
    }

    private void OnDisable()
    {
        if (_subscribeRoutine != null)
        {
            StopCoroutine(_subscribeRoutine);
            _subscribeRoutine = null;
        }

        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost -= StopSpawning;
    }

    private void StopSpawning()
    {
        if (_stopped) return;
        _stopped = true;

        StopAllCoroutines();
        CancelInvoke();
        enabled = false;

        Debug.Log("Spawner Durduruldu.");
    }
}
