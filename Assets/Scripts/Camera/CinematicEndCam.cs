using UnityEngine;

public class CinematicEndCam : MonoBehaviour
{
    [Header("Hedef")]
    [Tooltip("Karakterin Spine veya Hips kemiğini buraya sürükle")]
    [SerializeField] private Transform focusTarget;

    [Header("Fotoğraf Modu Ayarları")]
    [Tooltip("Kameranın karakterden ne kadar uzakta duracağı (Derinlik)")]
    [SerializeField] private float photoDistance = 8.0f; // 3 metre geride dur

    [Tooltip("Kameranın yerden/hedef kemikten yüksekliği. (0 = Kemiğin tam hizası, -0.5 = Hafif alttan bakış)")]
    private float photoHeight = 2.0f; // Tam göz/gövde hizası

    [Header("Yumuşatma Ayarları")]
    [Tooltip("Pozisyon ne kadar sürede otursun?")]
    [SerializeField] private float smoothTime = 1.0f;

    [Tooltip("Rotasyon ne kadar hızlı dönsün?")]
    [SerializeField] private float rotationSpeed = 5.0f;

    [Header("Kapatılacaklar")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable;

    private bool _isActive = false;
    private Vector3 _velocity;

    private void Start()
    {
        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost += TriggerZoom;
    }

    private void OnDestroy()
    {
        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost -= TriggerZoom;
    }

    [ContextMenu("Test Zoom")]
    public void TriggerZoom()
    {
        _isActive = true;

        foreach (var script in scriptsToDisable)
        {
            if (script != null) script.enabled = false;
        }

        transform.SetParent(null);
    }

    private void LateUpdate()
    {
        if (!_isActive || focusTarget == null) return;

        // --- 1. YENİ POZİSYON MANTIĞI (FOTOĞRAFÇI GİBİ) ---

        // A. Kameranın karakterle olan YATAY yönünü bul (Yüksekliği yok say)
        Vector3 flatDirection = transform.position - focusTarget.position;
        flatDirection.y = 0; // Kuş bakışını öldüren satır burası!
        flatDirection.Normalize();

        // B. Hedef pozisyonu sıfırdan inşa et:
        //    Karakterin Yeri + (Yatay Yön * Mesafe) + Senin İstediğin Yükseklik
        Vector3 targetPos = focusTarget.position + (flatDirection * photoDistance);
        targetPos.y = focusTarget.position.y + photoHeight;

        // C. Oraya git
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref _velocity,
            smoothTime
        );

        // --- 2. ROTASYON MANTIĞI (KARAKTERE BAK) ---
        Vector3 lookDirection = focusTarget.position - transform.position;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }
}