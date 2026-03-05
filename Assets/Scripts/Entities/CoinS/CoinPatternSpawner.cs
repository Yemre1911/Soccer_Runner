using UnityEngine;

public class CoinPatternSpawner : MonoBehaviour
{
    public enum CoinMode { Wave, Break }

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string coinPoolId = "coin";

    [Header("Runtime")]
    public bool spawningEnabled = true;
    public CoinMode mode = CoinMode.Wave;

    [Header("Spawn Settings")]
    [Tooltip("Player'ın ne kadar ilerisine spawn edilecek?")]
    public float spawnDistanceAhead = 100f; 
    public Vector2 waveGapMeters = new Vector2(18f, 30f);
    public Vector2Int waveStripLen = new Vector2Int(5, 9);
    public float waveZSpacing = 3.0f;
    public float waveMinX = -10f;
    public float waveMaxX = 10f;
    public float spawnY = 0f;

    private float _nextSpawnDistance;

    private void Start()
    {
        TriggerImmediateSpawn();
    }

    private void Update()
    {
        if (!spawningEnabled || GameManager.Instance == null) return;
        
        // 1. Break anında spawn tamamen iptal
        if (mode == CoinMode.Break) return;

        // 2. Wave modundayken hedef mesafeye ulaşıldığında spawn yap
        if (GameManager.Instance.RunDistance >= _nextSpawnDistance)
        {
            SpawnCoinGroup();
            SetNextTargetDistance();
        }
    }

    public void SetMode(CoinMode newMode)
    {
        if (mode == newMode) return;
        mode = newMode;

        // Wave tekrar başladığında sistemi beklemeden hemen spawn yapması için tetikle
        if (mode == CoinMode.Wave)
        {
            TriggerImmediateSpawn();
        }
    }

    private void TriggerImmediateSpawn()
    {
        if (GameManager.Instance != null)
        {
            // Wave başlar başlamaz (sadece 1 metre sonra) ilk grubu at
            _nextSpawnDistance = GameManager.Instance.RunDistance + 1f; 
        }
    }

    private void SetNextTargetDistance()
    {
        _nextSpawnDistance = GameManager.Instance.RunDistance + Random.Range(waveGapMeters.x, waveGapMeters.y);
    }

    private void SpawnCoinGroup()
    {
        if (playerTransform == null || ItemPooler.Instance == null) return;

        int coinCount = Random.Range(waveStripLen.x, waveStripLen.y + 1);
        float baseZ = playerTransform.position.z + spawnDistanceAhead;
        float xPos = Random.Range(waveMinX, waveMaxX); // Tek şerit hizası

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 pos = new Vector3(xPos, spawnY, baseZ + (i * waveZSpacing));
            ItemPooler.Instance.Spawn(coinPoolId, pos, Quaternion.identity);
        }
    }
}