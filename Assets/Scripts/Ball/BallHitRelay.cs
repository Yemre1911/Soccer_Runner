using UnityEngine;

public class BallHitRelay : MonoBehaviour
{
    [SerializeField] private HitResolver resolver;

    [Header("Only these layers count as HIT (OpponentHit)")]
    [SerializeField] private LayerMask opponentHitLayers;

    private void OnTriggerEnter(Collider other)
    {
        // NearMiss collider'ını asla ball hit sayma
        if (other.GetComponent<OpponentNearMissZone>() != null) return;

        if ((opponentHitLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        var opp = other.GetComponentInParent<OpponentIdentity>();
        if (opp != null)
            resolver.NotifyBallHit(opp);
    }
}