using UnityEngine;

public class ShieldBoosterEffect : IBoosterEffect
{
    private readonly ShieldEffectDefinitionSO _definition;
    private ShieldBoostTarget _target;

    public ShieldBoosterEffect(ShieldEffectDefinitionSO definition)
    {
        _definition = definition;
    }

    public void OnApply(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_definition == null || context == null) return;

        _target = context.GetShieldTarget();
        if (_target == null)
        {
            Debug.LogWarning("[ShieldBoosterEffect] ShieldBoostTarget not found.", context);
            return;
        }

        _target.SetShieldActive(true);
    }

    public void OnRemove(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_target == null) return;
        _target.SetShieldActive(false);
    }
}