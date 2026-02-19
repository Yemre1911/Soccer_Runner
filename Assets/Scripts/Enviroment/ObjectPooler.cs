/*using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject opponentPrefab;
    [SerializeField] private int initialSize = 20;
    [SerializeField] private Transform container;

    private readonly List<GameObject> pool = new List<GameObject>();
    private readonly List<BaseOpponent> activeOpponents = new List<BaseOpponent>();

    public IReadOnlyList<BaseOpponent> ActiveOpponents => activeOpponents;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        WarmUp();
    }

    private void WarmUp()
    {
        if (opponentPrefab == null)
        {
            Debug.LogError("[ObjectPooler] opponentPrefab atanmadı!");
            return;
        }

        for (int i = 0; i < initialSize; i++)
            CreateNewInstance();
    }

    private GameObject CreateNewInstance()
    {
        GameObject obj = Instantiate(opponentPrefab, container);
        obj.SetActive(false);
        pool.Add(obj);
        return obj;
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation, Transform playerTransform)
    {
        GameObject obj = null;

        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                obj = pool[i];
                break;
            }
        }

        if (obj == null)
            obj = CreateNewInstance();

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        var opp = obj.GetComponent<BaseOpponent>();
        if (opp != null)
        {
            opp.Initialize(playerTransform);

            if (!activeOpponents.Contains(opp))
                activeOpponents.Add(opp);
        }
        else
        {
            Debug.LogWarning("[ObjectPooler] Spawnlanan prefab root'ta BaseOpponent yok!");
        }

        return obj;
    }

    public void Despawn(BaseOpponent opp)
    {
        if (!opp) return;

        opp.OnDespawned();
        activeOpponents.Remove(opp);
        opp.gameObject.SetActive(false);
    }

    // BaseOpponent içinden çağıracağız:
    public void NotifyDespawned(BaseOpponent opp)
    {
        activeOpponents.Remove(opp);
    }
}*/



