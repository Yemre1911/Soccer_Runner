using UnityEngine;

public class ScoreMultiplierTarget : MonoBehaviour
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

    public void SetMultiplier(float multiplier)
    {
        currentMultiplier = Mathf.Max(1f, multiplier);
        Debug.Log($"[ScoreMultiplierTarget] Multiplier set -> {currentMultiplier}", this);
    }

    public void ResetMultiplier()
    {
        currentMultiplier = baseMultiplier;
        Debug.Log($"[ScoreMultiplierTarget] Multiplier reset -> {currentMultiplier}", this);
    }
}