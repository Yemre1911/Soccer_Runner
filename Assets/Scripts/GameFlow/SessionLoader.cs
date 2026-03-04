/*using UnityEngine;

public class SessionLoader : MonoBehaviour
{
    [Header("Auto Start (GameScene direkt açılırsa)")]
    [SerializeField] private bool autoStartIfNoConfig = true;

    [Header("Optional References (şimdilik boş bırakabilirsin)")]
    [Tooltip("Varsa: sahneye yerleştirdiğin SessionDirector objesini buraya ver.")]
    [SerializeField] private SessionDirector sessionDirector;

    [Tooltip("Environment kurulumunu yapan bir scriptin varsa buraya ver (yoksa boş kalsın).")]
    [SerializeField] private MonoBehaviour environmentBuilder; // örn: EnvironmentBuilder

    [Tooltip("Pool warmup yapacağın ObjectPooler varsa buraya ver (yoksa boş kalsın).")]
    [SerializeField] private ObjectPooler pooler;

    [Tooltip("UI init yöneticin varsa buraya ver (yoksa boş kalsın).")]
    [SerializeField] private MonoBehaviour uiInitializer; // örn: SessionUIBootstrap

    private bool _started;

    private void Awake()
    {
        // Sahneye koymayı unutursan otomatik bulmaya çalışır
        if (!sessionDirector) sessionDirector = FindFirstObjectByType<SessionDirector>();
        if (!pooler) pooler = FindFirstObjectByType<ObjectPooler>();
    }

    private void Start()
    {
        TryStartFromPendingOrDefault();
    }

    private void TryStartFromPendingOrDefault()
    {
        if (_started) return;

        // TestScene’den gelen config
        SessionConfig config = SessionBootstrap.ConsumeConfig();

        // GameScene direkt açıldıysa default ile başlat
        if (config == null && autoStartIfNoConfig)
            config = SessionConfig.DefaultTest();

        if (config == null)
        {
            Debug.Log("[SessionLoader] No config found. Waiting (autoStartIfNoConfig=false).");
            return;
        }

        StartSession(config);
    }

    public void StartSession(SessionConfig config)
    {
        if (_started) return;
        _started = true;

        Debug.Log($"[SessionLoader] Starting Session | " +
                  $"Level={config.levelNumber}, Length={config.levelLengthMeters}m, Diff={config.difficulty}, Env={config.environmentId}");

        // 1) Environment (şimdilik placeholder)
        BuildEnvironment(config);

        // 2) Pool Warmup (varsa)
        WarmupPools(config);

        // 3) UI Init (varsa)
        InitUI(config);

        // 4) Director başlat
        StartDirector(config);
    }

    private void BuildEnvironment(SessionConfig config)
    {
        // Şimdilik sadece log. EnvironmentBuilder ekleyince buraya bağlarsın.
        if (environmentBuilder == null)
        {
            Debug.Log($"[SessionLoader] EnvironmentBuilder not set. (EnvId: {config.environmentId})");
            return;
        }

        // İleride: environmentBuilder.GetComponent<EnvironmentBuilder>().Build(config.environmentId);
        Debug.Log("[SessionLoader] EnvironmentBuilder exists (hook it when ready).");
    }

    private void WarmupPools(SessionConfig config)
    {
        if (pooler == null)
        {
            Debug.Log("[SessionLoader] Pooler not set. Skipping warmup.");
            return;
        }

        // Senin pooler’da WarmUp fonksiyonu varsa burada çağır.
        // pooler.WarmUp();
        Debug.Log("[SessionLoader] Pooler exists. Call pooler.WarmUp() here if you have it.");
    }

    private void InitUI(SessionConfig config)
    {
        if (uiInitializer == null)
        {
            Debug.Log("[SessionLoader] UI initializer not set. Skipping UI init.");
            return;
        }

        // İleride: uiInitializer.GetComponent<SessionUIBootstrap>().Init(config);
        Debug.Log("[SessionLoader] UI initializer exists (hook it when ready).");
    }

    private void StartDirector(SessionConfig config)
    {
        if (sessionDirector == null)
        {
            Debug.LogWarning("[SessionLoader] SessionDirector not found. Session started without director.");
            return;
        }

        // SessionDirector'ı sen yazdığında/varsa bu imzayı kullan:
        // sessionDirector.Begin(config);

        sessionDirector.Begin(config); // Aşağıdaki stub ile uyumlu
    }
}*/

using UnityEngine;

public class SessionLoader : MonoBehaviour
{
    [Header("Auto Start (GameScene direkt açılırsa)")]
    [SerializeField] private bool autoStartIfNoConfig = true;

    [Header("Core References")]
    [SerializeField] private SessionDirector sessionDirector;
    [SerializeField] private OpponentSpawner opponentSpawner;

