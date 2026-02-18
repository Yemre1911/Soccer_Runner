using UnityEngine;
using System.Collections;

public class SimpleCameraShake : MonoBehaviour
{
    // Titreme bittiğinde kamera buraya dönecek
    private Vector3 _originalLocalPos;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        _originalLocalPos = transform.localPosition;
    }

    public void TriggerShake(float duration = 0.15f, float magnitude = 0.2f)
    {
        // Eğer zaten titriyorsa durdur, yenisini başlat (Overlap olmasın)
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        
        _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Rastgele bir nokta seç (x ve y ekseninde)
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Kamerayı oynat
            transform.localPosition = _originalLocalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Süre bitince orijinal yerine dön
        transform.localPosition = _originalLocalPos;
    }
}