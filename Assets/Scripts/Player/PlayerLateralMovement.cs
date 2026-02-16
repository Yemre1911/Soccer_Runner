using UnityEngine;

public class PlayerLateralMovement : MonoBehaviour, IMover
{
    [Header("Adım Ayarları")]
    [SerializeField] private float stepSize = 5f;
    [SerializeField] private float roadWidth = 10f;

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