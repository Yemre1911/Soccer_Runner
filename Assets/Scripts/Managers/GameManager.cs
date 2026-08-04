using UnityEngine;

public class GameManager : MonoBehaviour, IGameSpeed, IRunDistance
{
    public static GameManager Instance { get; private set; }

    [Header("Yol Akış Ayarları")]
    [SerializeField] private float _initialSpeed = 10f;
    [SerializeField] private float _acceleration = 0.1f;

    [Header("Speed Caps")]
    [Tooltip("BaseSpeed bu değeri geçmez (zamanla artan ham hız tavanı).")]
    [SerializeField] private float _baseSpeedCap = 17f;

    [Tooltip("Gerçek dünya hızı (BaseSpeed * multiplier) bu değeri geçmez.")]
    [SerializeField] private float _worldSpeedCap = 17f;

    [Header("Global Pace")]
    [Tooltip("Tüm koşu hız eğrisini çarpar. 1.65 = oyunun tamamı yaklaşık %65 daha hızlı akar.")]
    [SerializeField, Min(0.1f)] private float _gameplaySpeedMultiplier = 1.20f;

    [SerializeField] private float _baseSpeed;
    [SerializeField] private float _speedMultiplier = 1f;

    public float RunDistance { get; private set; }

    public float InitialSpeed => _initialSpeed;
    public float BaseSpeed => _baseSpeed;
    public float SpeedMultiplier => _speedMultiplier;
    public float GameplaySpeedMultiplier => _gameplaySpeedMultiplier;

    public bool SimulationPaused => _simulationPaused;
    public bool WorldMovementForcedStopped => _worldMovementForcedStopped;

    private bool _simulationPaused;
    private bool _worldMovementForcedStopped;

    public float CurrentSpeed
    {
        get
        {
            if (_simulationPaused || _worldMovementForcedStopped)
                return 0f;

            float raw = _baseSpeed * _speedMultiplier;
            float cappedSpeed = Mathf.Min(raw, _worldSpeedCap);
            return cappedSpeed * Mathf.Max(0.1f, _gameplaySpeedMultiplier);
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        ResetRuntimeState();
    }

    private void Update()
    {
        if (_simulationPaused)
            return;

        _baseSpeed += _acceleration * Time.deltaTime;
        _baseSpeed = Mathf.Min(_baseSpeed, _baseSpeedCap);

        RunDistance += CurrentSpeed * Time.deltaTime;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void SetGameplaySpeedMultiplier(float multiplier)
    {
        _gameplaySpeedMultiplier = Mathf.Max(0.1f, multiplier);
    }

    public void SetSimulationPaused(bool paused)
    {
        _simulationPaused = paused;
    }

    public void SetWorldMovementForcedStopped(bool stopped)
    {
        _worldMovementForcedStopped = stopped;
    }

    public void ResetRunDistance()
    {
        RunDistance = 0f;
    }

    public void ResetRuntimeState()
    {
        _baseSpeed = _initialSpeed;
        _speedMultiplier = 1f;
        _simulationPaused = false;
        _worldMovementForcedStopped = false;
        RunDistance = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}