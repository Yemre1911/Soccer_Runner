using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinPickup : PooledItem
{
    [SerializeField] private int value = 1;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Basit: Player tag ile topla
        if (!other.CompareTag("Player")) return;

        if (CoinManager.Instance != null)
            CoinManager.Instance.Add(value);

        Despawn();
    }
}