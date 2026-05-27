using System.Collections;
using UnityEngine;

public class GoalCameraSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float defaultDuration = 1.1f;

    [Tooltip("Yatay sallanma miktarı.")]
    [SerializeField] private float xAmount = 0.08f;

    [Tooltip("Dikey sallanma miktarı.")]
    [SerializeField] private float yAmount = 0.04f;

    [Tooltip("Sallantı hızı. Düşük değer daha sinematik olur.")]
    [SerializeField] private float frequency = 1.8f;

    [Tooltip("Sallantı başta ve sonda yumuşasın.")]
    [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private Coroutine _swayRoutine;

    private Vector3 _runtimeBaseLocalPosition;
    private Vector3 _lastAppliedOffset;

    public void Play()
    {
        Play(defaultDuration);
    }

    public void Play(float duration)
    {
        Stop();

        // KRİTİK NOKTA:
        // Kamera sway başladığı anda neredeyse, orayı merkez kabul ediyoruz.
        _runtimeBaseLocalPosition = transform.localPosition;
        _lastAppliedOffset = Vector3.zero;

        _swayRoutine = StartCoroutine(SwayRoutine(duration));
    }

    public void Stop()
    {
        if (_swayRoutine != null)
        {
            StopCoroutine(_swayRoutine);
            _swayRoutine = null;
        }

        // Sadece bizim en son verdiğimiz offset'i geri al.
        // Böylece kamera başka bir script tarafından taşındıysa eski saçma pozisyona dönmez.
        transform.localPosition -= _lastAppliedOffset;
        _lastAppliedOffset = Vector3.zero;
    }

    private IEnumerator SwayRoutine(float duration)
    {
        float safeDuration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;

        if (debugLogs)
            Debug.Log("[GoalCameraSway] Sway started.");

        while (elapsed < safeDuration)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += deltaTime;

            float t01 = Mathf.Clamp01(elapsed / safeDuration);

            float fadeInOut = Mathf.Sin(t01 * Mathf.PI);
            float curveValue = intensityCurve != null ? intensityCurve.Evaluate(t01) : 1f;
            float intensity = fadeInOut * curveValue;

            float time = useUnscaledTime ? Time.unscaledTime : Time.time;

            float x = Mathf.Sin(time * frequency * Mathf.PI * 2f) * xAmount;
            float y = Mathf.Cos(time * frequency * Mathf.PI * 2f * 0.85f) * yAmount;

            Vector3 newOffset = new Vector3(x, y, 0f) * intensity;

            // Önce bir önceki frame'de bizim verdiğimiz offset'i temizle.
            transform.localPosition -= _lastAppliedOffset;

            // Sonra yeni offset'i mevcut pozisyonun üstüne ekle.
            transform.localPosition += newOffset;

            _lastAppliedOffset = newOffset;

            yield return null;
        }

        transform.localPosition -= _lastAppliedOffset;
        _lastAppliedOffset = Vector3.zero;
        _swayRoutine = null;

        if (debugLogs)
            Debug.Log("[GoalCameraSway] Sway finished.");
    }

    private void OnDisable()
    {
        Stop();
    }
}