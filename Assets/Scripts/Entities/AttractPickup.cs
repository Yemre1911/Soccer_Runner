/*using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PooledItem))]
public class AttractPickup : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private Transform visualRoot;

    [Header("Attract")]
    [SerializeField] private float duration = 0.22f;
    [SerializeField] private float finishDistance = 0.25f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.0f, 0f);

    [Header("Scale Down")]
    [SerializeField] private float scaleDownDuration = 0.20f;
    [SerializeField] private float endScaleMultiplier = 0.0f;

    [Header("Optional")]
    [SerializeField] private bool disableSpinWhileAttract = true;

    [Tooltip("Bu pickup toplanınca çalışacak effect component'leri (aynı objede bulunur).")]
    [SerializeField] private MonoBehaviour[] effects;

    private PooledItem _pooledItem;
    private bool _picked;
    private Coroutine _routine;
    private Vector3 _initialScale;

    private void Awake()
    {
        _pooledItem = GetComponent<PooledItem>();

        if (!triggerCollider) triggerCollider = GetComponentInChildren<Collider>();
        if (!visualRoot) visualRoot = transform;

        _initialScale = visualRoot.localScale;

        // güvenlik: trigger
        if (triggerCollider) triggerCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        _picked = false;

        if (triggerCollider) triggerCollider.enabled = true;
        if (visualRoot) visualRoot.localScale = _initialScale;

        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        if (disableSpinWhileAttract)
        {
            var spin = GetComponent<ItemSpin>();
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

    private void StartPickup(Transform picker)
    {
        _picked = true;

        if (triggerCollider) triggerCollider.enabled = false;

        if (disableSpinWhileAttract)
        {
            var spin = GetComponent<ItemSpin>();
            if (spin) spin.enabled = false;
        }

        // ✅ EFFECT'LERİ HEMEN UYGULA (coin gibi, animasyonu beklemez)
        ApplyEffects(picker);

        _routine = StartCoroutine(AttractRoutine(picker));
    }

    private void ApplyEffects(Transform picker)
    {
        var ctx = new PickupContext(picker, gameObject);

        if (effects != null && effects.Length > 0)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i] is IPickupEffect fx)
                    fx.Apply(ctx);
            }
        }
        else
        {
            // effect atanmadıysa debug (sen istedin)
            Debug.Log($"[Pickup] Collected: {name}", this);
        }
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
        duration = Mathf.Max(0f, duration);
        scaleDownDuration = Mathf.Max(0f, scaleDownDuration);
        finishDistance = Mathf.Max(0f, finishDistance);
    }
#endif
}*/

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PooledItem))]
public class AttractPickup : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Collider triggerCollider;
    [SerializeField] private Transform visualRoot;

    [Header("Attract")]
    [SerializeField] private float duration = 0.22f;
    [SerializeField] private float finishDistance = 0.25f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.0f, 0f);

    [Header("Scale Down")]
    [SerializeField] private float scaleDownDuration = 0.20f;
    [SerializeField] private float endScaleMultiplier = 0.0f;

    [Header("Optional")]
    [SerializeField] private bool disableSpinWhileAttract = true;

    [Header("Magnet")]
    [Tooltip("Sadece coin gibi magnet tarafından çekilmesini istediğin pickup'larda aç.")]
    [SerializeField] private bool allowMagnetCollect = false;

    [Tooltip("Bu pickup toplanınca çalışacak effect component'leri (aynı objede bulunur).")]
    [SerializeField] private MonoBehaviour[] effects;

    private PooledItem _pooledItem;
    private bool _picked;
    private Coroutine _routine;
    private Vector3 _initialScale;

    public bool IsPicked => _picked;
    public bool AllowMagnetCollect => allowMagnetCollect;
    public Transform VisualTransform => visualRoot ? visualRoot : transform;

    private void Awake()
    {
        _pooledItem = GetComponent<PooledItem>();

        if (!triggerCollider) triggerCollider = GetComponentInChildren<Collider>();
        if (!visualRoot) visualRoot = transform;

        _initialScale = visualRoot.localScale;

        if (triggerCollider) triggerCollider.isTrigger = true;
    }

    private void OnEnable()
    {
        _picked = false;

        if (triggerCollider) triggerCollider.enabled = true;
        if (visualRoot) visualRoot.localScale = _initialScale;

        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        if (disableSpinWhileAttract)
        {
            var spin = GetComponent<ItemSpin>();
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

    public bool ForcePickup(Transform picker)
    {
        if (_picked) return false;
        if (picker == null) return false;

        Transform playerRoot = picker.root;
        if (!playerRoot.CompareTag(playerTag))
            return false;

        StartPickup(playerRoot);
        return true;
    }

    private void StartPickup(Transform picker)
    {
        _picked = true;

        if (triggerCollider) triggerCollider.enabled = false;

        if (disableSpinWhileAttract)
        {
            var spin = GetComponent<ItemSpin>();
            if (spin) spin.enabled = false;
        }

        ApplyEffects(picker);
        _routine = StartCoroutine(AttractRoutine(picker));
    }

    private void ApplyEffects(Transform picker)
    {
        var ctx = new PickupContext(picker, gameObject);

        if (effects != null && effects.Length > 0)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i] is IPickupEffect fx)
                    fx.Apply(ctx);
            }
        }
        else
        {
            Debug.Log($"[Pickup] Collected: {name}", this);
        }
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
        duration = Mathf.Max(0f, duration);
        scaleDownDuration = Mathf.Max(0f, scaleDownDuration);
        finishDistance = Mathf.Max(0f, finishDistance);
    }
#endif
}