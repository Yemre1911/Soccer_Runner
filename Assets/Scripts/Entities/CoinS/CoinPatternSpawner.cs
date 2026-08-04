using UnityEngine;

public class CoinPatternSpawner : MonoBehaviour
{
    public enum CoinMode { Wave, Break }

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerLateralMovement playerLateralMovement;
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

    [Header("Lane Settings")]
    [Tooltip("Coin çizgilerini oyuncunun sağa/sola hareket ederken ulaşabildiği sabit X konumlarına oturtur.")]
    [SerializeField] private bool snapToPlayerLanes = true;

    [Tooltip("Coin üretilebilecek en soldaki lane sınırı.")]
    public float waveMinX = -10f;

    [Tooltip("Coin üretilebilecek en sağdaki lane sınırı.")]
    public float waveMaxX = 10f;

    [Tooltip("PlayerLateralMovement bulunamazsa kullanılacak lane aralığı.")]
    [SerializeField, Min(0.01f)] private float fallbackLaneStep = 5f;

    public float spawnY = 0f;

    private float _nextSpawnDistance;
    private float _fallbackLaneOriginX;

    private void Awake()
    {
        ResolveLaneReference();
        _fallbackLaneOriginX = playerTransform != null ? playerTransform.position.x : 0f;
    }

    private void Start()
    {
        ResolveLaneReference();
        TriggerImmediateSpawn();
    }

    private void Update()
    {
        if (!spawningEnabled || GameManager.Instance == null) return;

        // Break anında coin üretimini tamamen durdur.
        if (mode == CoinMode.Break) return;

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

        // Wave tekrar başladığında ilk coin çizgisini geciktirme.
        if (mode == CoinMode.Wave)
            TriggerImmediateSpawn();
    }

    private void ResolveLaneReference()
    {
        if (playerLateralMovement == null && playerTransform != null)
            playerLateralMovement = playerTransform.GetComponent<PlayerLateralMovement>();
    }

    private void TriggerImmediateSpawn()
    {
        if (GameManager.Instance != null)
            _nextSpawnDistance = GameManager.Instance.RunDistance + 1f;
    }

    private void SetNextTargetDistance()
    {
        _nextSpawnDistance = GameManager.Instance.RunDistance
            + Random.Range(waveGapMeters.x, waveGapMeters.y);
    }

    private void SpawnCoinGroup()
    {
        if (playerTransform == null || ItemPooler.Instance == null) return;

        int coinCount = Random.Range(waveStripLen.x, waveStripLen.y + 1);
        float baseZ = playerTransform.position.z + spawnDistanceAhead;
        float laneX = GetSpawnLaneX();

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 position = new Vector3(laneX, spawnY, baseZ + (i * waveZSpacing));
            ItemPooler.Instance.Spawn(coinPoolId, position, Quaternion.identity);
        }
    }

    private float GetSpawnLaneX()
    {
        float minX = Mathf.Min(waveMinX, waveMaxX);
        float maxX = Mathf.Max(waveMinX, waveMaxX);

        if (!snapToPlayerLanes)
            return Random.Range(minX, maxX);

        ResolveLaneReference();

        float laneStep = playerLateralMovement != null
            ? playerLateralMovement.StepSize
            : Mathf.Max(0.01f, fallbackLaneStep);

        float laneOriginX = playerLateralMovement != null
            ? playerLateralMovement.LaneOriginX
            : _fallbackLaneOriginX;

        if (playerLateralMovement != null)
        {
            minX = Mathf.Max(minX, -playerLateralMovement.RoadWidth);
            maxX = Mathf.Min(maxX, playerLateralMovement.RoadWidth);
        }

        int minLaneIndex = Mathf.CeilToInt((minX - laneOriginX) / laneStep);
        int maxLaneIndex = Mathf.FloorToInt((maxX - laneOriginX) / laneStep);

        if (minLaneIndex > maxLaneIndex)
        {
            Debug.LogWarning(
                "[CoinPatternSpawner] Lane sınırları içinde geçerli bir lane bulunamadı. " +
                "Coin en yakın geçerli X konumuna yerleştirildi.",
                this);

            return Mathf.Clamp(laneOriginX, minX, maxX);
        }

        int selectedLaneIndex = Random.Range(minLaneIndex, maxLaneIndex + 1);
        return laneOriginX + (selectedLaneIndex * laneStep);
    }
}
