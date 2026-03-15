using UnityEngine;

[CreateAssetMenu(menuName = "SoccerRunner/Effects/Score Multiplier Effect", fileName = "Effect_ScoreMultiplier_")]
public class ScoreMultiplierEffectDefinitionSO : EffectDefinitionSO
{
    [Min(1f)] public float multiplier = 2f;

    public override BoosterEffectType EffectType => BoosterEffectType.ScoreMultiplier;
}