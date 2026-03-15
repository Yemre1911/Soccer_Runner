public interface IBoosterEffect
{
    void OnApply(ItemDefinitionSO itemDefinition, PlayerEffectContext context);
    void OnRemove(ItemDefinitionSO itemDefinition, PlayerEffectContext context);
}