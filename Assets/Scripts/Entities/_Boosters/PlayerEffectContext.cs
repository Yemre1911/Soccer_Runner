using UnityEngine;

public class PlayerEffectContext : MonoBehaviour
{
    [Header("Optional refs")]
    [SerializeField] private MonoBehaviour jumpBoostTarget;
    [SerializeField] private MonoBehaviour magnetTarget;

    public T GetOptional<T>() where T : class
    {
        if (jumpBoostTarget is T jt) return jt;
        if (magnetTarget is T mt) return mt;

        var found = GetComponentInChildren(typeof(T), true) as T;
        if (found != null) return found;

        found = GetComponentInParent(typeof(T)) as T;
        return found;
    }
}