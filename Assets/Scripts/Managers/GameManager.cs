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

    [SerializeField] private float _baseSpeed;
    [SerializeField] private float _speedMultiplier = 1f;

    public float RunDistance { get; private set; }

    public float InitialSpeed => _initialSpeed;
    public float BaseSpeed => _baseSpeed;
    public float SpeedMultiplier => _speedMultiplier;
    public bool SimulationPaused => _simulationPaused;

    private bool _simulationPaused;

    public float CurrentSpeed
    {
        get
        {
            float raw = _baseSpeed * _speedMultiplier;
            return Mathf.Min(raw, _worldSpeedCap);
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

    public void SetSimulationPaused(bool paused)
    {
        _simulationPaused = paused;
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
        RunDistance = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}