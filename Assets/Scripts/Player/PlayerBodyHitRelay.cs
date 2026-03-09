using UnityEngine;

public class PlayerBodyHitRelay : MonoBehaviour
{
    [SerializeField] private HitResolver resolver;

    [Header("Only these layers count as HIT (e.g. OpponentHit, HazardHit)")]
    [SerializeField] private LayerMask opponentHitLayers;

    private void OnTriggerEnter(Collider other)
    {
        // NearMiss collider'ını asla hit sayma
        if (other.GetComponent<OpponentNearMissZone>() != null) return;

        // Sadece izin verilen HIT layer'ları
        if ((opponentHitLayers.value & (1 << other.gameObject.layer)) == 0) return;

        // Önce rakip kontrolü
        var opp = other.GetComponentInParent<OpponentIdentity>();
        if (opp != null)
        {
            resolver.NotifyPlayerHit(opp);
            return;
        }

        // Rakip değilse hazard kontrolü
        var hazard = other.GetComponentInParent<GroundHazard>();
        if (hazard != null)
        {
            resolver.NotifyGroundHazardHit();
        }
    }
}