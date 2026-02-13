using UnityEngine;

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
}