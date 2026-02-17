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
}