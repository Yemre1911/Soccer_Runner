using UnityEngine;

public class ScoreMultiplierBoosterEffect : IBoosterEffect
{
    private readonly ScoreMultiplierEffectDefinitionSO _definition;
    private ScoreMultiplierTarget _target;

    public ScoreMultiplierBoosterEffect(ScoreMultiplierEffectDefinitionSO definition)
    {
        _definition = definition;
    }

    public void OnApply(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_definition == null || context == null) return;

        _target = context.GetScoreMultiplierTarget();
        if (_target == null)
        {
            Debug.LogWarning("[ScoreMultiplierBoosterEffect] ScoreMultiplierTarget not found.", context);
            return;
        }

        _target.SetMultiplier(_definition.multiplier);
    }

    public void OnRemove(ItemDefinitionSO itemDefinition, PlayerEffectContext context)
    {
        if (_target == null) return;
        _target.ResetMultiplier();
    }
}