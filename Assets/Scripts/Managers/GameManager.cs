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

    // Ham hız (zamanla artar)
    [SerializeField] private float _baseSpeed;

    // Sprint çarpanı
    [SerializeField] private float _speedMultiplier = 1f;

    // Mesafe takibi (distance-based spawn için)
    public float RunDistance { get; private set; }

    public float InitialSpeed => _initialSpeed;
    public float BaseSpeed => _baseSpeed;
    public float SpeedMultiplier => _speedMultiplier;

    // Asıl dünya hızı (cap'li)
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

        _baseSpeed = _initialSpeed;
        RunDistance = 0f;
    }

    private void Update()
    {
        // 1) Base speed zamanla artsın ama cap'i geçmesin
        _baseSpeed += _acceleration * Time.deltaTime;
        _baseSpeed = Mathf.Min(_baseSpeed, _baseSpeedCap);

        // 2) Distance biriktir (cap'li CurrentSpeed ile)
        RunDistance += CurrentSpeed * Time.deltaTime;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = Mathf.Max(0f, multiplier);
        // CurrentSpeed zaten cap'li olduğu için burada ekstra clamp şart değil.
    }

    /// <summary>
    /// (Opsiyonel) Yeni level/run başlarken distance sıfırlamak için.
    /// </summary>
    public void ResetRunDistance()
    {
        RunDistance = 0f;
    }
}