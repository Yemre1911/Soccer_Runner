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

    public SessionConfig ActiveConfig { get; private set; } //*


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

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        ActiveConfig = config;

        bool isShotTraining = config.mode == SessionMode.ShotTraining;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetRunDistance();

            if (isShotTraining)
            {
                GameManager.Instance.SetSpeedMultiplier(0f);
                GameManager.Instance.SetSimulationPaused(true);
            }
            else
            {
                GameManager.Instance.ResetRuntimeState();
            }
        }

        // 0) Tuning resolve
        SessionTuning tuning = null;
        if (!isShotTraining)
            tuning = ResolveTuning(config);

        // burada override
        if (tuning != null && tuning.overrideLevelLength)
        {
            config.levelLengthMeters = Mathf.Max(1f, tuning.levelLengthMeters);
            Debug.Log($"[SessionLoader] LevelLength overridden by tuning -> {config.levelLengthMeters}");
        }

        var hud = FindFirstObjectByType<GameHUDController>();
        if (hud) hud.SetDifficulty(config.difficulty);

        // 1) Environment
        if (!isShotTraining)
            BuildEnvironment(config);
        else
            Debug.Log("[SessionLoader] ShotTraining -> Environment build skipped.");

        // 2) Pool Warmup
        if (!isShotTraining)
            WarmupPools(config);
        else
            Debug.Log("[SessionLoader] ShotTraining -> Pool warmup skipped.");

        // 3) UI Init
        InitUI(config);

        // 4) Apply tuning to systems
        if (!isShotTraining)
            ApplyTuningToSystems(tuning);
        else
            Debug.Log("[SessionLoader] ShotTraining -> Tuning skipped.");

        // 5) Begin director
        if (!isShotTraining)
            StartDirector(config, tuning);
        else
            Debug.Log("[SessionLoader] ShotTraining -> SessionDirector.Begin skipped.");

        Debug.Log($"[SessionLoader] Starting Session | Level={config.levelNumber}, Length={config.levelLengthMeters}m, Diff={config.difficulty}, Env={config.environmentId}, Mode={config.mode}");
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