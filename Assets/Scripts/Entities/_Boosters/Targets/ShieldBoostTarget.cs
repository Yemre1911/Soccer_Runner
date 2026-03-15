/*using UnityEngine;

public class ShieldBoostTarget : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject shieldVisualRoot;

    [Tooltip("Shield aktifken kapatılacak collider'lar")]
    [SerializeField] private Collider[] collidersToDisable;

    private bool _active;

    public void SetShieldActive(bool active)
    {
        _active = active;

        if (shieldVisualRoot)
            shieldVisualRoot.SetActive(active);

        if (collidersToDisable != null)
        {
            for (int i = 0; i < collidersToDisable.Length; i++)
            {
                if (collidersToDisable[i] != null)
                    collidersToDisable[i].enabled = !active;
            }
        }

        Debug.Log($"[ShieldBoostTarget] Active -> {_active}", this);
    }
}*/

using UnityEngine;

public class ShieldBoostTarget : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject shieldVisualRoot;
    [SerializeField] private HitResolver hitResolver;

    private bool _active;

    private void Awake()
    {
        if (!hitResolver)
            hitResolver = GetComponentInParent<HitResolver>();

        if (shieldVisualRoot)
            shieldVisualRoot.SetActive(false);
    }

    public void SetShieldActive(bool active)
    {
        _active = active;

        if (shieldVisualRoot)
            shieldVisualRoot.SetActive(active);

        if (hitResolver != null)
            hitResolver.SetShieldActive(active);
        else
            Debug.LogWarning("[ShieldBoostTarget] HitResolver not found.", this);

        Debug.Log($"[ShieldBoostTarget] Active -> {_active}", this);
    }
}