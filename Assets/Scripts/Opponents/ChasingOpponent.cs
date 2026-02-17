using UnityEngine;

public class ChasingOpponent : BaseOpponent
{
    [Header("Behavior Settings")]
    [SerializeField] private float triggerDistance = 20f;   // Ne kadar yaklaşınca X'e kıracak?
    [SerializeField] private float curveStrength = 0.65f;   // Kavis yumuşaklığı

    protected override void Move()
    {
        float currentSpeed = GetCurrentSpeed();
        // Z ekseninde sabit hızla "geri" gelir (senin sistemine göre doğru)
        //transform.Translate(Vector3.back * speed * Time.deltaTime, Space.World);
        transform.Translate(Vector3.back * currentSpeed * Time.deltaTime, Space.World);// yeni

        // Sadece Z farkı ile hesap (performans)
        float distanceZ = transform.position.z - targetPlayer.position.z;

        // Yaklaşınca X ekseninde oyuncuya doğru yumuşakça kay
        if (distanceZ <= triggerDistance && distanceZ > 0f)
        {
            float newX = Mathf.Lerp(transform.position.x, targetPlayer.position.x, curveStrength * Time.deltaTime);
            Vector3 pos = transform.position;
            pos.x = newX;
            transform.position = pos;
        }
    }
}