using UnityEngine;

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
}