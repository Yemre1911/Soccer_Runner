using UnityEngine;

public class GameManager : MonoBehaviour, IGameSpeed
{
    public static GameManager Instance { get; private set; }

    [Header("Yol Akış Ayarları")]
    [SerializeField] private float _initialSpeed = 10f;
    [SerializeField] private float _acceleration = 0.1f;

    // Ham hız (zamanla artar)
    [SerializeField] private float _baseSpeed;

    // Sprint çarpanı
    [SerializeField] private float _speedMultiplier = 1f;

    public float InitialSpeed => _initialSpeed;
    public float BaseSpeed => _baseSpeed;
    public float SpeedMultiplier => _speedMultiplier;

    // Asıl dünya hızı
    public float CurrentSpeed => _baseSpeed * _speedMultiplier;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        _baseSpeed = _initialSpeed;
    }

    private void Update()
    {
        _baseSpeed += _acceleration * Time.deltaTime;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = Mathf.Max(0f, multiplier);
    }
}