/*using UnityEngine;

public class GameManager : MonoBehaviour, IGameSpeed
{
    // Singleton: Her yerden ulaşabilmek için (GameManager.Instance.CurrentSpeed gibi)
    public static GameManager Instance { get; private set; }

    [Header("Yol Akış Ayarları")]
    private float _initialSpeed = 10f; // Başlangıç hızı
    private float _acceleration = 0.1f; // Zamanla hızlanma miktarı

    // Interface implementasyonu
    public float CurrentSpeed { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CurrentSpeed = _initialSpeed;
    }

    private void Update()
    {
        // Zamanla oyunun hızını (zorluğunu) artır
        CurrentSpeed += _acceleration * Time.deltaTime;
    }
}*/

using UnityEngine;

public class GameManager : MonoBehaviour, IGameSpeed
{
    public static GameManager Instance { get; private set; }

    [Header("Yol Akış Ayarları")]
    private float _initialSpeed = 10f;
    private float _acceleration = 0.1f;

    // Ham hız (Zamanla artan zorluk hızı)
    private float _baseSpeed;

    // Sprint çarpanı (Normalde 1, sprintte 2 vs.)
    private float _speedMultiplier = 1f;

    // Interface implementasyonu: Base hız * Çarpan
    public float CurrentSpeed => _baseSpeed * _speedMultiplier;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _baseSpeed = _initialSpeed;
    }

    private void Update()
    {
        // Sadece base hızı zamanla artırıyoruz, multiplier anlık değişir.
        _baseSpeed += _acceleration * Time.deltaTime;
    }

    // Sprint sistemi burayı tetikleyecek
    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;
    }
}