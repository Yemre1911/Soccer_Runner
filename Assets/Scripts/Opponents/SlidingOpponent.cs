using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class SlidingOpponent : BaseOpponent
{
    [Header("Sliding Settings")]
    [SerializeField] private float triggerDistance = 10f;
    [SerializeField] private float curveStrength = 0.65f;

    private CapsuleCollider col;
    private bool isSliding = false;

    // Pooling için kapsülün orijinal değerlerini tutuyoruz
    private float originalHeight;
    private float originalCenterY;
    private Quaternion originalRotation;

    private void Awake()
    {
        col = GetComponent<CapsuleCollider>();
        originalHeight = col.height;
        originalCenterY = col.center.y;
        originalRotation = transform.rotation;
    }

    public override void Initialize(Transform playerTransform)
    {
        base.Initialize(playerTransform);
        
        // Pool'dan her çıktığında kayma durumunu ve kapsülü sıfırla
        ResetSlideState();
    }

    protected override void Move()
    {
        float currentSpeed = GetCurrentSpeed();
        
        // Dikkat: Space.World kullandığımız için kapsül yatsa bile yerin dibine girmez, dünya ekseninde Z'de gelir.
        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime, Space.World);

        float distanceZ = transform.position.z - targetPlayer.position.z;

        // X ekseninde oyuncuya kay (Chasing mantığı)
        if (distanceZ <= triggerDistance && distanceZ > 0f)
        {
            float newX = Mathf.Lerp(transform.position.x, targetPlayer.position.x, curveStrength * Time.deltaTime);
            Vector3 pos = transform.position;
            pos.x = newX;
            transform.position = pos;

            // Henüz kaymıyorsa kayma aksiyonunu tetikle
            if (!isSliding)
            {
                PerformSlide();
            }
        }
    }

    private void PerformSlide()
    {
        isSliding = true;

        // 1. Hitbox'ı zıplamaya uygun hale getir (Boyu ve merkezi yarıya indir)
        col.height = originalHeight / 2f;
        col.center = new Vector3(col.center.x, originalCenterY / 2f, col.center.z);
        
        // 2. Görsel geribildirim için 90 derece yatır (Animatör gelince bu satırı sileceğiz)
        transform.rotation = Quaternion.Euler(90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    private void ResetSlideState()
    {
        isSliding = false;
        
        if (col != null)
        {
            col.height = originalHeight;
            col.center = new Vector3(col.center.x, originalCenterY, col.center.z);
        }
        
        transform.rotation = originalRotation;
    }
}