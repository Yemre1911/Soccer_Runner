/*using UnityEngine;

public class AudienceAttackDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SessionDirector sessionDirector;
    [SerializeField] private Transform leftLauncher;
    [SerializeField] private Transform rightLauncher;

    [Header("Projectile Pool Ids (sequence)")]
    [SerializeField] private string[] projectilePoolIds;

    [Header("Spawn (distance-based)")]
    [Tooltip("Oyuncunun önünde kaç metreye spawn edilsin.")]
    [SerializeField] private float spawnAheadMeters = 75f;

    [Tooltip("Attack aktifken her kaç metrede bir atış olsun.")]
    [SerializeField] private float spawnEveryMeters = 22f;

    [Header("Landing X (edge-biased)")]
    [Tooltip("Genelde kenara düşsün: min..max arası, side işaretiyle çarpılacak.")]
    [SerializeField] private float landingEdgeMinX = 5.5f;

    [SerializeField] private float landingEdgeMaxX = 9.5f;

    [Tooltip(">1 => daha çok min tarafına yığılır (kenara bias için iyi).")]
    [SerializeField] private float edgeBias = 2.2f;

    [Header("Throw Shape")]
    [SerializeField] private float throwDuration = 0.55f;
    [SerializeField] private float arcHeight = 3.5f;
    [SerializeField] private float landingLocalY = 0f;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private bool _active;
    private float _nextSpawnDistance;
    private int _side = -1; // -1 left, +1 right
    private int _seq;

    private IRunDistance _run;

    private void Awake()
    {
        if (!sessionDirector) sessionDirector = FindFirstObjectByType<SessionDirector>();
        _run = GameManager.Instance; // GameManager implements IRunDistance

        if (!leftLauncher || !rightLauncher)
            Debug.LogWarning("[AudienceAttack] Launchers not set. Please assign Left/Right launcher transforms.");
    }

    private void OnEnable()
    {
        if (sessionDirector)
        {
            sessionDirector.OnWaveStarted += HandleWaveStarted;
            sessionDirector.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDisable()
    {
        if (sessionDirector)
        {
            sessionDirector.OnWaveStarted -= HandleWaveStarted;
            sessionDirector.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandleWaveStarted(int waveIndex, int totalWaves)
    {
        if (_run == null) _run = GameManager.Instance;

        bool should = AudienceAttackRules.ShouldBeActiveOnWave(sessionDirector.Difficulty, waveIndex, totalWaves);

        if (should)
        {
            StartAttack();
            if (log) Debug.Log($"[AudienceAttack] START (wave {waveIndex}/{totalWaves}, diff={sessionDirector.Difficulty})");
        }
        else
        {
            StopAttack();
        }
    }

    private void HandlePhaseChanged(SessionDirector.Phase phase)
    {
        // Wave dışına çıkınca kapat
        if (phase != SessionDirector.Phase.Wave)
        {
            StopAttack();
        }
    }

    private void StartAttack()
    {
        _active = true;
        _nextSpawnDistance = (_run != null ? _run.RunDistance : 0f) + 8f; // wave başında hemen spam olmasın
        _side = -_side; // görsel çeşit
        Debug.Log("Audience Attack Started");
    }

    private void StopAttack()
    {
        _active = false;
        Debug.Log("Audience Attack Stopped");
    }

    private void Update()
    {
        if (!_active) return;
        if (_run == null) return;
        if (ItemPooler.Instance == null) return;

        float d = _run.RunDistance;
        if (d < _nextSpawnDistance) return;

        SpawnOne();
        _nextSpawnDistance = d + Mathf.Max(1f, spawnEveryMeters);
        _side = -_side;
    }

    private void SpawnOne()
    {
        if (projectilePoolIds == null || projectilePoolIds.Length == 0) return;
        if (!leftLauncher || !rightLauncher) return;

        string id = projectilePoolIds[_seq % projectilePoolIds.Length];
        _seq++;

        Transform launcher = (_side < 0) ? leftLauncher : rightLauncher;

        // Root spawn position: launcher X/Y, Z oyuncunun önünde
        Vector3 pos = launcher.position;

        // Player Z sabitse 0 kabul; değilse player transformdan alırsın.
        float playerZ = 0f;
        pos.z = playerZ + spawnAheadMeters;

        var go = ItemPooler.Instance.Spawn(id, pos, Quaternion.identity);
        if (!go) return;

        var proj = go.GetComponent<AudienceProjectile>();
        if (!proj)
        {
            Debug.LogError($"[AudienceAttack] Spawned object '{id}' has no AudienceProjectile component.");
            return;
        }

        // Landing X: edge-biased, side işaretli
        float landingX = SampleEdgeBiased(landingEdgeMinX, landingEdgeMaxX, edgeBias) * Mathf.Sign(_side);
        Vector3 endLocal = new Vector3(landingX, landingLocalY, 0f); // Z local=0, çünkü Z'yi WorldScrollMover taşıyor

        proj.BeginThrow(endLocal, throwDuration, arcHeight);
    }

    private float SampleEdgeBiased(float min, float max, float bias)
    {
        float t = Mathf.Pow(Random.value, Mathf.Max(1f, bias));
        return Mathf.Lerp(min, max, t);
    }
}*/

