using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class SlidingOpponent : BaseOpponent
{
    [Header("Sliding Settings")]
    [SerializeField] private float triggerDistance = 10f;
    [SerializeField] private float curveStrength = 0.65f;

    private CapsuleCollider col;
    private bool isSliding = false;

    // Pooling için kapsülün orijinal değerleri
    private float originalHeight;
    private Vector3 originalCenter;

    private Quaternion originalRotation;

    protected override void Awake()
    {
        base.Awake();

        col = GetComponent<CapsuleCollider>();
        originalHeight = col.height;
        originalCenter = col.center;
        originalRotation = transform.rotation;
    }

    public override void Initialize(Transform playerTransform)
    {
        base.Initialize(playerTransform);
        ResetSlideState();
    }

    protected override void Move()
    {
        float currentSpeed = GetCurrentSpeed();
        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime, Space.World);

        float distanceZ = transform.position.z - targetPlayer.position.z;

        if (distanceZ <= triggerDistance && distanceZ > 0f)
        {
            // X'e kay
            float newX = Mathf.Lerp(transform.position.x, targetPlayer.position.x, curveStrength * Time.deltaTime);
            Vector3 pos = transform.position;
            pos.x = newX;
            transform.position = pos;

            if (!isSliding)
            {
                PerformSlide();
            }
        }
    }

    private void PerformSlide()
    {
        isSliding = true;

        // Hitbox ayarı
        col.height = originalHeight / 2f;
        col.center = new Vector3(originalCenter.x, originalCenter.y / 2f, originalCenter.z);

        // Yeni sistem: DoSlide trigger
        TriggerSlide();
    }

    private void ResetSlideState()
    {
        isSliding = false;

        if (col != null)
        {
            col.height = originalHeight;
            col.center = originalCenter;
        }

        transform.rotation = originalRotation;
    }

    public override void OnDespawned()
    {
        base.OnDespawned();
        ResetSlideState();
    }
}
