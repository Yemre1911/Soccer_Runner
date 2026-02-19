/*using UnityEngine;

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
    [Range(0f, 1f)] private float _followFactorEdge = 1f;

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
}*/
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Hedef")]
    [SerializeField] private Transform _target;

    [Header("Lane / Track Merkezi (ÖNEMLİ)")]
    [Tooltip("Orta şeridi temsil eden referans. Player'ın sağ/solu bu objenin local X'ine göre hesaplanır.\n" +
             "Örn: TrackRoot, LaneCenter, LevelRoot.")]
    [SerializeField] private Transform _laneCenterRef;

    [Header("Takip")]
    [SerializeField] private float _followSpeed = 5f;

    [Tooltip("Kamera ile hedef arasındaki başlangıç offset'i otomatik alınır. İstersen Inspector'dan da elle verebilirsin.")]
    [SerializeField] private Vector3 _offset;

    [Header("X Takip Azaltma")]
    [Tooltip("Player merkezden ne kadar uzaklaşınca 'kenar' sayılacak? (şerit sınırı gibi düşün)")]
    [SerializeField] private float _maxPlayerX = 4f;

    [Tooltip("Merkezde X takibi (1 = tam takip)")]
    [Range(0f, 1f)][SerializeField] private float _followFactorCenter = 1f;

    [Tooltip("Kenarda X takibi (0.3 = daha az takip)")]
    [Range(0f, 1f)][SerializeField] private float _followFactorEdge = 0.6f;

    [Header("Yaw-only Rotasyon")]
    [SerializeField] private float _yawSpeed = 6f;

    [Header("Kenar Görüşü (Karşı tarafa döndürme)")]
    [Tooltip("Player kenara gittikçe kameraya eklenecek max yaw offset (derece). Örn: 12-25")]
    [Range(0f, 45f)]
    [SerializeField] private float _maxCounterYaw = 18f;

    [Tooltip("Merkezdeyken (küçük x hareketlerinde) yaw offset uygulama. 0.05-0.15 iyi başlar.")]
    [Range(0f, 1f)]
    [SerializeField] private float _centerDeadZone01 = 0.08f;

    [Tooltip("Offset etkisinin eğrisi. X=0 merkez, X=1 kenar. Genelde EaseInOut güzel.")]
    [SerializeField]
    private AnimationCurve _counterYawCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Debug (Opsiyonel)")]
    [SerializeField] private bool _debugDraw = false;

    private Quaternion _baseRotation;

    private void Start()
    {
        _baseRotation = transform.rotation;

        if (_target != null)
            _offset = transform.position - _target.position;

        // Eğer laneCenterRef verilmemişse, yine de çalışsın diye target'ın parent'ını dene.
        // (Ama en doğru yöntem Inspector'dan vermek.)
        if (_laneCenterRef == null && _target != null && _target.parent != null)
            _laneCenterRef = _target.parent;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        // 1) Player'ın merkeze göre X offset'ini bul (simetrik ve doğru)
        float playerXFromCenter = GetPlayerXFromCenter();

        // 2) 0..1 kenar oranı (merkez=0, kenar=1)
        float tEdge = (_maxPlayerX > 0.0001f)
            ? Mathf.Clamp01(Mathf.Abs(playerXFromCenter) / _maxPlayerX)
            : 0f;

        // Deadzone
        if (tEdge < _centerDeadZone01) tEdge = 0f;
        else tEdge = Mathf.InverseLerp(_centerDeadZone01, 1f, tEdge);

        // 3) X takip azaltma
        float followFactor = Mathf.Lerp(_followFactorCenter, _followFactorEdge, tEdge);

        // --- POZİSYON ---
        Vector3 desiredPos = transform.position;

        // Hedefin world X'i + offset (kamera hedefe göre nerede durmalı)
        float desiredTargetX = _target.position.x + _offset.x;

        // "Merkez kamera X" değeri: laneCenterRef'in world X'i + offset.x
        // Böylece lane merkezi world X=0 olmasa bile merkez doğru olur.
        float centerWorldX = GetCenterWorldX() + _offset.x;

        // Merkezde: centerWorldX'e yakın dur, kenarda: hedefin X'ine daha çok yaklaş (veya tam tersi istersen)
        desiredPos.x = Mathf.Lerp(centerWorldX, desiredTargetX, followFactor);

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            _followSpeed * Time.deltaTime
        );

        // --- ROTASYON (Yaw-only) ---
        Vector3 dir = _target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.0001f)
        {
            float lookYaw = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;

            // Player sağda (+) => kamera sola dönsün (-yaw)
            // Player solda (-) => kamera sağa dönsün (+yaw)
            float sign = Mathf.Sign(playerXFromCenter);
            float curve01 = Mathf.Clamp01(_counterYawCurve.Evaluate(tEdge));
            float counterYaw = -sign * _maxCounterYaw * curve01;

            float desiredYaw = lookYaw + counterYaw;

            Vector3 baseEuler = _baseRotation.eulerAngles;
            Quaternion desiredRot = Quaternion.Euler(baseEuler.x, desiredYaw, baseEuler.z);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRot,
                _yawSpeed * Time.deltaTime
            );
        }

        if (_debugDraw)
        {
            Debug.DrawLine(transform.position, _target.position, Color.white);
        }
    }

    private float GetPlayerXFromCenter()
    {
        if (_laneCenterRef == null)
        {
            // Fallback: world X'e göre (en azından çalışsın)
            return _target.position.x;
        }

        // Player pozisyonunu laneCenterRef'in local space'ine çevir.
        // Bu local space'te X=0 "orta şerit" demek.
        Vector3 local = _laneCenterRef.InverseTransformPoint(_target.position);
        return local.x;
    }

    private float GetCenterWorldX()
    {
        if (_laneCenterRef == null) return 0f;

        // laneCenterRef'in world konumu "merkez" kabul edilir
        return _laneCenterRef.position.x;
    }
}
