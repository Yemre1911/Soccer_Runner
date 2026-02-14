/*using UnityEngine;

public class PlayerLateralMovement : MonoBehaviour, IMover
{
    [Header("Adım Ayarları")]
    private float _stepSize = 3f; // Her basışta ne kadar yana kaysın?
    private float _roadWidth = 15f; // Yolun sınırları (-4 ile 4 arası)

    [Header("Şıpp Hissi")]
    private float _smoothTime = 0.1f; // Hareketin tamamlanma süresi (Düşük = Hızlı)
    private float _maxSpeed = 20f; // Kayma sırasındaki maksimum hız

    private float _targetX; // Gitmek istediğimiz nihai nokta
    private float _currentVelocity; // SmoothDamp için referans

    private void Start()
    {
        // Oyuna başladığımızda hedefimiz olduğumuz yerdir.
        _targetX = transform.position.x;
    }

    // Interface'den gelen emir: "Bir adım yana kay"
    public void MoveStep(int direction)
    {
        // Mevcut hedefin üzerine ekleme yapıyoruz (Böylece peş peşe basarsa birikir)
        _targetX += direction * _stepSize;

        // Hedefi yolun sınırları içinde tut (Yoldan çıkmasın)
        _targetX = Mathf.Clamp(_targetX, -_roadWidth, _roadWidth);
    }

    private void Update()
    {
        // Karakteri HER ZAMAN hedef noktaya (TargetX) doğru pürüzsüzce çek
        float newX = Mathf.SmoothDamp(transform.position.x, _targetX, ref _currentVelocity, _smoothTime, _maxSpeed);

        Vector3 newPos = transform.position;
        newPos.x = newX;
        transform.position = newPos;
    }
}*/

using UnityEngine;

public class PlayerLateralMovement : MonoBehaviour, IMover
{
    [Header("Adım Ayarları")]
    [SerializeField] private float stepSize = 3f;
    [SerializeField] private float roadWidth = 15f;

    [Header("Hız Ayarları (Upgrade İçin)")]
    // Upgrade sistemi burayı değiştirecek (Örn: 0.2 -> 0.1 -> 0.05)
    // Değer ne kadar KÜÇÜKSE o kadar HIZLI gider.
    public float MovementSmoothTime = 0.2f; 
    private float _maxSpeed = 30f; 

    // Topun "Karakter nereye gidiyor?" sorusunun cevabı:
    public float TargetX { get; private set; } 

    private float _currentVelocity;

    private void Start()
    {
        TargetX = transform.position.x;
    }

    public void MoveStep(int direction)
    {
        TargetX += direction * stepSize;
        TargetX = Mathf.Clamp(TargetX, -roadWidth, roadWidth);
    }

    private void Update()
    {
        float newX = Mathf.SmoothDamp(transform.position.x, TargetX, ref _currentVelocity, MovementSmoothTime, _maxSpeed);
        
        // Y ve Z pozisyonlarını koru, sadece X'i güncelle
        Vector3 newPos = transform.position;
        newPos.x = newX;
        transform.position = newPos;
    }

    // Upgrade sistemi çağırdığında bu metodu kullanacak
    public void SetSpeed(float newSmoothTime)
    {
        MovementSmoothTime = Mathf.Clamp(newSmoothTime, 0.05f, 0.5f);
    }
}