// Scripts/AudienceAttack/AudienceAttackDirector.cs
using UnityEngine;

public class AudienceAttackDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SessionDirector sessionDirector;
    [SerializeField] private Transform leftLauncher;
    [SerializeField] private Transform rightLauncher;

    [Header("Projectile Pool Ids (sequence)")]
    [SerializeField] private string[] projectilePoolIds;

    [Header("Spawn (distance-based)")]
    [SerializeField] private float spawnAheadMeters = 75f;
    [SerializeField] private float spawnEveryMeters = 22f;

    [Header("Ground")]
    [SerializeField] private float groundWorldY = 0f;

    [Header("Landing X (edge-biased)")]
    [SerializeField] private float landingEdgeMinX = 5.5f;
    [SerializeField] private float landingEdgeMaxX = 9.5f;
    [SerializeField] private float edgeBias = 2.2f;

    [Header("Throw Shape")]
    [SerializeField] private float throwDuration = 0.55f;
    [SerializeField] private float arcHeight = 3.5f;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private bool _active;
    private float _nextSpawnDistance;
    private int _side = -1; // -1 left, +1 right
    private int _seq;

    private IRunDistance _run;

    private void Awake()
    {
        if (!sessionDirector) sessionDirector = FindFirstObjectByType<SessionDirector>();
        _run = GameManager.Instance;
    }

    private void OnEnable()
    {
        if (sessionDirector)
        {
            sessionDirector.OnWaveStarted += HandleWaveStarted;
            sessionDirector.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDisable()
    {
        if (sessionDirector)
        {
            sessionDirector.OnWaveStarted -= HandleWaveStarted;
            sessionDirector.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandleWaveStarted(int waveIndex, int totalWaves)
    {
        if (_run == null) _run = GameManager.Instance;

        bool should = AudienceAttackRules.ShouldBeActiveOnWave(sessionDirector.Difficulty, waveIndex, totalWaves);

        if (should)
        {
            _active = true;
            _nextSpawnDistance = (_run != null ? _run.RunDistance : 0f) + 8f;
            _side = -_side;

            if (log) Debug.Log($"[AudienceAttack] START (wave {waveIndex}/{totalWaves}, diff={sessionDirector.Difficulty})");
        }
        else
        {
            _active = false;
        }
    }

    private void HandlePhaseChanged(SessionDirector.Phase phase)
    {
        if (phase != SessionDirector.Phase.Wave)
            _active = false;
    }

    private void Update()
    {
        if (!_active) return;
        if (_run == null) return;
        if (ItemPooler.Instance == null) return;

        float d = _run.RunDistance;
        if (d < _nextSpawnDistance) return;

        SpawnOne();

        _nextSpawnDistance = d + Mathf.Max(1f, spawnEveryMeters);
        _side = -_side;
    }

    private void SpawnOne()
    {
        if (projectilePoolIds == null || projectilePoolIds.Length == 0) return;
        if (!leftLauncher || !rightLauncher) return;

        string id = projectilePoolIds[_seq % projectilePoolIds.Length];
        _seq++;

        Transform launcher = (_side < 0) ? leftLauncher : rightLauncher;

        // Root spawn position
        Vector3 pos = launcher.position;
        float playerZ = 0f;
        pos.z = playerZ + spawnAheadMeters;

        var go = ItemPooler.Instance.Spawn(id, pos, Quaternion.identity);
        if (!go) return;

        var proj = go.GetComponent<AudienceProjectile>();
        if (!proj) return;

        // ✅ KRİTİK FIX: Start/End world Z = root Z
        Vector3 startWorld = launcher.position;
        startWorld.z = go.transform.position.z;

        float landingX = SampleEdgeBiased(landingEdgeMinX, landingEdgeMaxX, edgeBias) * Mathf.Sign(_side);
        Vector3 endWorld = new Vector3(landingX, groundWorldY, go.transform.position.z);

        proj.BeginThrowWorld(startWorld, endWorld, throwDuration, arcHeight);
    }

    private float SampleEdgeBiased(float min, float max, float bias)
    {
        float t = Mathf.Pow(Random.value, Mathf.Max(1f, bias));
        return Mathf.Lerp(min, max, t);
    }
}