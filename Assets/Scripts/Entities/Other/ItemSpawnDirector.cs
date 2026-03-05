using UnityEngine;

/// <summary>
/// SessionDirector phase'lerine göre spawner policy set eder.
/// Spawner item türlerini bilmez.
/// </summary>
public class ItemSpawnDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SessionDirector sessionDirector;
    [SerializeField] private WeightedItemSpawner spawner;

    [Header("Policies")]
    [SerializeField] private WeightedSpawnPolicySO startingPolicy;
    [SerializeField] private WeightedSpawnPolicySO wavePolicy;
    [SerializeField] private WeightedSpawnPolicySO breakPolicy;
    [SerializeField] private WeightedSpawnPolicySO endingPolicy;

    private void Awake()
    {
        if (sessionDirector == null)
            sessionDirector = FindFirstObjectByType<SessionDirector>();

        if (spawner == null)
            spawner = FindFirstObjectByType<WeightedItemSpawner>();
    }

    private void OnEnable()
    {
        if (sessionDirector != null)
            sessionDirector.OnPhaseChanged += HandlePhaseChanged;

        // initial
        if (sessionDirector != null)
            Apply(sessionDirector.CurrentPhase);
    }

    private void OnDisable()
    {
        if (sessionDirector != null)
            sessionDirector.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(SessionDirector.Phase phase)
    {
        Apply(phase);
    }

    private void Apply(SessionDirector.Phase phase)
    {
        if (spawner == null) return;

        WeightedSpawnPolicySO p = phase switch
        {
            SessionDirector.Phase.Starting => startingPolicy,
            SessionDirector.Phase.Wave => wavePolicy,
            SessionDirector.Phase.Break => breakPolicy,
            SessionDirector.Phase.Ending => endingPolicy,
            _ => null
        };

        if (p == null)
        {
            spawner.SetEnabled(false);
            return;
        }

        spawner.SetPolicy(p, resetSchedule: true);
        spawner.SetEnabled(true);
    }
}