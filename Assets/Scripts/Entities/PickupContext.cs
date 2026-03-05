using UnityEngine;

public readonly struct PickupContext
{
    public readonly Transform picker;     // player root
    public readonly GameObject itemGO;    // bu pickup objesi

    public PickupContext(Transform picker, GameObject itemGO)
    {
        this.picker = picker;
        this.itemGO = itemGO;
    }
}