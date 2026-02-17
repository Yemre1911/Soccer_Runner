using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject opponentPrefab;
    [SerializeField] private int initialSize = 20;
    [SerializeField] private Transform container;

    private readonly List<GameObject> pool = new List<GameObject>();

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
        {
            CreateNewInstance();
        }
    }

    private GameObject CreateNewInstance()
    {
        GameObject obj = Instantiate(opponentPrefab, container);
        obj.SetActive(false);
        pool.Add(obj);
        return obj;
    }

    // Pool'dan obje al
    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        GameObject obj = null;

        // inactive ara
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                obj = pool[i];
                break;
            }
        }

        // yoksa büyüt (istersen kapatabilirsin)
        if (obj == null)
        {
            obj = CreateNewInstance();
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    // İstersen manuel despawn (şart değil, BaseOpponent zaten kapatıyor)
    public void Despawn(GameObject obj)
    {
        if (!obj) return;

        var opp = obj.GetComponent<BaseOpponent>();
        if (opp) opp.OnDespawned();

        obj.SetActive(false);
    }
}