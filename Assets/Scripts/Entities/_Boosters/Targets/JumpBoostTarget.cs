/*using UnityEngine;

public class JumpBoostTarget : MonoBehaviour
{
    [Header("Base")]
    [SerializeField] private float baseMultiplier = 1f;

    [Header("Runtime")]
    [SerializeField] private float currentMultiplier = 1f;

    public float BaseMultiplier => baseMultiplier;
    public float CurrentMultiplier => currentMultiplier;

    private void Awake()
    {
        currentMultiplier = baseMultiplier;
    }

    public void SetBoostMultiplier(float multiplier)
    {
        currentMultiplier = Mathf.Max(0f, multiplier);
        Debug.Log($"[JumpBoostTarget] Multiplier set -> {currentMultiplier}", this);
    }

    public void ResetMultiplier()
    {
        currentMultiplier = baseMultiplier;
        Debug.Log($"[JumpBoostTarget] Multiplier reset -> {currentMultiplier}", this);
    }
}*/

using UnityEngine;

public class JumpBoostTarget : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerVerticalMovement verticalMovement;

    [Header("Base")]
    [SerializeField] private float baseMultiplier = 1f;

    [Header("Runtime")]
    [SerializeField] private float currentMultiplier = 1f;

    public float BaseMultiplier => baseMultiplier;
    public float CurrentMultiplier => currentMultiplier;

    private void Awake()
    {
        if (!verticalMovement)
            verticalMovement = GetComponentInParent<PlayerVerticalMovement>();

        currentMultiplier = baseMultiplier;
    }

    public void SetBoostMultiplier(float multiplier)
    {
        currentMultiplier = Mathf.Max(0.1f, multiplier);

        if (verticalMovement != null)
            verticalMovement.SetJumpHeightMultiplier(currentMultiplier);

        Debug.Log($"[JumpBoostTarget] Multiplier set -> {currentMultiplier}", this);
    }

    public void ResetMultiplier()
    {
        currentMultiplier = baseMultiplier;

        if (verticalMovement != null)
            verticalMovement.ResetJumpHeightMultiplier();

        Debug.Log($"[JumpBoostTarget] Multiplier reset -> {currentMultiplier}", this);
    }
}