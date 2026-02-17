using UnityEngine;

public class OpponentSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;

    [Header("Spawn Band (Player'ın önünde)")]
    private float minAheadZ = 100f;   // en yakın
    private float maxAheadZ = 105f;  // en uzak
    [SerializeField] private float spawnY = 0f;

    [Header("X Range")]
    private float minX = -10f;
    private float maxX = 10f;

    [Header("Frequency (seconds)")]
    [Tooltip("Min 1s, Max 2s gibi ayarla")]
    private float minInterval = 0.2f;
    private float maxInterval = 0.7f;

    [Header("Anti-Cluster (opsiyonel)")]
    [Tooltip("Yeni spawn noktası, aktif rakiplere bu mesafeden daha yakın olmasın")]
    [SerializeField] private float minDistanceFromOtherOpponents = 6f;

    [Tooltip("Yakın çıktıysa kaç kez yeniden denesin")]
    [SerializeField] private int maxTries = 8;

    [Header("Rotation")]
    [SerializeField] private bool matchSpawnerRotation = false;

    private float nextSpawnTime;

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
        bool found = false;

        for (int i = 0; i < maxTries; i++)
        {
            float x = Random.Range(minX, maxX);
            float z = playerTransform.position.z + Random.Range(minAheadZ, maxAheadZ);
            spawnPos = new Vector3(x, spawnY, z);

            if (!IsTooCloseToOtherOpponents(spawnPos))
            {
                found = true;
                break;
            }
        }

        // Çok sıkışık sahnede yer bulamazsa, son denediği yeri kullan
        if (!found)
        {
            // Debug istersen:
            // Debug.Log("[Spawner] Yakınlık kuralı yüzünden yer bulamadı, son pozisyon kullanıldı.");
        }

        Quaternion rot = matchSpawnerRotation ? transform.rotation : Quaternion.identity;

        GameObject enemy = ObjectPooler.Instance.Spawn(spawnPos, rot, playerTransform);
       
    }

    private bool IsTooCloseToOtherOpponents(Vector3 pos)
    {
        if (minDistanceFromOtherOpponents <= 0f) return false;

        var activeList = ObjectPooler.Instance.ActiveOpponents;

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
}