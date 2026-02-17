using UnityEngine;
public class PlayerBodyHitRelay : MonoBehaviour
{
    [SerializeField] private HitResolver resolver;

    private void OnTriggerEnter(Collider other)
    {
        var opp = other.GetComponentInParent<OpponentIdentity>();
        if (opp != null)
            resolver.NotifyPlayerHit(opp);
    }
}