    [Header("Tuning")]
    [Tooltip("Easy/Medium/Hard tuning provider (ScriptableObject).")]
    [SerializeField] private SessionTuningProviderSO tuningProvider;

    [Header("Optional References")]
    [SerializeField] private MonoBehaviour environmentBuilder;
    [SerializeField] private ObjectPooler pooler;
    [SerializeField] private MonoBehaviour uiInitializer;

    private bool _started;

    private void Awake()
    {
        if (!sessionDirector) sessionDirector = FindFirstObjectByType<SessionDirector>();
        if (!opponentSpawner) opponentSpawner = FindFirstObjectByType<OpponentSpawner>();
        if (!pooler) pooler = FindFirstObjectByType<ObjectPooler>();
    }

    private void Start()
    {
        TryStartFromPendingOrDefault();
    }

    private void TryStartFromPendingOrDefault()
    {
        if (_started) return;

        SessionConfig config = SessionBootstrap.ConsumeConfig();

        if (config == null && autoStartIfNoConfig)
            config = SessionConfig.DefaultTest();

        if (config == null)
        {
            Debug.Log("[SessionLoader] No config found. Waiting (autoStartIfNoConfig=false).");
            return;
        }

        StartSession(config);
    }

    public void StartSession(SessionConfig config)
    {
        if (_started) return;
        _started = true;

        // 0) Tuning resolve
        SessionTuning tuning = ResolveTuning(config);

        // burada override
        if (tuning != null && tuning.overrideLevelLength)
        {
            config.levelLengthMeters = Mathf.Max(1f, tuning.levelLengthMeters);
            Debug.Log($"[SessionLoader] LevelLength overridden by tuning -> {config.levelLengthMeters}");
        }

        // UI: Difficulty yazdır (ŞUANDA SADECE TEST-SİLİNECEK) -----------------------------------------
        var hud = FindFirstObjectByType<GameHUDController>();
        if (hud) hud.SetDifficulty(config.difficulty);
        // ----------------------------------------------------------------------------------------------

        // 1) Environment
        BuildEnvironment(config);

        // 2) Pool Warmup
        WarmupPools(config);

        // 3) UI Init
        InitUI(config);

        // 4) Apply tuning to systems
        ApplyTuningToSystems(tuning);

        // 5) Begin director
        StartDirector(config, tuning);

        Debug.Log($"[SessionLoader] Starting Session | Level={config.levelNumber}, Length={config.levelLengthMeters}m, " +
                  $"Diff={config.difficulty}, Env={config.environmentId}");
    }

    private SessionTuning ResolveTuning(SessionConfig config)
    {
        if (!tuningProvider)
        {
            Debug.LogWarning("[SessionLoader] tuningProvider not set. Using runtime defaults (Director/Spawner inspector values).");
            return null;
        }

        var tuning = tuningProvider.GetTuning(config.difficulty);
        if (!tuning)
            Debug.LogWarning($"[SessionLoader] No tuning asset for {config.difficulty}. Using defaults.");

        return tuning;
    }

    private void ApplyTuningToSystems(SessionTuning tuning)
    {
        if (!tuning) return;

        // Spawner
        if (opponentSpawner)
        {
            opponentSpawner.ApplyTuning(tuning.opponentSpawn);
        }
        else
        {
            Debug.LogWarning("[SessionLoader] OpponentSpawner not found to apply tuning.");
        }

        // İleride: Coin spawner, skill spawner, obstacle spawner vb.
    }

    private void BuildEnvironment(SessionConfig config)
    {
        if (environmentBuilder == null)
        {
            Debug.Log($"[SessionLoader] EnvironmentBuilder not set. (EnvId: {config.environmentId})");
            return;
        }

        Debug.Log("[SessionLoader] EnvironmentBuilder exists (hook it when ready).");
    }

    private void WarmupPools(SessionConfig config)
    {
        if (pooler == null)
        {
            Debug.Log("[SessionLoader] Pooler not set. Skipping warmup.");
            return;
        }

        Debug.Log("[SessionLoader] Pooler exists. Call pooler.WarmUp() here if you have it.");
    }

    private void InitUI(SessionConfig config)
    {
        if (uiInitializer == null)
        {
            Debug.Log("[SessionLoader] UI initializer not set. Skipping UI init.");
            return;
        }

        Debug.Log("[SessionLoader] UI initializer exists (hook it when ready).");
    }

    private void StartDirector(SessionConfig config, SessionTuning tuning)
    {
        if (sessionDirector == null)
        {
            Debug.LogWarning("[SessionLoader] SessionDirector not found. Session started without director.");
            return;
        }

        sessionDirector.Begin(config, tuning);
    }
}