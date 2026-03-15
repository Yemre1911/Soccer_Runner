using UnityEngine;

public abstract class EffectDefinitionSO : ScriptableObject
{
    public abstract BoosterEffectType EffectType { get; }
}