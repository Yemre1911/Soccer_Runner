using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemPooler : MonoBehaviour
{
    public static ItemPooler Instance { get; private set; }

    [Serializable]
    public class PoolDef
    {
        public string id;
        public GameObject prefab;
        public int initialSize = 20;
    }

    [Header("Pools")]
    [SerializeField] private PoolDef[] pools;
    [SerializeField] private Transform container;

    private readonly Dictionary<string, Queue<GameObject>> _pool = new();
    private readonly Dictionary<string, GameObject> _prefabs = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (!container) container = this.transform;

        BuildPools();
    }

    private void BuildPools()
    {
        _pool.Clear();
        _prefabs.Clear();

        foreach (var p in pools)
        {
            if (p == null || string.IsNullOrWhiteSpace(p.id) || p.prefab == null) continue;

            _prefabs[p.id] = p.prefab;
            if (!_pool.ContainsKey(p.id))
                _pool[p.id] = new Queue<GameObject>(p.initialSize);

            for (int i = 0; i < Mathf.Max(0, p.initialSize); i++)
            {
                var go = CreateInstance(p.id);
                Return(p.id, go);
            }
        }
    }

    private GameObject CreateInstance(string id)
    {
        var prefab = _prefabs[id];
        var go = Instantiate(prefab, container);
        go.name = $"{prefab.name} (Pooled:{id})";
        go.SetActive(false);

        // item davranışı için zorunlu component
        var item = go.GetComponent<PooledItem>();
        if (!item) item = go.AddComponent<PooledItem>();
        item.Bind(this, id);

        return go;
    }

    public GameObject Spawn(string id, Vector3 pos, Quaternion rot)
    {
        if (!_prefabs.ContainsKey(id))
        {
            Debug.LogError($"[ItemPooler] Unknown pool id: {id}");
            return null;
        }

        if (!_pool.TryGetValue(id, out var q))
            _pool[id] = q = new Queue<GameObject>();

        GameObject go = (q.Count > 0) ? q.Dequeue() : CreateInstance(id);

        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);

        var item = go.GetComponent<PooledItem>();
        item?.OnSpawned();

        return go;
    }

    public void Return(string id, GameObject go)
    {
        if (go == null) return;

        go.SetActive(false);
        go.transform.SetParent(container, true);

        if (!_pool.TryGetValue(id, out var q))
            _pool[id] = q = new Queue<GameObject>();

        q.Enqueue(go);
    }
}