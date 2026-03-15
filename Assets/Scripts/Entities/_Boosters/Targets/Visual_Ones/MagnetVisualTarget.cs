/*using UnityEngine;

public class MagnetVisualTarget : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject visualRoot;

    [Header("Look At")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float turnSpeed = 8f;

    [Header("Optional nearest coin tracking")]
    [SerializeField] private bool autoTrackNearestCoin = false;
    [SerializeField] private string coinTag = "Coin";
    [SerializeField] private float searchRadius = 20f;

    [Header("Recoil")]
    [SerializeField] private bool enableRecoil = false;
    [SerializeField] private float recoilDistance = 0.08f;
    [SerializeField] private float recoilReturnSpeed = 10f;

    private Vector3 _initialLocalPos;
    private Vector3 _recoilOffset;
    private bool _isActive;

    private void Awake()
    {
        if (!pivot) pivot = transform;
        if (visualRoot) _initialLocalPos = visualRoot.transform.localPosition;
        SetActiveVisual(false);
    }

    private void Update()
    {
        if (!_isActive) return;

        Transform target = autoTrackNearestCoin ? FindNearestCoin() : null;
        if (target != null)
        {
            Vector3 dir = target.position - pivot.position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                pivot.rotation = Quaternion.Slerp(pivot.rotation, targetRot, Time.deltaTime * turnSpeed);
            }
        }

        if (enableRecoil && visualRoot)
        {
            _recoilOffset = Vector3.Lerp(_recoilOffset, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
            visualRoot.transform.localPosition = _initialLocalPos + _recoilOffset;
        }
    }

    public void SetActiveVisual(bool active)
    {
        _isActive = active;
        if (visualRoot) visualRoot.SetActive(active);

        if (!active && visualRoot)
        {
            _recoilOffset = Vector3.zero;
            visualRoot.transform.localPosition = _initialLocalPos;
        }
    }

    public void PulseRecoil()
    {
        if (!_isActive || !enableRecoil || !visualRoot) return;
        _recoilOffset += Vector3.back * recoilDistance;
    }

    private Transform FindNearestCoin()
    {
        Collider[] hits = Physics.OverlapSphere(pivot.position, searchRadius);
        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i];
            if (!c || !c.CompareTag(coinTag)) continue;

            float d = (c.transform.position - pivot.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = c.transform;
            }
        }

        return best;
    }
}*/

using UnityEngine;

public class MagnetVisualTarget : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private Transform pivot;
    [SerializeField] private MagnetBoostTarget magnetTarget;

    [Header("Look At")]
    [SerializeField] private float turnSpeed = 8f;

    [Header("Recoil")]
    [SerializeField] private bool enableRecoil = false;
    [SerializeField] private float recoilDistance = 0.08f;
    [SerializeField] private float recoilReturnSpeed = 10f;

    private Vector3 _initialLocalPos;
    private Vector3 _recoilOffset;
    private bool _isActive;

    private void Awake()
    {
        if (!pivot) pivot = transform;

        if (!magnetTarget)
            magnetTarget = GetComponentInParent<MagnetBoostTarget>();

        if (visualRoot)
            _initialLocalPos = visualRoot.transform.localPosition;

        SetActiveVisual(false);
    }

    private void Update()
    {
        if (!_isActive) return;

        Transform target = magnetTarget ? magnetTarget.CurrentTarget : null;
        if (target != null)
        {
            Vector3 dir = target.position - pivot.position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                pivot.rotation = Quaternion.Slerp(pivot.rotation, targetRot, Time.deltaTime * turnSpeed);
            }
        }

        if (enableRecoil && visualRoot)
        {
            _recoilOffset = Vector3.Lerp(_recoilOffset, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
            visualRoot.transform.localPosition = _initialLocalPos + _recoilOffset;
        }
    }

    public void SetActiveVisual(bool active)
    {
        _isActive = active;

        if (visualRoot)
            visualRoot.SetActive(active);

        if (!active && visualRoot)
        {
            _recoilOffset = Vector3.zero;
            visualRoot.transform.localPosition = _initialLocalPos;
        }
    }

    public void PulseRecoil()
    {
        if (!_isActive || !enableRecoil || !visualRoot) return;
        _recoilOffset += Vector3.back * recoilDistance;
    }
}