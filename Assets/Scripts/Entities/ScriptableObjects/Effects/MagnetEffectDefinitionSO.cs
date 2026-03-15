using UnityEngine;

[CreateAssetMenu(menuName = "SoccerRunner/Effects/Magnet Effect", fileName = "Effect_Magnet_")]
public class MagnetEffectDefinitionSO : EffectDefinitionSO
{
    [Min(0f)] public float magnetRadius = 8f;

    public override BoosterEffectType EffectType => BoosterEffectType.Magnet;
}