using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [Serializable]
    public class PoolEntry
    {
        [Tooltip("Bu tipin adı (Debug/istatistik için). Örn: Op_Normal, Op_Slider")]
        public string id = "Op_Normal";

        [Tooltip("Spawn edilecek prefab (root'ta BaseOpponent olmalı)")]
        public GameObject prefab;

        [Tooltip("Warm-up sırasında kaç tane üretilecek")]
        public int initialSize = 20;

        [Tooltip("Ağırlık: seçim olasılığını belirler. 4=Normal, 1=Slider gibi.")]
        public int weight = 1;

        [Tooltip("İstersen hiyerarşide ayrı bir container")]
        public Transform containerOverride;
    }

    [Header("Pools (Weighted)")]
    [SerializeField] private List<PoolEntry> pools = new List<PoolEntry>();

    [Header("Default Container (opsiyonel)")]
    [SerializeField] private Transform defaultContainer;

    // Her prefab tipi için ayrı inactive pool listesi
    private readonly Dictionary<string, List<GameObject>> _poolById = new();
    private readonly Dictionary<string, PoolEntry> _entryById = new();

    // Aktif rakip listesi (anti-cluster için)
    private readonly List<BaseOpponent> _activeOpponents = new();
    public IReadOnlyList<BaseOpponent> ActiveOpponents => _activeOpponents;

    // Weighted seçim için
    private int _totalWeight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        BuildIndex();
        WarmUpAll();
    }

    private void BuildIndex()
    {
        _poolById.Clear();
        _entryById.Clear();
        _totalWeight = 0;

        for (int i = 0; i < pools.Count; i++)
        {
            var e = pools[i];

            if (e == null || string.IsNullOrWhiteSpace(e.id))
            {
                Debug.LogError("[ObjectPooler] PoolEntry id boş olamaz!");
                continue;
            }

            if (e.prefab == null)
            {
                Debug.LogError($"[ObjectPooler] PoolEntry prefab atanmadı! id: {e.id}");
                continue;
            }

            if (_entryById.ContainsKey(e.id))
            {
                Debug.LogError($"[ObjectPooler] Aynı id birden fazla var: {e.id}");
                continue;
            }

            _entryById[e.id] = e;
            _poolById[e.id] = new List<GameObject>();

            if (e.weight < 0) e.weight = 0;
            _totalWeight += e.weight;
        }

        if (_entryById.Count == 0)
            Debug.LogError("[ObjectPooler] Hiç pool tanımı yok! pools listesine en az 1 prefab ekle.");

        if (_totalWeight <= 0)
            Debug.LogError("[ObjectPooler] Total weight 0! En az bir pool weight > 0 olmalı.");
    }

    private void WarmUpAll()
    {
        foreach (var kv in _entryById)
        {
            var e = kv.Value;
            int count = Mathf.Max(0, e.initialSize);

            for (int i = 0; i < count; i++)
                CreateNewInstance(e);
        }
    }

    private GameObject CreateNewInstance(PoolEntry entry)
    {
        Transform parent = entry.containerOverride != null
            ? entry.containerOverride
            : defaultContainer;

        GameObject obj = Instantiate(entry.prefab, parent);
        obj.SetActive(false);

        _poolById[entry.id].Add(obj);
        return obj;
    }

    // -------- Public API --------

    /// <summary>
    /// Ağırlıklara göre rastgele bir opponent spawn eder.
    /// </summary>
    public GameObject SpawnRandom(Vector3 position, Quaternion rotation, Transform playerTransform)
    {
        var entry = PickWeightedEntry();
        if (entry == null) return null;

        return SpawnById(entry.id, position, rotation, playerTransform);
    }

    /// <summary>
    /// Belirli id ile spawn (debug veya özel durumlar için)
    /// </summary>
    public GameObject SpawnById(string id, Vector3 position, Quaternion rotation, Transform playerTransform)
    {
        if (!_entryById.TryGetValue(id, out var entry))
        {
            Debug.LogError($"[ObjectPooler] Böyle bir pool id yok: {id}");
            return null;
        }

        var list = _poolById[id];

        GameObject obj = null;
        for (int i = 0; i < list.Count; i++)
        {
            if (!list[i].activeInHierarchy)
            {
                obj = list[i];
                break;
            }
        }

        if (obj == null)
            obj = CreateNewInstance(entry);

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        var opp = obj.GetComponent<BaseOpponent>();
        if (opp != null)
        {
            opp.Initialize(playerTransform);

            if (!_activeOpponents.Contains(opp))
                _activeOpponents.Add(opp);
        }
        else
        {
            Debug.LogWarning($"[ObjectPooler] Spawnlanan prefab root'ta BaseOpponent yok! id: {id}");
        }

        return obj;
    }

    public void Despawn(BaseOpponent opp)
    {
        if (!opp) return;

        opp.OnDespawned();
        _activeOpponents.Remove(opp);
        opp.gameObject.SetActive(false);
    }

    // BaseOpponent içinden çağırılırsa:
    public void NotifyDespawned(BaseOpponent opp)
    {
        _activeOpponents.Remove(opp);
    }

    // -------- Weighted Pick --------

    private PoolEntry PickWeightedEntry()
    {
        if (_totalWeight <= 0 || pools == null || pools.Count == 0)
            return null;

        int r = UnityEngine.Random.Range(0, _totalWeight); // 0..totalWeight-1
        int sum = 0;

        for (int i = 0; i < pools.Count; i++)
        {
            var e = pools[i];
            if (e == null || e.prefab == null) continue;
            if (e.weight <= 0) continue;

            sum += e.weight;
            if (r < sum)
                return e;
        }

        // Fallback (normalde buraya düşmez)
        for (int i = 0; i < pools.Count; i++)
        {
            var e = pools[i];
            if (e != null && e.prefab != null && e.weight > 0)
                return e;
        }

        return null;
    }
}
