// Scripts/AudienceAttack/AudienceAttackDirector.cs
/*using UnityEngine;

public class AudienceAttackDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SessionDirector sessionDirector;
    [SerializeField] private Transform leftLauncher;
    [SerializeField] private Transform rightLauncher;
    [SerializeField] private SimpleCameraShake cameraShake;

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

    [Header("Attack Start Delay")]
    [SerializeField] private float firstSpawnDelayMeters = 8f;

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
        if (sessionDirector != null)
        {
            sessionDirector.OnWaveStarted += HandleWaveStarted;
            sessionDirector.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDisable()
    {
        if (sessionDirector != null)
        {
            sessionDirector.OnWaveStarted -= HandleWaveStarted;
            sessionDirector.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandleWaveStarted(int waveIndex, int totalWaves)
    {
        if (_run == null) _run = GameManager.Instance;

        bool shouldStartAttackThisWave =
            AudienceAttackRules.ShouldBeActiveOnWave(sessionDirector.Difficulty, waveIndex, totalWaves);

        // Sadece bu wave başında attack başlatılacaksa aç.
        // Kapatmayı burada yapmıyoruz. Attack başladıysa, wave bitene kadar sürsün.
        if (!shouldStartAttackThisWave) return;

        _active = true;
        _nextSpawnDistance = (_run != null ? _run.RunDistance : 0f) + Mathf.Max(0f, firstSpawnDelayMeters);
        _side = -_side;

        if (cameraShake != null)
            cameraShake.SetAmbientActive(true);

        if (log)
            Debug.Log($"[AudienceAttack] START (wave {waveIndex}/{totalWaves}, diff={sessionDirector.Difficulty})");
    }

    private void HandlePhaseChanged(SessionDirector.Phase phase)
    {
        // Attack başladıktan sonra sadece wave fazı bitince kapansın.
        if (phase != SessionDirector.Phase.Wave)
        {
            _active = false;

            if (cameraShake != null)
                cameraShake.SetAmbientActive(false);

            if (log)
                Debug.Log($"[AudienceAttack] STOP (phase={phase})");
        }
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

        Vector3 pos = launcher.position;
        float playerZ = 0f;
        pos.z = playerZ + spawnAheadMeters;

        GameObject go = ItemPooler.Instance.Spawn(id, pos, Quaternion.identity);
        if (!go) return;

        AudienceProjectile proj = go.GetComponent<AudienceProjectile>();
        if (!proj) return;

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
}*/

using UnityEngine;

public class AudienceAttackDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SessionDirector sessionDirector;
    [SerializeField] private Transform leftLauncher;
    [SerializeField] private Transform rightLauncher;
    [SerializeField] private SimpleCameraShake cameraShake;

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

    [Header("Attack Start Delay")]
    [SerializeField] private float firstSpawnDelayMeters = 8f;

    [Header("Debug")]
    [SerializeField] private bool log = true;

    private bool _active;
    private float _nextSpawnDistance;
    private int _side = -1; // -1 left, +1 right
    private int _seq;

    private GameManager _gameManager;
    private Transform _player;

    private void Awake()
    {
        if (!sessionDirector) sessionDirector = FindFirstObjectByType<SessionDirector>();
        ResolveRefs(true);
        ResetState();
    }

    private void OnEnable()
    {
        ResolveRefs(true);

        if (sessionDirector != null)
        {
            sessionDirector.OnWaveStarted -= HandleWaveStarted;
            sessionDirector.OnWaveStarted += HandleWaveStarted;

            sessionDirector.OnPhaseChanged -= HandlePhaseChanged;
            sessionDirector.OnPhaseChanged += HandlePhaseChanged;
        }
    }

    private void OnDisable()
    {
        if (sessionDirector != null)
        {
            sessionDirector.OnWaveStarted -= HandleWaveStarted;
            sessionDirector.OnPhaseChanged -= HandlePhaseChanged;
        }

        StopAttack();
    }

    private void ResolveRefs(bool force = false)
    {
        if (force || _gameManager == null)
            _gameManager = GameManager.Instance != null
                ? GameManager.Instance
                : FindFirstObjectByType<GameManager>();

        if (force || _player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            _player = p != null ? p.transform : null;
        }

        if (!sessionDirector)
            sessionDirector = FindFirstObjectByType<SessionDirector>();
    }

    private void ResetState()
    {
        _active = false;
        _nextSpawnDistance = 0f;
        _side = -1;
        _seq = 0;

        if (cameraShake != null)
            cameraShake.SetAmbientActive(false);
    }

    private void StopAttack()
    {
        _active = false;

        if (cameraShake != null)
            cameraShake.SetAmbientActive(false);
    }

    private void HandleWaveStarted(int waveIndex, int totalWaves)
    {
        ResolveRefs(true);

        bool shouldStartAttackThisWave =
            AudienceAttackRules.ShouldBeActiveOnWave(sessionDirector.Difficulty, waveIndex, totalWaves);

        if (!shouldStartAttackThisWave) return;

        float currentDistance = _gameManager != null ? _gameManager.RunDistance : 0f;

        _active = true;
        _nextSpawnDistance = currentDistance + Mathf.Max(0f, firstSpawnDelayMeters);
        _side = -_side;

        if (cameraShake != null)
            cameraShake.SetAmbientActive(true);

        if (log)
            Debug.Log($"[AudienceAttack] START (wave {waveIndex}/{totalWaves}, diff={sessionDirector.Difficulty})");
    }

    private void HandlePhaseChanged(SessionDirector.Phase phase)
    {
        if (phase != SessionDirector.Phase.Wave)
        {
            StopAttack();

            if (log)
                Debug.Log($"[AudienceAttack] STOP (phase={phase})");
        }
    }

    private void Update()
    {
        if (!_active) return;

        if (_gameManager == null || _player == null)
            ResolveRefs();

        if (_gameManager == null) return;
        if (ItemPooler.Instance == null) return;

        float d = _gameManager.RunDistance;
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

        float playerZ = _player != null ? _player.position.z : 0f;

        Vector3 pos = launcher.position;
        pos.z = playerZ + spawnAheadMeters;

        GameObject go = ItemPooler.Instance.Spawn(id, pos, Quaternion.identity);
        if (!go) return;

        AudienceProjectile proj = go.GetComponent<AudienceProjectile>();
        if (!proj) return;

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