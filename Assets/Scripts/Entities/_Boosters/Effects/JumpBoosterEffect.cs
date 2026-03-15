/*using UnityEngine;

public class JumpBoosterEffect : IBoosterEffect
{
    private readonly JumpBoostEffectDefinitionSO _definition;
    private JumpBoostTarget _target;

    public JumpBoosterEffect(JumpBoostEffectDefinitionSO definition)
    {
        _definition = definition;
    }

    public void OnApply(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_definition == null || context == null) return;

        _target = context.GetJumpTarget();
        if (_target == null)
        {
            Debug.LogWarning("[JumpBoosterEffect] JumpBoostTarget not found.", context);
            return;
        }

        _target.SetBoostMultiplier(_definition.jumpHeightMultiplier);
    }

    public void OnRemove(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_target == null) return;
        _target.ResetMultiplier();
    }
}*/

using UnityEngine;

public class JumpBoosterEffect : IBoosterEffect
{
    private readonly JumpBoostEffectDefinitionSO _definition;
    private JumpBoostTarget _target;
    private JumpVisualTarget _visualTarget;

    public JumpBoosterEffect(JumpBoostEffectDefinitionSO definition)
    {
        _definition = definition;
    }

    public void OnApply(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_definition == null || context == null) return;

        _target = context.GetJumpTarget();
        _visualTarget = context.GetJumpVisualTarget();

        if (_target == null)
        {
            Debug.LogWarning("[JumpBoosterEffect] JumpBoostTarget not found.", context);
        }
        else
        {
            _target.SetBoostMultiplier(_definition.jumpHeightMultiplier);
        }

        if (_visualTarget != null)
        {
            _visualTarget.SetActiveVisual(true);
            _visualTarget.SetAnimatorBoostState(true);
        }
    }

    public void OnRemove(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_target != null)
            _target.ResetMultiplier();

        if (_visualTarget != null)
        {
            _visualTarget.SetAnimatorBoostState(false);
            _visualTarget.SetActiveVisual(false);
        }
    }
}