using UnityEngine;
using System.Collections;

public class SimpleCameraShake : MonoBehaviour
{
    private Vector3 _originalLocalPos;
    private Coroutine _shakeCoroutine;

    [Header("Ambient")]
    [SerializeField] private float ambientX = 0.35f;   // TEST için yüksek
    [SerializeField] private float ambientY = 0.2f;   // TEST için yüksek
    [SerializeField] private float ambientFrequency = 10f;
    [SerializeField] private float ambientBlendSpeed = 20f;

    private bool _ambientActive;
    private float _ambientWeight;
    private Vector3 _impulseOffset;

    private void Awake()
    {
        _originalLocalPos = transform.localPosition;
    }

    private void LateUpdate()
    {
        float target = _ambientActive ? 1f : 0f;
        _ambientWeight = Mathf.MoveTowards(_ambientWeight, target, ambientBlendSpeed * Time.deltaTime);

        Vector3 ambientOffset = Vector3.zero;

        if (_ambientWeight > 0.001f)
        {
            float t = Time.time * ambientFrequency;

            float x = Mathf.Sin(t) * ambientX;
            float y = Mathf.Cos(t * 1.17f) * ambientY;

            ambientOffset = new Vector3(x, y, 0f) * _ambientWeight;
        }

        transform.localPosition = _originalLocalPos + ambientOffset + _impulseOffset;
    }

    public void TriggerShake(float duration = 0.15f, float magnitude = 0.2f)
    {
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            _impulseOffset = new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _impulseOffset = Vector3.zero;
        _shakeCoroutine = null;
    }

    public void SetAmbientActive(bool active)
    {
        _ambientActive = active;
        //Debug.Log($"[SimpleCameraShake] SetAmbientActive({active}) on {name}");
    }
}