using UnityEngine;

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
}