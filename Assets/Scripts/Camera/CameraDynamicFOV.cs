using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraDynamicFOV : MonoBehaviour
{
    [Header("FOV Ayarları")]
    [SerializeField] private float _baseFOV = 60f;      // Normal FOV
    [SerializeField] private float _maxFOV = 75f;       // Ulaşılabilecek Max FOV
    [SerializeField] private float _smoothTime = 0.2f;  // Hedefe kaç saniyede varsın? (Düşük = Çok Hızlı)

    [Header("Hız Etkileşimi")]
    // Sprint'e basıldığı an, hızın ne olursa olsun eklenecek garanti FOV miktarı.
    [SerializeField] private float _guaranteedBoost = 2f; 
    
    // Hızın etkisinin başladığı alt limit (Oyunun en yavaş hali)
    [SerializeField] private float _minSpeedRef = 10f; 
    // Hızın etkisinin tavan yaptığı üst limit (Oyunun çok hızlandığı hal)
    [SerializeField] private float _maxSpeedRef = 50f; 

    private Camera _cam;
    private ISprinter _playerSprinter;
    private float _currentVelocity; // SmoothDamp için referans değişken

    // Debug için: Hedef FOV'u Inspector'da görmek istersen
    [SerializeField, ReadOnlyInspector] private float _debugTargetFOV; 

    private void Start()
    {
        _cam = GetComponent<Camera>();
        _cam.fieldOfView = _baseFOV;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _playerSprinter = playerObj.GetComponent<ISprinter>();
        }
    }

    private void Update()
    {
        float targetFOV = _baseFOV;

        if (_playerSprinter != null && _playerSprinter.IsSprinting)
        {
            float currentSpeed = GameManager.Instance.CurrentSpeed;

            // 1. Hız Faktörünü Hesapla (0 ile 1 arası)
            float t = Mathf.InverseLerp(_minSpeedRef, _maxSpeedRef, currentSpeed);

            // 2. Ekstra FOV Miktarı
            // Garanti etki + (Kalan boşluk * Hız Faktörü)
            // Örn: Hız düşükse sadece 2 artar. Hız yüksekse MaxFOV'a kadar çıkar.
            float speedBonus = (_maxFOV - _baseFOV - _guaranteedBoost) * t;
            
            targetFOV = _baseFOV + _guaranteedBoost + speedBonus;
        }

        _debugTargetFOV = targetFOV; // Inspector'dan izleyebilmen için

        // 3. SmoothDamp ile Pürüzsüz ve Hızlı Geçiş
        _cam.fieldOfView = Mathf.SmoothDamp(
            _cam.fieldOfView, 
            targetFOV, 
            ref _currentVelocity, 
            _smoothTime
        );
    }
}

// Inspector'da gri renkte sadece okunabilir alan yapmak için basit bir attribute (Opsiyonel)
// Bunu kullanmak istemezsen yukarıdaki [ReadOnlyInspector] kısmını silebilirsin.
public class ReadOnlyInspectorAttribute : PropertyAttribute { }