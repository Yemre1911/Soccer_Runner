using UnityEngine;

public class GrantItemPickupEffect : MonoBehaviour, IPickupEffect
{
    [SerializeField] private ItemDefinitionSO itemDefinition;

    public void Apply(PickupContext context)
    {
        if (itemDefinition == null)
        {
            Debug.LogWarning("[GrantItemPickupEffect] ItemDefinition is null.", this);
            return;
        }

        if (context.picker == null)
        {
            Debug.LogWarning("[GrantItemPickupEffect] Picker is null.", this);
            return;
        }

        IItemReceiver receiver = context.picker.GetComponentInParent<IItemReceiver>();
        if (receiver == null)
        {
            Debug.LogWarning(
                $"[GrantItemPickupEffect] No IItemReceiver found on picker root: {context.picker.name}",
                context.picker
            );
            return;
        }

        receiver.ReceiveItem(itemDefinition);
    }
}