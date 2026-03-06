/*using UnityEngine;

public class PlayerBodyHitRelay : MonoBehaviour
{
    [SerializeField] private HitResolver resolver;

    [Header("Only these layers count as HIT (e.g. OpponentHit)")]
    [SerializeField] private LayerMask opponentHitLayers;

    private void OnTriggerEnter(Collider other)
    {
        // NearMiss collider'ını asla hit sayma
        if (other.GetComponent<OpponentNearMissZone>() != null) return;

        // Sadece HIT layer’ları
        if ((opponentHitLayers.value & (1 << other.gameObject.layer)) == 0) return;

        var opp = other.GetComponentInParent<OpponentIdentity>();
        if (opp != null)
            resolver.NotifyPlayerHit(opp);
    }
}*/

using UnityEngine;

public class PlayerBodyHitRelay : MonoBehaviour
{
    [SerializeField] private HitResolver resolver;

    [Header("Only these layers count as HIT")]
    [SerializeField] private LayerMask hitLayers;

    private void OnTriggerEnter(Collider other)
    {
        if (GameOver.Instance.IsGameOver) return;

        // NearMiss collider'ını hit sayma
        if (other.GetComponent<OpponentNearMissZone>() != null) return;

        // Sadece izinli layer'lar
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0) return;

        // 1) Rakip mi?
        var opp = other.GetComponentInParent<OpponentIdentity>();
        if (opp != null)
        {
            resolver.NotifyPlayerHit(opp);
            return;
        }

        // 2) Yerdeki tehlike mi?
        var hazard = other.GetComponentInParent<GroundHazard>();
        if (hazard != null)
        {
            resolver.NotifyGroundHazardHit();
            return;
        }
    }
}