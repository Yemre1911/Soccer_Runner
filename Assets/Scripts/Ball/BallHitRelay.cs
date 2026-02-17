using UnityEngine;
public class BallHitRelay : MonoBehaviour
{
    [SerializeField] private HitResolver resolver;

    private void OnTriggerEnter(Collider other)
    {
        var opp = other.GetComponentInParent<OpponentIdentity>();
        if (opp != null)
            resolver.NotifyBallHit(opp);
    }
}