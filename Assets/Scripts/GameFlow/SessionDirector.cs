/*using UnityEngine;
using System;
using System.Collections;

public class SessionDirector : MonoBehaviour
{
    public float PhaseStartDistanceDebug => _phaseStartDistance;
    public float LevelLengthDebug => _config != null ? _config.levelLengthMeters : 0f;

    public enum Phase
    {
        Idle,
        Starting,
        Wave,
        Break,
        Ending
    }

    [Header("References")]
    [SerializeField] private OpponentSpawner opponentSpawner;

    [Header("Starting Phase")]
    [Tooltip("Run başlarken rakip çıkmasın; oyuncuya ısınma süresi (metre).")]
    [SerializeField] private float startingDistance = 25f;

    [Header("Wave / Break Pattern (meters)")]
    [SerializeField] private float waveDistance = 400f;
    [SerializeField] private float breakDistance = 100f;

    [Header("Debug")]
    [SerializeField] private bool logPhaseChanges = true;

    public Phase CurrentPhase { get; private set; } = Phase.Idle;

    public event Action<Phase> OnPhaseChanged;

    private SessionConfig _config;
    private float _phaseStartDistance;
    private bool _running;

    private Coroutine _subscribeRoutine;
    private bool _subscribedGameOver;

    private void Awake()
    {
        if (!opponentSpawner)
            opponentSpawner = FindFirstObjectByType<OpponentSpawner>();
    }

    private void OnEnable()
    {
        if (_subscribeRoutine != null) StopCoroutine(_subscribeRoutine);
        _subscribeRoutine = StartCoroutine(SubscribeGameOverWhenReady());
    }

    private void OnDisable()
    {
        if (_subscribeRoutine != null)
        {
            StopCoroutine(_subscribeRoutine);
            _subscribeRoutine = null;
        }

        if (GameOver.Instance != null && _subscribedGameOver)
        {
            GameOver.Instance.OnGameLost -= HandleGameLost;
            _subscribedGameOver = false;
        }
    }

    public void Begin(SessionConfig config)
    {
        _config = config;
        _running = true;

        // Level uzunluğu yoksa güvenli fallback
        if (_config.levelLengthMeters <= 0f)
            _config.levelLengthMeters = 1200f;

        // Spawner yoksa yine de phase çalışsın ama log ver
        if (!opponentSpawner)
            Debug.LogWarning("[SessionDirector] OpponentSpawner not found in scene.");

        // Distance reset (istersen)
        if (GameManager.Instance != null)
            GameManager.Instance.ResetRunDistance();

        EnterPhase(Phase.Starting);
    }

    private void Update()
    {
        if (!_running) return;
        if (GameManager.Instance == null) return;

        float d = GameManager.Instance.RunDistance;

        // Level bitiş
        if (CurrentPhase != Phase.Ending && d >= _config.levelLengthMeters)
        {
            EnterPhase(Phase.Ending);
            return;
        }

        // Phase progression (Ending’de ilerleme yok)
        if (CurrentPhase == Phase.Starting)
        {
            if (d - _phaseStartDistance >= startingDistance)
                EnterPhase(Phase.Wave);
        }
        else if (CurrentPhase == Phase.Wave)
        {
            if (d - _phaseStartDistance >= waveDistance)
                EnterPhase(Phase.Break);
        }
        else if (CurrentPhase == Phase.Break)
        {
            if (d - _phaseStartDistance >= breakDistance)
                EnterPhase(Phase.Wave);
        }
    }

    private void EnterPhase(Phase phase)
    {
        CurrentPhase = phase;
        _phaseStartDistance = (GameManager.Instance != null) ? GameManager.Instance.RunDistance : 0f;

        if (logPhaseChanges)
            Debug.Log($"[SessionDirector] Phase -> {phase} (startDistance={_phaseStartDistance:0.0})");

        // Spawner kontrolü
        if (opponentSpawner)
        {
            switch (phase)
            {
                case Phase.Starting:
                case Phase.Break:
                case Phase.Ending:
                    opponentSpawner.SetSpawningEnabled(false);
                    break;

                case Phase.Wave:
                    opponentSpawner.SetSpawningEnabled(true);
                    opponentSpawner.ResetSchedule(); // wave başında daha “temiz” his
                    break;
            }
        }

        OnPhaseChanged?.Invoke(phase);

        if (phase == Phase.Ending)
            HandleEnding();
    }

    private void HandleEnding()
    {
        _running = false;

        // Şimdilik sadece durduruyoruz. İleride: goal shot sequence.
        if (opponentSpawner)
            opponentSpawner.SetSpawningEnabled(false);

        Debug.Log("[SessionDirector] Ending reached. (TODO: Goal shot sequence)");
    }

    private void HandleGameLost()
    {
        if (!_running) return;
        _running = false;

        if (opponentSpawner)
            opponentSpawner.SetSpawningEnabled(false);

        Debug.Log("[SessionDirector] GameOver received -> session stopped.");
    }

    private IEnumerator SubscribeGameOverWhenReady()
    {
        while (GameOver.Instance == null)
            yield return null;

        GameOver.Instance.OnGameLost -= HandleGameLost;
        GameOver.Instance.OnGameLost += HandleGameLost;
        _subscribedGameOver = true;
    }
}*/

