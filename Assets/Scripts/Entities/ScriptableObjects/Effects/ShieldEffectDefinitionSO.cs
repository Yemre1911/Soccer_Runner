using UnityEngine;

[CreateAssetMenu(menuName = "SoccerRunner/Effects/Shield Effect", fileName = "Effect_Shield_")]
public class ShieldEffectDefinitionSO : EffectDefinitionSO
{
    public override BoosterEffectType EffectType => BoosterEffectType.Shield;
}