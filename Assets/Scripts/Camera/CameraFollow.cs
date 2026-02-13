/*using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Hedef")]
    [SerializeField] private Transform _target; // PlayerRoot buraya sürüklenecek

    [Header("Ayarlar")]
    [SerializeField] private float _followSpeed = 5f; // Kameranın takip hızı (Düşük = Daha ağır/sinematik)
    [SerializeField] private Vector3 _offset; // Kamera karakterden ne kadar uzakta duracak?

    private void Start()
    {
        // Eğer editörde offset girmezsen, oyun başladığındaki mesafeyi korur.
        if (_target != null)
        {
            _offset = transform.position - _target.position;
        }
    }

    // Kamera takipleri her zaman LateUpdate'te yapılır!
    // Sebebi: Player Update'te hareket eder, Camera LateUpdate'te onu takip eder. Titremeyi önler.
    private void LateUpdate()
    {
        if (_target == null) return;

        // Hedef pozisyon:
        // X: Karakterin X'i + Offset X
        // Y: Kameranın kendi Y'si (Sabit kalsın veya Offset Y) -> Genelde runnerlarda Y sabit kalırsa daha temiz durur.
        // Z: Kameranın kendi Z'si (Sabit, çünkü dünya akıyor)
        
        Vector3 targetPosition = transform.position;
        
        // Sadece X ekseninde takip istiyoruz (Subway Surfers stili)
        targetPosition.x = _target.position.x + _offset.x;

        // Lerp: Mevcut konumdan hedef konuma yumuşak geçiş
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, _followSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }
}*/

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Hedef")]
    [SerializeField] private Transform _target;

    [Header("Takip")]
    [SerializeField] private float _followSpeed = 5f;
    [SerializeField] private Vector3 _offset;

    [Header("X Takip Azaltma")]
    [SerializeField] private float _maxPlayerX = 4f;

    [Tooltip("Merkezde X takibi (1 = tam takip)")]
    [Range(0f, 1f)] [SerializeField] private float _followFactorCenter = 1f;

    [Tooltip("Kenarda X takibi (0.3 = daha az takip)")]
    [Range(0f, 1f)] [SerializeField] private float _followFactorEdge = 0.35f;

    [Header("Yaw-only Rotasyon")]
    [SerializeField] private float _yawSpeed = 6f;

    // Başlangıç rotasyonunu korumak için
    private Quaternion _baseRotation;

    private void Start()
    {
        _baseRotation = transform.rotation;

        if (_target != null)
            _offset = transform.position - _target.position;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // 0..1 (merkez=0, kenar=1)
        float t = (_maxPlayerX > 0.0001f)
            ? Mathf.Clamp01(Mathf.Abs(_target.position.x) / _maxPlayerX)
            : 0f;

        // İstersen yumuşat:
        // t = t*t*(3f - 2f*t); // smoothstep

        // Kenara gidince X takibi düşsün
        float followFactor = Mathf.Lerp(_followFactorCenter, _followFactorEdge, t);

        // --- POZİSYON (sadece X takip; Y ve Z aynı kalsın) ---
        Vector3 desiredPos = transform.position;

        // Temel hedef X: target + offset
        float targetX = _target.position.x + _offset.x;

        // Takip azaltma: X'e tam gitme, kısmi git
        // (Lane merkezinin dünya X=0 olduğunu varsayar; runner'larda tipik)
        desiredPos.x = Mathf.Lerp(_offset.x, targetX, followFactor);

        transform.position = Vector3.Lerp(transform.position, desiredPos, _followSpeed * Time.deltaTime);

        // --- ROTASYON (sadece yaw değişsin; pitch/roll başlangıçtaki gibi kalsın) ---
        // Hedefe bakış yönünden sadece yaw al
        Vector3 dir = _target.position - transform.position;
        dir.y = 0f; // yaw-only yön vektörü

        if (dir.sqrMagnitude > 0.0001f)
        {
            float desiredYaw = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;

            Vector3 baseEuler = _baseRotation.eulerAngles; // pitch/roll buradan korunacak
            Quaternion desiredRot = Quaternion.Euler(baseEuler.x, desiredYaw, baseEuler.z);

            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, _yawSpeed * Time.deltaTime);
        }
    }
}