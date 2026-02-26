using UnityEngine;

public class ChasingOpponent : BaseOpponent
{
    [Header("Curve (X'e kayma)")]
    [SerializeField] private float curveTriggerDistance = 20f; // ESKİ triggerDistance = bunu kontrol ediyor
    [SerializeField] private float curveStrength = 0.65f;

    [Header("Attack (Aksiyon tetikleme)")]
    [SerializeField] private float attackDistanceMin = 5f;
    [SerializeField] private float attackDistanceMax = 7f;

    private bool isAttacking;
    private float _attackDistance; // spawn başına seçilen gerçek değer

    public override void Initialize(Transform playerTransform)
    {
        base.Initialize(playerTransform);

        isAttacking = false;

        // Her spawn'da 3-5 arası ufak varyasyon
        _attackDistance = Random.Range(attackDistanceMin, attackDistanceMax);
    }

    protected override void Move()
    {
        float currentSpeed = GetCurrentSpeed();
        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime, Space.World);

        float distanceZ = transform.position.z - targetPlayer.position.z;

        // 1) Curve: belirli mesafeden sonra X'e kaymaya başla
        if (distanceZ <= curveTriggerDistance && distanceZ > 0f)
        {
            float newX = Mathf.Lerp(transform.position.x, targetPlayer.position.x, curveStrength * Time.deltaTime);
            Vector3 pos = transform.position;
            pos.x = newX;
            transform.position = pos;
        }

        // 2) Attack: çok daha yakınken tetikle (3-5)
        if (!isAttacking && distanceZ <= _attackDistance && distanceZ > 0f)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        isAttacking = true;
        TriggerAttack(); // BaseOpponent'taki DoAttack trigger
    }
}
