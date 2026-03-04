/*using UnityEngine;

/// <summary>
/// CoinPatternSpawner v2 (LaneReservation YOK)
/// - Distance-based schedule (GameManager.RunDistance)
/// - Wave: daha seyrek, daha uzun gruplar
/// - Gruplar birbirine fazla yaklaşmaz (min separation ile)
/// - Break: Break başlar başlamaz 4 adet upuzun şerit anında spawn olur (tek sefer)
/// </summary>
public class CoinPatternSpawner : MonoBehaviour
{
    public enum CoinMode { Wave, Break }

    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string coinPoolId = "coin";

    [Header("Runtime")]
    [SerializeField] private bool spawningEnabled = true;
    [SerializeField] private CoinMode mode = CoinMode.Wave;

    [Header("Spawn Band (Player'ın önünde)")]
    [SerializeField] private float minAheadZ = 80f;
    [SerializeField] private float maxAheadZ = 115f;
    [SerializeField] private float spawnY = 0f;

    [Header("ANTI-OVERLAP (Group spacing)")]
    [Tooltip("Bir coin grubunun bitiş Z'inden sonra yeni grubun başlayabileceği minimum Z farkı.")]
    [SerializeField] private float minSeparationZ = 35f;

    [Tooltip("Aşırı durumlarda baseZ'yi ileri itmek için güvenlik limiti (sonsuz loop önler).")]
    [SerializeField] private float maxForwardPushZ = 300f;

    [Header("WAVE (Longer groups + less frequent)")]
    [Tooltip("Wave spawn eventleri arası mesafe (metre) - büyüttük => daha seyrek.")]
    [SerializeField] private Vector2 waveGapMeters = new Vector2(18f, 30f);

    [Tooltip("Wave coin grubu uzunluğu (coin sayısı) - büyüttük => daha uzun gruplar.")]
    [SerializeField] private Vector2Int waveStripLen = new Vector2Int(5, 9);

    [Tooltip("Wave coin aralığı (Z). Daha küçük => daha sıkı görünür.")]
    [SerializeField] private float waveZSpacing = 3.0f;

    [Tooltip("Wave: X random aralık")]
    [SerializeField] private float waveMinX = -10f;
    [SerializeField] private float waveMaxX = 10f;

    [Tooltip("Wave: grupların 'aynı çizgi' olmaması için küçük X jitter")]
    [SerializeField] private float waveXJitter = 0.6f;

    [Header("BREAK (Instant 4 long strips)")]
    [Tooltip("Break başlar başlamaz tek seferde 4 şerit spawn eder.")]
    [SerializeField] private bool spawnBreakInstantOnce = true;

    [Tooltip("Break'te 4 lane X pozisyonu (örn: -6, -2, +2, +6). 4 eleman önerilir.")]
    [SerializeField] private float[] breakLaneX = new float[] { -6f, -2f, 2f, 6f };

    [Tooltip("Break şerit uzunluğu (coin sayısı) - 'upuzun' istiyorsun.")]
    [SerializeField] private int breakCoinsPerLane = 15;

    [Tooltip("Break coin aralığı (Z).")]
    [SerializeField] private float breakZSpacing = 3f;

    [Header("Optional Hard Cap")]
    [Tooltip("0 = limit yok. Çok coin istemiyorsan 0'dan büyük ver.")]
    [SerializeField] private int maxCoinsPerEventHardCap = 0;

    // ✅ NEW: Break'e girince Wave'den kalmış coinleri temizle
    [Header("Mode Transition Cleanup")]
    [SerializeField] private bool clearAheadCoinsOnBreak = true;

    [Tooltip("Break'e girerken player'ın önünde bu mesafeye kadar coinleri temizler.")]
    [SerializeField] private float clearAheadDistanceOnBreak = 300f;

    [Tooltip("Break'e girerken player'ın arkasında da küçük bir tampon temizlemek istersen.")]
    [SerializeField] private float clearBehindDistanceOnBreak = 10f;

    // --- internals ---
    private bool _initialized;
    private float _nextAtDistance;
    private float _lastGroupEndZ = float.NegativeInfinity;

    private bool _breakInstantSpawned; // Break'e girince 1 kez

    private void Start()
    {
        if (!playerTransform)
        {
            Debug.LogError("[CoinPatternSpawner] playerTransform atanmadı!");
            enabled = false;
            return;
        }

        if (!ItemPooler.Instance)
        {
            Debug.LogError("[CoinPatternSpawner] ItemPooler.Instance yok! Sahneye ItemPooler ekledin mi?");
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
        if (!_initialized) InitSchedule(d);

        if (mode == CoinMode.Break)
        {
            // Break: Break olur olmaz anında 4 uzun şerit
            if (spawnBreakInstantOnce && !_breakInstantSpawned)
            {
                SpawnBreak_InstantFourLongStrips();
                _breakInstantSpawned = true;

                // Break anında spawn ettikten sonra "wave gibi" tekrar tekrar spawn olmasını istemiyorsan,
                // schedule'ı ileri atıyoruz.
                _nextAtDistance = d + 999999f;
            }
            return;
        }

        // Wave
        if (d >= _nextAtDistance)
        {
            SpawnWave_LongGroup();
            ScheduleNext(d);
        }
    }

    // ---------------- Public API (SessionDirector bağlamak için) ----------------

    public void SetEnabled(bool enabledState)
    {
        spawningEnabled = enabledState;
        if (spawningEnabled) ResetSchedule();
    }

    public void SetMode(CoinMode newMode)
    {
        if (mode == newMode) return;

        mode = newMode;

        if (mode == CoinMode.Break)
        {
            // Break'e girince anında spawn için flag sıfırla
            _breakInstantSpawned = false;

            // ✅ PROFESYONEL ÇÖZÜM:
            // Break'e girerken Wave döneminden "önceden spawn edilmiş" coinleri temizle.
            if (clearAheadCoinsOnBreak)
                ClearCoinsInFrontBand();

            // Break'e geçince overlap state'i sıfırlamak mantıklı
            _lastGroupEndZ = float.NegativeInfinity;

            // Break'te schedule'ı stabilize et
            _initialized = true;
            _nextAtDistance = float.PositiveInfinity;
        }
        else
        {
            // Wave'e dönünce yeniden schedule kur
            ResetSchedule();
        }
    }

    public void ResetSchedule()
    {
        if (GameManager.Instance == null) return;

        _breakInstantSpawned = false;             // güvenli
        _lastGroupEndZ = float.NegativeInfinity;  // ✅ önemli

        _initialized = false; // yeniden init
        InitSchedule(GameManager.Instance.RunDistance);
    }

    // ---------------- Scheduling ----------------

    private void InitSchedule(float currentDistance)
    {
        _initialized = true;
        _nextAtDistance = currentDistance + RandomGap(waveGapMeters);
    }

    private void ScheduleNext(float currentDistance)
    {
        _nextAtDistance = currentDistance + RandomGap(waveGapMeters);
    }

    private float RandomGap(Vector2 gapMeters)
    {
        float min = Mathf.Max(0.05f, Mathf.Min(gapMeters.x, gapMeters.y));
        float max = Mathf.Max(min, Mathf.Max(gapMeters.x, gapMeters.y));
        return Random.Range(min, max);
    }

    // ---------------- Cleanup (Break transition) ----------------

    private void ClearCoinsInFrontBand()
    {
        if (!ItemPooler.Instance || !playerTransform) return;

        float playerZ = playerTransform.position.z;

        float minZ = playerZ - Mathf.Max(0f, clearBehindDistanceOnBreak);
        float maxZ = playerZ + Mathf.Max(0f, clearAheadDistanceOnBreak);

        // SADECE coinPoolId aktiflerini temizle
        ItemPooler.Instance.DespawnActiveWhere(coinPoolId, go =>
        {
            if (!go) return false;
            float z = go.transform.position.z;
            return z >= minZ && z <= maxZ;
        });
    }

    // ---------------- Spawn Helpers ----------------

    private float PickBaseZ(int groupCoinCount, float zSpacing)
    {
        float playerZ = playerTransform.position.z;

        // normal band içinden seç
        float baseZ = playerZ + Random.Range(minAheadZ, maxAheadZ);

        // grubun biteceği z
        float groupLenZ = (groupCoinCount - 1) * zSpacing;
        float intendedEndZ = baseZ + groupLenZ;

        // Önceki grubun bitişi ile çakışmasın:
        float minAllowedBaseZ = _lastGroupEndZ + minSeparationZ;

        if (baseZ < minAllowedBaseZ)
        {
            float pushed = Mathf.Min(minAllowedBaseZ, playerZ + maxForwardPushZ);
            baseZ = pushed;
            intendedEndZ = baseZ + groupLenZ;
        }

        // Spawn bandı çok geride kaldıysa yine de player'ın önünde kalsın
        if (baseZ < playerZ + minAheadZ)
        {
            baseZ = playerZ + minAheadZ;
            intendedEndZ = baseZ + groupLenZ;
        }

        _lastGroupEndZ = Mathf.Max(_lastGroupEndZ, intendedEndZ);

        return baseZ;
    }

    // ---------------- Patterns ----------------

    // WAVE: Daha uzun coin grubu, daha seyrek schedule
    private void SpawnWave_LongGroup()
    {
        int len = Random.Range(waveStripLen.x, waveStripLen.y + 1);
        if (maxCoinsPerEventHardCap > 0) len = Mathf.Min(len, maxCoinsPerEventHardCap);

        float baseZ = PickBaseZ(len, waveZSpacing);

        // ✅ Şerit gibi: tek bir X seç, tüm coinlerde aynı kalsın
        // (waveXJitter'ı şu an kullanmıyoruz; değerlerini bozmadım)
        float x = Random.Range(waveMinX, waveMaxX);

        for (int i = 0; i < len; i++)
        {
            float z = baseZ + i * waveZSpacing;
            Vector3 pos = new Vector3(x, spawnY, z);

            ItemPooler.Instance.Spawn(coinPoolId, pos, Quaternion.identity);
        }
    }

    // BREAK: Break olur olmaz 4 adet upuzun şerit (tek sefer)
    private void SpawnBreak_InstantFourLongStrips()
    {
        if (breakLaneX == null || breakLaneX.Length == 0) return;

        int laneCount = Mathf.Min(4, breakLaneX.Length);
        int coinsPerLane = breakCoinsPerLane;

        // Hard cap varsa toplamı kırp
        int total = laneCount * coinsPerLane;
        if (maxCoinsPerEventHardCap > 0) total = Mathf.Min(total, maxCoinsPerEventHardCap);

        float baseZ = PickBaseZ(coinsPerLane, breakZSpacing);

        int spawned = 0;

        for (int lane = 0; lane < laneCount; lane++)
        {
            float x = breakLaneX[lane];

            for (int i = 0; i < coinsPerLane; i++)
            {
                if (maxCoinsPerEventHardCap > 0 && spawned >= maxCoinsPerEventHardCap)
                    return;

                float z = baseZ + i * breakZSpacing;
                Vector3 pos = new Vector3(x, spawnY, z);

                ItemPooler.Instance.Spawn(coinPoolId, pos, Quaternion.identity);
                spawned++;
            }
        }
    }
}*/
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