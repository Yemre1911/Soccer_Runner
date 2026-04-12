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

    public float StartingDistanceMeters => startingDistance;
    public float WaveDistanceMeters => waveDistance;
    public float BreakDistanceMeters => breakDistance;

    public Difficulty Difficulty => _config != null ? _config.difficulty : Difficulty.Easy;

    public int CurrentWaveIndex { get; private set; } = 0;
    public int TotalWaves { get; private set; } = 0;

    public event Action<int, int> OnWaveStarted;
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

        if (tuning != null)
        {
            startingDistance = Mathf.Max(0f, tuning.startingDistance);
            waveDistance = Mathf.Max(1f, tuning.waveDistance);
            breakDistance = Mathf.Max(1f, tuning.breakDistance);

            if (tuning.overrideLevelLength)
                _config.levelLengthMeters = Mathf.Max(1f, tuning.levelLengthMeters);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.ResetRunDistance();

        CurrentWaveIndex = 0;
        TotalWaves = ComputeTotalWaves(_config.levelLengthMeters, startingDistance, waveDistance, breakDistance);

        EnterPhase(Phase.Starting);
    }

    public void BeginManual(SessionConfig config, Phase initialPhase = Phase.Idle)
    {
        _config = config;
        _running = false;

        if (GameManager.Instance != null)
            GameManager.Instance.ResetRunDistance();

        CurrentWaveIndex = 0;
        TotalWaves = 0;

        EnterPhase(initialPhase);
    }

    public void SetManualPhase(Phase phase)
    {
        _running = false;
        EnterPhase(phase);
    }

    private void Update()
    {
        if (!_running) return;
        if (GameManager.Instance == null) return;
        if (_config == null) return;

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

        if (phase == Phase.Wave)
        {
            CurrentWaveIndex++;
            OnWaveStarted?.Invoke(CurrentWaveIndex, TotalWaves);
        }

        _phaseStartDistance = (GameManager.Instance != null) ? GameManager.Instance.RunDistance : 0f;

        if (opponentSpawner)
        {
            switch (phase)
            {
                case Phase.Idle:
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
    }

    private void HandleGameLost()
    {
        if (!_running) return;
        _running = false;

        if (opponentSpawner)
            opponentSpawner.SetSpawningEnabled(false);
    }

    private IEnumerator SubscribeGameOverWhenReady()
    {
        while (GameOver.Instance == null)
            yield return null;

        GameOver.Instance.OnGameLost -= HandleGameLost;
        GameOver.Instance.OnGameLost += HandleGameLost;
        _subscribedGameOver = true;
    }

    private int ComputeTotalWaves(float levelLength, float start, float wave, float brk)
    {
        levelLength = Mathf.Max(0f, levelLength);
        start = Mathf.Max(0f, start);
        wave = Mathf.Max(0.01f, wave);
        brk = Mathf.Max(0.01f, brk);

        float d = 0f;
        d += start;

        int waves = 0;
        bool inWave = true;

        while (d < levelLength)
        {
            if (inWave)
            {
                waves++;
                d += wave;
                inWave = false;
            }
            else
            {
                d += brk;
                inWave = true;
            }

            if (waves > 9999) break;
        }

        return Mathf.Max(1, waves);
    }
}