using UnityEngine;
using System;
using System.Collections;

public class SessionDirector : MonoBehaviour
{
    public float PhaseStartDistanceDebug => _phaseStartDistance;
    public float LevelLengthDebug => _config != null ? _config.levelLengthMeters : 0f;

    public enum Phase { Idle, Starting, Wave, Break, Ending }

    [Header("References")]
    [SerializeField] private OpponentSpawner opponentSpawner;

    [Header("Phase Defaults (used if no tuning provided)")]
    [SerializeField] private float startingDistance = 25f;
    [SerializeField] private float waveDistance = 400f;
    [SerializeField] private float breakDistance = 100f;

    [Header("Debug")]
    [SerializeField] private bool logPhaseChanges = true;

    public Phase CurrentPhase { get; private set; } = Phase.Idle;
    public event Action<Phase> OnPhaseChanged;

    private SessionConfig _config;
    private float _phaseStartDistance;
    private bool _running;

    private Coroutine _subscribeRoutine;
    private bool _subscribedGameOver;

    private void Awake()
    {
        if (!opponentSpawner)
            opponentSpawner = FindFirstObjectByType<OpponentSpawner>();
    }

    private void OnEnable()
    {
        if (_subscribeRoutine != null) StopCoroutine(_subscribeRoutine);
        _subscribeRoutine = StartCoroutine(SubscribeGameOverWhenReady());
    }

    private void OnDisable()
    {
        if (_subscribeRoutine != null)
        {
            StopCoroutine(_subscribeRoutine);
            _subscribeRoutine = null;
        }

        if (GameOver.Instance != null && _subscribedGameOver)
        {
            GameOver.Instance.OnGameLost -= HandleGameLost;
            _subscribedGameOver = false;
        }
    }

    public void Begin(SessionConfig config, SessionTuning tuning = null)
    {
        _config = config;
        _running = true;

        if (_config.levelLengthMeters <= 0f)
            _config.levelLengthMeters = 1200f;

        // Apply tuning (phase distances)
        if (tuning != null)
        {
            startingDistance = Mathf.Max(0f, tuning.startingDistance);
            waveDistance = Mathf.Max(1f, tuning.waveDistance);
            breakDistance = Mathf.Max(1f, tuning.breakDistance);

            if (logPhaseChanges)
                Debug.Log($"[SessionDirector] Tuning applied -> start={startingDistance}, wave={waveDistance}, break={breakDistance}");
        }

        if (!opponentSpawner)
            Debug.LogWarning("[SessionDirector] OpponentSpawner not found in scene.");

        if (GameManager.Instance != null)
            GameManager.Instance.ResetRunDistance();

        EnterPhase(Phase.Starting);
    }

    private void Update()
    {
        if (!_running) return;
        if (GameManager.Instance == null) return;

        float d = GameManager.Instance.RunDistance;

        if (CurrentPhase != Phase.Ending && d >= _config.levelLengthMeters)
        {
            EnterPhase(Phase.Ending);
            return;
        }

        if (CurrentPhase == Phase.Starting)
        {
            if (d - _phaseStartDistance >= startingDistance)
                EnterPhase(Phase.Wave);
        }
        else if (CurrentPhase == Phase.Wave)
        {
            if (d - _phaseStartDistance >= waveDistance)
                EnterPhase(Phase.Break);
        }
        else if (CurrentPhase == Phase.Break)
        {
            if (d - _phaseStartDistance >= breakDistance)
                EnterPhase(Phase.Wave);
        }
    }

    private void EnterPhase(Phase phase)
    {
        CurrentPhase = phase;
        _phaseStartDistance = (GameManager.Instance != null) ? GameManager.Instance.RunDistance : 0f;

        if (logPhaseChanges)
            Debug.Log($"[SessionDirector] Phase -> {phase} (startDistance={_phaseStartDistance:0.0})");

        if (opponentSpawner)
        {
            switch (phase)
            {
                case Phase.Starting:
                case Phase.Break:
                case Phase.Ending:
                    opponentSpawner.SetSpawningEnabled(false);
                    break;

                case Phase.Wave:
                    opponentSpawner.SetSpawningEnabled(true);
                    opponentSpawner.ResetSchedule();
                    break;
            }
        }

        OnPhaseChanged?.Invoke(phase);

        if (phase == Phase.Ending)
            HandleEnding();
    }

    private void HandleEnding()
    {
        _running = false;

        if (opponentSpawner)
            opponentSpawner.SetSpawningEnabled(false);

        Debug.Log("[SessionDirector] Ending reached. (TODO: Goal shot sequence)");
    }

    private void HandleGameLost()
    {
        if (!_running) return;
        _running = false;

        if (opponentSpawner)
            opponentSpawner.SetSpawningEnabled(false);

        Debug.Log("[SessionDirector] GameOver received -> session stopped.");
    }

    private IEnumerator SubscribeGameOverWhenReady()
    {
        while (GameOver.Instance == null)
            yield return null;

        GameOver.Instance.OnGameLost -= HandleGameLost;
        GameOver.Instance.OnGameLost += HandleGameLost;
        _subscribedGameOver = true;
    }
}