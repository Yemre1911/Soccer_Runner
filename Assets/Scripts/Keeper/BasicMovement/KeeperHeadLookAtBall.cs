using UnityEngine;

public class KeeperHeadLookAtBall : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform ball;

    [Header("Look Settings")]
    [SerializeField] private float rotateSpeed = 12f;
    [SerializeField] private float maxYaw = 70f;
    [SerializeField] private float maxPitch = 35f;

    [Header("Axis")]
    [Tooltip("Modelin ileri baktığı lokal yön. Genelde Vector3.forward.")]
    [SerializeField] private Vector3 localForwardAxis = Vector3.forward;

    private Quaternion _initialLocalRotation;
    private Transform _parent;

    private void Awake()
    {
        _initialLocalRotation = transform.localRotation;
        _parent = transform.parent;
    }

    private void LateUpdate()
    {
        if (!ball || !_parent)
            return;

        Vector3 worldDirection = ball.position - transform.position;

        if (worldDirection.sqrMagnitude < 0.0001f)
            return;

        Vector3 localDirection = _parent.InverseTransformDirection(worldDirection.normalized);

        float yaw = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
        float pitch = -Mathf.Asin(Mathf.Clamp(localDirection.y, -1f, 1f)) * Mathf.Rad2Deg;

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        Quaternion lookRotation = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion targetRotation = _initialLocalRotation * lookRotation;

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }

    public void SetBall(Transform newBall)
    {
        ball = newBall;
    }
}