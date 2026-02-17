using UnityEngine;

public class PlayerLateralMovement : MonoBehaviour, IMover
{
    [Header("Adım Ayarları")]
    [SerializeField] private float stepSize = 5f;
    [SerializeField] private float roadWidth = 10f;

    [Header("Hız Ayarları")]
    public float MovementSmoothTime = 0.2f;
    private float _maxSpeed = 30f;

    [Header("Görsel Efektler")]
    [Tooltip("Sağa/Sola giderken maksimum dönüş açısı.")]
    [SerializeField] private float turnAngle = 90f;

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
        // 1. POZİSYON (Yumuşak Geçiş)
        float newX = Mathf.SmoothDamp(transform.position.x, TargetX, ref _currentVelocity, MovementSmoothTime, _maxSpeed);

        // Vektör tahsisi yapmamak için sadece x'i set edebiliriz ama bu yöntem okunabilir ve güvenlidir.
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        // 2. ROTASYON (Hıza Duyarlı)
        HandleRotation();
    }

    private void HandleRotation()
    {
        // Hız oranını al (-1 ile 1 arası)
        float velocityRatio = Mathf.Clamp(_currentVelocity / _maxSpeed, -1f, 1f);

        // Hedef rotasyonu hesapla
        Quaternion targetRot = Quaternion.Euler(0, velocityRatio * turnAngle, 0);

        // Yumuşakça dön (Lerp)
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 10f);
    }

    public void SetSpeed(float newSmoothTime)
    {
        MovementSmoothTime = Mathf.Clamp(newSmoothTime, 0.05f, 0.5f);
    }
}