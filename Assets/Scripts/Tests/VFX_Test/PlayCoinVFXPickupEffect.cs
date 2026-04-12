using UnityEngine;

public class PlayCoinVFXPickupEffect : MonoBehaviour, IPickupEffect
{
    public void Apply(PickupContext context)
    {
        if (context.picker == null) return;

        PlayerVFXController vfx = context.picker.GetComponentInChildren<PlayerVFXController>(true);
        if (vfx != null)
            vfx.PlayCoinVFX();

    }
}