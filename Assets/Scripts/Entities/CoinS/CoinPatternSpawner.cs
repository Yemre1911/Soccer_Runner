using UnityEngine;

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
    [SerializeField] private float minAheadZ = 90f;
    [SerializeField] private float maxAheadZ = 130f;
    [SerializeField] private float spawnY = 0f;

    [Header("ANTI-OVERLAP (Group spacing)")]
    [Tooltip("Bir coin grubunun bitiş Z'inden sonra yeni grubun başlayabileceği minimum Z farkı.")]
    [SerializeField] private float minSeparationZ = 35f;

    [Tooltip("Aşırı durumlarda baseZ'yi ileri itmek için güvenlik limiti (sonsuz loop önler).")]
    [SerializeField] private float maxForwardPushZ = 300f;

    [Header("WAVE (Longer groups + less frequent)")]
    [Tooltip("Wave spawn eventleri arası mesafe (metre) - büyüttük => daha seyrek.")]
    [SerializeField] private Vector2 waveGapMeters = new Vector2(35f, 55f);

    [Tooltip("Wave coin grubu uzunluğu (coin sayısı) - büyüttük => daha uzun gruplar.")]
    [SerializeField] private Vector2Int waveStripLen = new Vector2Int(10, 18);

    [Tooltip("Wave coin aralığı (Z). Daha küçük => daha sıkı görünür.")]
    [SerializeField] private float waveZSpacing = 2.0f;

    [Tooltip("Wave: X random aralık")]
    [SerializeField] private float waveMinX = -9f;
    [SerializeField] private float waveMaxX = 9f;

    [Tooltip("Wave: grupların 'aynı çizgi' olmaması için küçük X jitter")]
    [SerializeField] private float waveXJitter = 0.6f;

    [Header("BREAK (Instant 4 long strips)")]
    [Tooltip("Break başlar başlamaz tek seferde 4 şerit spawn eder.")]
    [SerializeField] private bool spawnBreakInstantOnce = true;

    [Tooltip("Break'te 4 lane X pozisyonu (örn: -6, -2, +2, +6). 4 eleman önerilir.")]
    [SerializeField] private float[] breakLaneX = new float[] { -6f, -2f, 2f, 6f };

    [Tooltip("Break şerit uzunluğu (coin sayısı) - 'upuzun' istiyorsun.")]
    [SerializeField] private int breakCoinsPerLane = 30;

    [Tooltip("Break coin aralığı (Z).")]
    [SerializeField] private float breakZSpacing = 1.9f;

    [Header("Optional Hard Cap")]
    [Tooltip("0 = limit yok. Çok coin istemiyorsan 0'dan büyük ver.")]
    [SerializeField] private int maxCoinsPerEventHardCap = 0;

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
        // Yeni grubun başlangıcı, _lastGroupEndZ + minSeparationZ'in gerisindeyse ileri it.
        float minAllowedBaseZ = _lastGroupEndZ + minSeparationZ;

        if (baseZ < minAllowedBaseZ)
        {
            float pushed = Mathf.Min(minAllowedBaseZ, playerZ + maxForwardPushZ);
            baseZ = pushed;
            intendedEndZ = baseZ + groupLenZ;
        }

        // Spawn bandı çok geride kaldıysa (ör: push yüzünden) yine de player'ın önünde kalsın
        if (baseZ < playerZ + minAheadZ)
        {
            baseZ = playerZ + minAheadZ;
            intendedEndZ = baseZ + groupLenZ;
        }

        // last end güncelle
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

        // baseZ: Break anında "hemen karşıma çıksın" => minAheadZ yakınından başlat
        // Ayrıca önceki wave grubu ile çakışmasın diye PickBaseZ mantığını kullanıyoruz.
        // Burada groupCount = coinsPerLane (en uzun lane), spacing = breakZSpacing
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
}