using UnityEngine;
using UnityEngine.UI;

public class ShotSwipeAreaCircle : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform circleRoot;
    [SerializeField] private Image circleImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Camera")]
    [SerializeField] private Camera uiWorldCamera;

    [Header("Visual")]
    [SerializeField] private float fadeInDuration = 0.5f;

    [Tooltip("119 / 255 = 0.466. UI görselinin ulaşacağı maksimum alpha değeri.")]
    [SerializeField] private float visibleAlpha = 119f / 255f;

    [Tooltip("Sprite'ın görsel yönünü kalibre etmek için. Görsel 90 derece fazla dönüyorsa genelde -90 iyi başlar.")]
    [SerializeField] private float visualRotationOffsetDegrees = -90f;

    [Header("Input Gate")]
    [Tooltip("Swipe başlangıcının kabul edileceği dairesel alan yarıçapı. Pixel cinsinden.")]
    [SerializeField] private float radiusPixels = 260f;

    [Tooltip("Açık izinli alanın açı genişliği. Senin sisteminde 90 derece.")]
    [SerializeField] private float openAngleDegrees = 90f;

    [Tooltip("Top merkezine çok yakın basışları engellemek için küçük güvenlik mesafesi.")]
    [SerializeField] private float minDistanceFromCenterPixels = 20f;

    [Header("Debug")]
    [SerializeField] private bool logRejectedStarts = false;
    [SerializeField] private bool logTracking = false;

    public bool IsVisible { get; private set; }
    public Vector2 CenterScreenPosition { get; private set; }
    public float OpenDirectionDegrees { get; private set; }
    public float RadiusPixels => radiusPixels;

    private Transform _ball;
    private Transform _goal;

    private float _fadeTimer;
    private bool _fadingIn;

    private RectTransform _imageRect;

    private Vector2 _imageBaseAnchorMin;
    private Vector2 _imageBaseAnchorMax;
    private Vector2 _imageBasePivot;
    private Vector2 _imageBaseAnchoredPosition;
    private Vector2 _imageBaseSizeDelta;
    private Vector3 _imageBaseScale;
    private Quaternion _imageBaseRotation;

    private void Awake()
    {
        CacheReferences();
        CacheImageInspectorTransform();
        HideImmediate();
    }

    private void LateUpdate()
    {
        if (!IsVisible)
            return;

        RefreshPositionAndRotation();
        UpdateFade();
    }

    public void Show(Transform ball, Transform goal, Camera cameraOverride = null)
    {
        _ball = ball;
        _goal = goal;

        if (cameraOverride != null)
            uiWorldCamera = cameraOverride;
        else if (uiWorldCamera == null)
            uiWorldCamera = Camera.main;

        if (_ball == null || _goal == null || uiWorldCamera == null)
        {
            Debug.LogWarning(
                $"[ShotSwipeAreaCircle] Show failed. " +
                $"Ball={(_ball ? _ball.name : "NULL")} | " +
                $"Goal={(_goal ? _goal.name : "NULL")} | " +
                $"Camera={(uiWorldCamera ? uiWorldCamera.name : "NULL")}"
            );

            HideImmediate();
            return;
        }

        IsVisible = true;
        _fadeTimer = 0f;
        _fadingIn = true;

        if (circleRoot != null)
        {
            circleRoot.gameObject.SetActive(true);
            circleRoot.localScale = Vector3.one;
        }

        if (circleImage != null)
        {
            circleImage.gameObject.SetActive(true);
            RestoreImageInspectorTransform();
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        RefreshPositionAndRotation();

        if (logTracking)
        {
            Debug.Log(
                $"[ShotSwipeAreaCircle] Show | " +
                $"Ball={_ball.name} | Goal={_goal.name} | Camera={uiWorldCamera.name}"
            );
        }
    }

    public void HideImmediate()
    {
        IsVisible = false;
        _fadingIn = false;

        _ball = null;
        _goal = null;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (circleImage != null)
            circleImage.gameObject.SetActive(false);
    }

    public bool IsScreenPointAllowed(Vector2 screenPoint)
    {
        if (!IsVisible)
            return true;

        Vector2 fromCenter = screenPoint - CenterScreenPosition;
        float distance = fromCenter.magnitude;

        if (distance < minDistanceFromCenterPixels)
        {
            LogRejectedStart("Too close to center", distance, 0f, screenPoint);
            return false;
        }

        if (distance > radiusPixels)
        {
            LogRejectedStart("Outside radius", distance, 0f, screenPoint);
            return false;
        }

        float pointAngle = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg;
        float deltaAngle = Mathf.DeltaAngle(OpenDirectionDegrees, pointAngle);
        float halfOpenAngle = openAngleDegrees * 0.5f;

        bool allowed = Mathf.Abs(deltaAngle) <= halfOpenAngle;

        if (!allowed)
            LogRejectedStart("Outside open angle", distance, deltaAngle, screenPoint);

        return allowed;
    }

    private void CacheReferences()
    {
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (circleRoot == null)
            circleRoot = transform as RectTransform;

        if (circleImage == null)
            circleImage = GetComponentInChildren<Image>(true);

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (uiWorldCamera == null)
            uiWorldCamera = Camera.main;
    }

    private void CacheImageInspectorTransform()
    {
        if (circleImage == null)
            return;

        _imageRect = circleImage.rectTransform;

        _imageBaseAnchorMin = _imageRect.anchorMin;
        _imageBaseAnchorMax = _imageRect.anchorMax;
        _imageBasePivot = _imageRect.pivot;
        _imageBaseAnchoredPosition = _imageRect.anchoredPosition;
        _imageBaseSizeDelta = _imageRect.sizeDelta;
        _imageBaseScale = _imageRect.localScale;
        _imageBaseRotation = _imageRect.localRotation;
    }

    private void RestoreImageInspectorTransform()
    {
        if (_imageRect == null)
            return;

        _imageRect.anchorMin = _imageBaseAnchorMin;
        _imageRect.anchorMax = _imageBaseAnchorMax;
        _imageRect.pivot = _imageBasePivot;
        _imageRect.anchoredPosition = _imageBaseAnchoredPosition;
        _imageRect.sizeDelta = _imageBaseSizeDelta;
        _imageRect.localScale = _imageBaseScale;
        _imageRect.localRotation = _imageBaseRotation;
    }

    private void RefreshPositionAndRotation()
    {
        if (_ball == null || _goal == null || uiWorldCamera == null || canvas == null || circleRoot == null)
            return;

        Vector3 ballScreen3 = uiWorldCamera.WorldToScreenPoint(_ball.position);
        Vector3 goalScreen3 = uiWorldCamera.WorldToScreenPoint(_goal.position);

        CenterScreenPosition = new Vector2(ballScreen3.x, ballScreen3.y);

        Vector2 goalScreen = new Vector2(goalScreen3.x, goalScreen3.y);
        Vector2 toGoal = goalScreen - CenterScreenPosition;

        if (toGoal.sqrMagnitude > 0.001f)
            OpenDirectionDegrees = Mathf.Atan2(toGoal.y, toGoal.x) * Mathf.Rad2Deg;

        RectTransform canvasRect = canvas.transform as RectTransform;

        Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            CenterScreenPosition,
            canvasCamera,
            out Vector2 localPoint
        );

        circleRoot.anchoredPosition = localPoint;

        circleRoot.localRotation = Quaternion.Euler(
            0f,
            0f,
            OpenDirectionDegrees + visualRotationOffsetDegrees
        );

        if (logTracking)
        {
            Debug.Log(
                $"[ShotSwipeAreaCircle] Tracking | " +
                $"CenterScreen={CenterScreenPosition} | " +
                $"GoalScreen={goalScreen} | " +
                $"OpenDir={OpenDirectionDegrees:F1} | " +
                $"FinalRot={OpenDirectionDegrees + visualRotationOffsetDegrees:F1}"
            );
        }
    }

    private void UpdateFade()
    {
        if (!_fadingIn || canvasGroup == null)
            return;

        if (fadeInDuration <= 0f)
        {
            canvasGroup.alpha = visibleAlpha;
            _fadingIn = false;
            return;
        }

        _fadeTimer += Time.unscaledDeltaTime;

        float t = Mathf.Clamp01(_fadeTimer / fadeInDuration);
        canvasGroup.alpha = Mathf.Lerp(0f, visibleAlpha, t);

        if (t >= 1f)
            _fadingIn = false;
    }

    private void LogRejectedStart(string reason, float distance, float deltaAngle, Vector2 screenPoint)
    {
        if (!logRejectedStarts)
            return;

        Debug.Log(
            $"[ShotSwipeAreaCircle] Swipe start rejected | " +
            $"Reason={reason} | " +
            $"ScreenPoint={screenPoint} | " +
            $"Center={CenterScreenPosition} | " +
            $"Distance={distance:F1} | " +
            $"Radius={radiusPixels:F1} | " +
            $"OpenDir={OpenDirectionDegrees:F1} | " +
            $"DeltaAngle={deltaAngle:F1}"
        );
    }
}