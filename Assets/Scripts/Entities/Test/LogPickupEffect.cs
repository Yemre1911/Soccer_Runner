using UnityEngine;

public class LogPickupEffect : MonoBehaviour, IPickupEffect
{
    [SerializeField] private string message = "";

    public void Apply(PickupContext context)
    {
        string label = string.IsNullOrWhiteSpace(message) ? context.itemGO.name : message;
        Debug.Log($"[Pickup] Collected: {label}", context.itemGO);
    }
}