using UnityEngine;

public class AnimatedGloveSocketFollower : MonoBehaviour
{
    public enum HandMode
    {
        AutoByBallSide,
        ForceLeft,
        ForceRight,
        Locked
    }

    [Header("Hand References")]
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    [Header("Side Detection")]
    [Tooltip("Genelde keeper root/capsule. Ball bunun local X'ine göre sağ/sol kabul edilir.")]
    [SerializeField] private Transform keeperRoot;

    [Tooltip("Top referansı. Boşsa bu socket'in mevcut pozisyonuna göre karar verir.")]
    [SerializeField] private Transform ballReference;

    [Tooltip("Eğer riginde local +X sağ taraf değilse bunu kapat.")]
    [SerializeField] private bool positiveXMeansRightHand = true;

    [Header("Mode")]
    [SerializeField] private HandMode handMode = HandMode.AutoByBallSide;

    [Header("Offsets")]
    [SerializeField] private Vector3 leftHandLocalOffset;
    [SerializeField] private Vector3 rightHandLocalOffset;

    [Header("Rotation")]
    [SerializeField] private bool copyHandRotation = true;
    [SerializeField] private Vector3 leftHandLocalEulerOffset;
    [SerializeField] private Vector3 rightHandLocalEulerOffset;

    [Header("Debug")]
    [SerializeField] private bool drawDebug;

    private Transform _currentHand;
    private Vector3 _currentOffset;
    private Vector3 _currentEulerOffset;

    private bool _lockedToRightHand;

    public Transform CurrentHand => _currentHand;
    public bool IsLocked => handMode == HandMode.Locked;

    private void LateUpdate()
    {
        ResolveCurrentHand();

        if (_currentHand == null)
            return;

        transform.position = _currentHand.TransformPoint(_currentOffset);

        if (copyHandRotation)
        {
            Quaternion offsetRotation = Quaternion.Euler(_currentEulerOffset);
            transform.rotation = _currentHand.rotation * offsetRotation;
        }
    }

    public void SetBallReference(Transform ball)
    {
        ballReference = ball;
    }

    public void SetHandMode(HandMode mode)
    {
        handMode = mode;
    }

    public void ForceLeftHand()
    {
        handMode = HandMode.ForceLeft;
    }

    public void ForceRightHand()
    {
        handMode = HandMode.ForceRight;
    }

    public void UseAutoByBallSide()
    {
        handMode = HandMode.AutoByBallSide;
    }

    public void LockHandByBallSide()
    {
        _lockedToRightHand = ShouldUseRightHandByBallSide();
        handMode = HandMode.Locked;

        if (_lockedToRightHand)
            UseRightHand();
        else
            UseLeftHand();
    }

    public void LockLeftHand()
    {
        _lockedToRightHand = false;
        handMode = HandMode.Locked;
        UseLeftHand();
    }

    public void LockRightHand()
    {
        _lockedToRightHand = true;
        handMode = HandMode.Locked;
        UseRightHand();
    }

    public void Unlock()
    {
        handMode = HandMode.AutoByBallSide;
    }

    private void ResolveCurrentHand()
    {
        switch (handMode)
        {
            case HandMode.ForceLeft:
                UseLeftHand();
                return;

            case HandMode.ForceRight:
                UseRightHand();
                return;

            case HandMode.Locked:
                if (_lockedToRightHand)
                    UseRightHand();
                else
                    UseLeftHand();
                return;

            case HandMode.AutoByBallSide:
            default:
                ResolveByBallSide();
                return;
        }
    }

    private void ResolveByBallSide()
    {
        if (ShouldUseRightHandByBallSide())
            UseRightHand();
        else
            UseLeftHand();
    }

    private bool ShouldUseRightHandByBallSide()
    {
        if (leftHand == null && rightHand == null)
            return false;

        if (leftHand == null)
            return true;

        if (rightHand == null)
            return false;

        Transform sideRoot = keeperRoot != null ? keeperRoot : transform.root;
        Transform reference = ballReference != null ? ballReference : transform;

        float localX = sideRoot.InverseTransformPoint(reference.position).x;

        bool shouldUseRight = positiveXMeansRightHand
            ? localX >= 0f
            : localX < 0f;

        return shouldUseRight;
    }

    private void UseLeftHand()
    {
        _currentHand = leftHand;
        _currentOffset = leftHandLocalOffset;
        _currentEulerOffset = leftHandLocalEulerOffset;
    }

    private void UseRightHand()
    {
        _currentHand = rightHand;
        _currentOffset = rightHandLocalOffset;
        _currentEulerOffset = rightHandLocalEulerOffset;
    }

    private void OnDrawGizmos()
    {
        if (!drawDebug)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.08f);

        if (_currentHand != null)
        {
            Gizmos.DrawLine(_currentHand.position, transform.position);
        }
    }
}