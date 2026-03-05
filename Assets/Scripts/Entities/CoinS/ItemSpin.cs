using UnityEngine;

public class ItemSpin : MonoBehaviour
{
    [Header("Spin")]
    [SerializeField] private float degreesPerSecond = 90f;

    [Tooltip("Dönüş ekseni (genelde Vector3.up)")]
    [SerializeField] private Vector3 axis = default;

    private float _startAngle;
    private bool _hasExternalReset;

    private void Awake()
    {
        if (axis == default) axis = Vector3.up;
    }

    private void OnEnable()
    {
        float angle = _hasExternalReset ? _startAngle : 0f;
        transform.localRotation = Quaternion.AngleAxis(angle, axis);
        _hasExternalReset = false;
    }

    public void ResetSpin(float startAngle)
    {
        _startAngle = startAngle;
        _hasExternalReset = true;
        transform.localRotation = Quaternion.AngleAxis(_startAngle, axis);
    }

    private void Update()
    {
        float delta = degreesPerSecond * Time.deltaTime;
        transform.Rotate(axis, delta, Space.Self);
    }
}