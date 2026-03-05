using System.Collections;
using UnityEngine;

/// <summary>
/// Player trigger edince coin player'a doğru süzülür, scale down olur ve pool'a döner.
/// Ayrıca coin değerini CoinManager'a ekler.
/// </summary>
[RequireComponent(typeof(PooledItem))]
public class CoinAttractPickup : MonoBehaviour
{
    [Header("Value")]
    [SerializeField] private int value = 1;

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Collider coinTrigger;      // coin trigger collider
    [SerializeField] private Transform visualRoot;      // mesh root (opsiyonel)

    [Header("Attract")]
    [SerializeField] private float duration = 0.22f;
    [SerializeField] private float finishDistance = 0.25f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("İstersen hedefe biraz yukarı offsetle (player pivot ayağındaysa iyi olur).")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.0f, 0f);

    [Header("Scale Down")]
    [SerializeField] private float scaleDownDuration = 0.20f;
    [SerializeField] private float endScaleMultiplier = 0.0f;

    [Header("Optional")]
    [SerializeField] private bool disableSpinWhileAttract = true;

    private PooledItem _pooledItem;
    private bool _picked;
    private Coroutine _routine;
    private Vector3 _initialScale;

    private void Awake()
    {
        _pooledItem = GetComponent<PooledItem>();

        if (!coinTrigger) coinTrigger = GetComponentInChildren<Collider>();
        if (!visualRoot) visualRoot = transform;

        _initialScale = visualRoot.localScale;
    }

    private void OnEnable()
    {
        // pool reset
        _picked = false;

        if (coinTrigger) coinTrigger.enabled = true;
        if (visualRoot) visualRoot.localScale = _initialScale;

        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        if (disableSpinWhileAttract)
        {
            var spin = GetComponent<CoinSpin>();
            if (spin) spin.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_picked) return;

        Transform playerRoot = other.transform.root;
        if (!playerRoot.CompareTag(playerTag))
            return;

        StartPickup(playerRoot);
    }

    private void StartPickup(Transform target)
    {
        _picked = true;

        if (coinTrigger) coinTrigger.enabled = false;

        if (disableSpinWhileAttract)
        {
            var spin = GetComponent<CoinSpin>();
            if (spin) spin.enabled = false;
        }

        // ✅ coin değerini burada ekle (animasyonu beklemeden, double-add riskini _picked ile engelliyoruz)
        if (CoinManager.Instance != null && value > 0)
            CoinManager.Instance.Add(value);

        _routine = StartCoroutine(AttractRoutine(target));
    }

    private IEnumerator AttractRoutine(Transform target)
    {
        Transform t = visualRoot ? visualRoot : transform;

        Vector3 startPos = t.position;
        Vector3 startScale = t.localScale;
        Vector3 endScale = _initialScale * endScaleMultiplier;

        float moveInv = duration > 0.0001f ? 1f / duration : 9999f;
        float scaleInv = scaleDownDuration > 0.0001f ? 1f / scaleDownDuration : 9999f;

        float moveT = 0f;
        float scaleT = 0f;

        while (true)
        {
            if (!target)
            {
                _pooledItem.Despawn();
                yield break;
            }

            moveT += Time.deltaTime * moveInv;
            scaleT += Time.deltaTime * scaleInv;

            float em = ease.Evaluate(Mathf.Clamp01(moveT));
            float es = ease.Evaluate(Mathf.Clamp01(scaleT));

            Vector3 targetPos = target.position + targetOffset;

            t.position = Vector3.Lerp(startPos, targetPos, em);
            t.localScale = Vector3.Lerp(startScale, endScale, es);

            if (moveT >= 1f || Vector3.Distance(t.position, targetPos) <= finishDistance)
                break;

            yield return null;
        }

        _pooledItem.Despawn();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        value = Mathf.Max(0, value);
        duration = Mathf.Max(0f, duration);
        scaleDownDuration = Mathf.Max(0f, scaleDownDuration);
        finishDistance = Mathf.Max(0f, finishDistance);
    }
#endif
}