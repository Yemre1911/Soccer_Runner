/*using UnityEngine;

public class MagnetBoosterEffect : IBoosterEffect
{
    private readonly MagnetEffectDefinitionSO _definition;
    private MagnetBoostTarget _target;

    public MagnetBoosterEffect(MagnetEffectDefinitionSO definition)
    {
        _definition = definition;
    }

    public void OnApply(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_definition == null || context == null) return;

        _target = context.GetMagnetTarget();
        if (_target == null)
        {
            Debug.LogWarning("[MagnetBoosterEffect] MagnetBoostTarget not found.", context);
            return;
        }

        _target.SetBoostedRadius(_definition.magnetRadius);
    }

    public void OnRemove(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_target == null) return;
        _target.ResetRadius();
    }
}*/

using UnityEngine;

public class MagnetBoosterEffect : IBoosterEffect
{
    private readonly MagnetEffectDefinitionSO _definition;
    private MagnetBoostTarget _target;
    private MagnetVisualTarget _visualTarget;

    public MagnetBoosterEffect(MagnetEffectDefinitionSO definition)
    {
        _definition = definition;
    }

    public void OnApply(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_definition == null || context == null) return;

        _target = context.GetMagnetTarget();
        _visualTarget = context.GetMagnetVisualTarget();

        if (_target == null)
        {
            Debug.LogWarning("[MagnetBoosterEffect] MagnetBoostTarget not found.", context);
        }
        else
        {
            _target.SetBoostedRadius(_definition.magnetRadius);
        }

        if (_visualTarget != null)
            _visualTarget.SetActiveVisual(true);
    }

    public void OnRemove(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_target != null)
            _target.ResetRadius();

        if (_visualTarget != null)
            _visualTarget.SetActiveVisual(false);
    }
}