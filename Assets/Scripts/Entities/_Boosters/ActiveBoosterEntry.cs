using System;

[Serializable]
public class ActiveBoosterEntry
{
    public ItemDefinitionSO itemDefinition;
    public float remainingTime;
    public float totalDuration;
    public IBoosterEffect runtimeEffect;

    public string ItemId
    {
        get
        {
            if (itemDefinition == null) return string.Empty;
            return string.IsNullOrWhiteSpace(itemDefinition.itemId)
                ? itemDefinition.name
                : itemDefinition.itemId;
        }
    }

    public float NormalizedRemaining
    {
        get
        {
            if (totalDuration <= 0.0001f) return 0f;
            return remainingTime / totalDuration;
        }
    }
}