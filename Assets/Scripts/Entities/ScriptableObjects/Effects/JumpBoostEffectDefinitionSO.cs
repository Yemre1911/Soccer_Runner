using UnityEngine;

[CreateAssetMenu(menuName = "SoccerRunner/Effects/Jump Boost Effect", fileName = "Effect_JumpBoost_")]
public class JumpBoostEffectDefinitionSO : EffectDefinitionSO
{
    [Min(0f)] public float jumpHeightMultiplier = 1.5f;

    public override BoosterEffectType EffectType => BoosterEffectType.JumpBoost;
}