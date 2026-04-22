using UnityEngine;

public class WorldCurveController : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private Transform _playerTransform;

    [Header("Curve Ayarları")]
    public float _maxSideCurveAmount = 0.0005f;
    [SerializeField] private float _maxPlayerX = 3f;

    [Header("Backward Curve")]
    [SerializeField] private float _defaultBackwardCurve = -0.002f;

    private int _sideCurvePropertyID;
    private int _backwardCurvePropertyID;

    private void Start()
    {
        _sideCurvePropertyID = Shader.PropertyToID("_SideWayStrenght");
        _backwardCurvePropertyID = Shader.PropertyToID("_BackWardsStrenght");

        // Başlangıçta backward curve'ü garanti set et
        Shader.SetGlobalFloat(_backwardCurvePropertyID, _defaultBackwardCurve);
    }

    private void LateUpdate()
    {
        if (_playerTransform == null) return;

        float ratio = -_playerTransform.position.x / _maxPlayerX;
        float currentSideCurve = ratio * _maxSideCurveAmount;

        Shader.SetGlobalFloat(_sideCurvePropertyID, currentSideCurve);
    }
}