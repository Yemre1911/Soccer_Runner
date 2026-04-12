using UnityEngine;

public class FogDistanceController : MonoBehaviour
{
    [Header("Linear Fog Distance")]
    [SerializeField] private float defaultLinearEndDistance = 87f;

    private float _targetEndDistance;
    private bool _isAnimating;

    private void Awake()
    {
        _targetEndDistance = RenderSettings.fogEndDistance;
    }

    private void Update()
    {
        if (!_isAnimating) return;
        if (RenderSettings.fogMode != FogMode.Linear) return;

        RenderSettings.fogEndDistance = Mathf.MoveTowards(
            RenderSettings.fogEndDistance,
            _targetEndDistance,
            Time.unscaledDeltaTime * 80f
        );

        if (Mathf.Approximately(RenderSettings.fogEndDistance, _targetEndDistance))
            _isAnimating = false;
    }

    public void SetImmediate(float endDistance)
    {
        if (RenderSettings.fogMode != FogMode.Linear)
            return;

        _targetEndDistance = endDistance;
        _isAnimating = false;
        RenderSettings.fogEndDistance = endDistance;
    }

    public void SmoothTo(float endDistance, float speed)
    {
        if (RenderSettings.fogMode != FogMode.Linear)
            return;

        _targetEndDistance = endDistance;
        _isAnimating = true;
        _smoothSpeed = Mathf.Max(0.01f, speed);
    }

    private float _smoothSpeed = 80f;

    private void LateUpdate()
    {
        if (!_isAnimating) return;
        if (RenderSettings.fogMode != FogMode.Linear) return;

        RenderSettings.fogEndDistance = Mathf.MoveTowards(
            RenderSettings.fogEndDistance,
            _targetEndDistance,
            Time.unscaledDeltaTime * _smoothSpeed
        );

        if (Mathf.Approximately(RenderSettings.fogEndDistance, _targetEndDistance))
            _isAnimating = false;
    }

    public void ResetToDefault()
    {
        SetImmediate(defaultLinearEndDistance);
    }
}