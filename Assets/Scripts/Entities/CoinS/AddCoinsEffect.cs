using UnityEngine;

public class AddCoinsEffect : MonoBehaviour, IPickupEffect
{
    [SerializeField] private int value = 1;

    public void Apply(PickupContext context)
    {
        if (value <= 0) return;

        if (CoinManager.Instance != null)
            CoinManager.Instance.Add(value);
    }

#if UNITY_EDITOR
    private void OnValidate() => value = Mathf.Max(0, value);
#endif
}