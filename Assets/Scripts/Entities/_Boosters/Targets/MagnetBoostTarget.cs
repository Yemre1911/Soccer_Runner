/*using UnityEngine;

public class MagnetBoostTarget : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private float baseRadius = 0f;

    [Header("Runtime")]
    [SerializeField] private float currentRadius = 0f;

    public float BaseRadius => baseRadius;
    public float CurrentRadius => currentRadius;

    private void Awake()
    {
        currentRadius = baseRadius;
    }

    public void SetBoostedRadius(float radius)
    {
        currentRadius = Mathf.Max(0f, radius);
        Debug.Log($"[MagnetBoostTarget] Radius set -> {currentRadius}", this);
    }

    public void ResetRadius()
    {
        currentRadius = baseRadius;
        Debug.Log($"[MagnetBoostTarget] Radius reset -> {currentRadius}", this);
    }
}*/

using UnityEngine;

public class MagnetBoostTarget : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private float baseRadius = 0f;

    [Header("Runtime")]
    [SerializeField] private float currentRadius = 0f;

    [Header("Refs")]
    [SerializeField] private Transform pickerRoot;
    [SerializeField] private MagnetVisualTarget visualTarget;

    [Header("Scan")]
    [SerializeField] private bool onlyInFront = true;
    [SerializeField] private float frontTolerance = -0.25f;

    private AttractPickup _currentTarget;
    private bool _active;

    public float BaseRadius => baseRadius;
    public float CurrentRadius => currentRadius;
    public Transform CurrentTarget => _currentTarget ? _currentTarget.VisualTransform : null;

    private void Awake()
    {
        currentRadius = baseRadius;

        if (!pickerRoot)
            pickerRoot = transform.root;
    }

    private void Update()
    {
        if (!_active || currentRadius <= 0.01f || pickerRoot == null)
        {
            _currentTarget = null;
            return;
        }

        _currentTarget = FindNearestEligiblePickup();

        if (_currentTarget != null)
        {
            bool collected = _currentTarget.ForcePickup(pickerRoot);
            if (collected && visualTarget != null)
                visualTarget.PulseRecoil();
        }
    }

    public void SetBoostedRadius(float radius)
    {
        currentRadius = Mathf.Max(0f, radius);
        _active = true;
        Debug.Log($"[MagnetBoostTarget] Radius set -> {currentRadius}", this);
    }

    public void ResetRadius()
    {
        currentRadius = baseRadius;
        _active = false;
        _currentTarget = null;
        Debug.Log($"[MagnetBoostTarget] Radius reset -> {currentRadius}", this);
    }

    private AttractPickup FindNearestEligiblePickup()
    {
        AttractPickup[] all = FindObjectsOfType<AttractPickup>(false);

        AttractPickup best = null;
        float bestDist = float.MaxValue;
        Vector3 origin = pickerRoot.position;

        for (int i = 0; i < all.Length; i++)
        {
            AttractPickup pickup = all[i];
            if (pickup == null) continue;
            if (!pickup.gameObject.activeInHierarchy) continue;
            if (pickup.IsPicked) continue;
            if (!pickup.AllowMagnetCollect) continue;

            Vector3 targetPos = pickup.VisualTransform.position;
            Vector3 delta = targetPos - origin;

            if (delta.sqrMagnitude > currentRadius * currentRadius)
                continue;

            if (onlyInFront)
            {
                Vector3 local = pickerRoot.InverseTransformPoint(targetPos);
                if (local.z < frontTolerance)
                    continue;
            }

            float sqrDist = delta.sqrMagnitude;
            if (sqrDist < bestDist)
            {
                bestDist = sqrDist;
                best = pickup;
            }
        }

        return best;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform center = pickerRoot ? pickerRoot : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center.position, currentRadius > 0f ? currentRadius : baseRadius);
    }
#endif
}