using System.Collections.Generic;
using UnityEngine;

public class WeightedItemSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeightedSpawnPolicySO policy;
    [SerializeField] private SessionDirector sessionDirector;
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Optional Overrides")]
    [Tooltip("Policy yoksa çalışmaz. Runtime'da director set edebilir.")]
    [SerializeField] private bool startEnabled = true;

    private IRunDistance _runDistance;
    private float _nextSpawnAtDistance;
    private float _lastSpawnDistance = -999999f;

    // cooldown tracking: poolId -> nextAllowedDistance
    private readonly Dictionary<string, float> _cooldownUntil = new();

    // per-item separation tracking (optional): poolId -> lastSpawnDistance
    private readonly Dictionary<string, float> _lastSpawnById = new();

    private bool _enabled;

    private void Awake()
    {
        ResolveRefs();
        _enabled = startEnabled;
        ScheduleNextSpawn(initial: true);
    }

    private void OnEnable()
    {
        ResolveRefs();
        if (sessionDirector != null)
            sessionDirector.OnPhaseChanged += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        if (sessionDirector != null)
            sessionDirector.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void ResolveRefs()
    {
        if (_runDistance == null)
        {
            // sahnede bir RunDistanceProvider varsa onu bul
            _runDistance = FindFirstObjectByType<RunDistanceProvider>();
        }

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }

        if (sessionDirector == null)
            sessionDirector = FindFirstObjectByType<SessionDirector>();
    }

    public void SetPolicy(WeightedSpawnPolicySO newPolicy, bool resetSchedule = true)
    {
        policy = newPolicy;
        if (resetSchedule) ScheduleNextSpawn(initial: true);
    }

    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
    }

    private void HandlePhaseChanged(SessionDirector.Phase phase)
    {
        // Director zaten policy set edebilir. Burada sadece enable/disable yapıyoruz.
        if (policy == null)
        {
            _enabled = false;
            return;
        }

        _enabled = policy.activeInPhases.HasFlag(ToMask(phase));
        if (_enabled)
        {
            // Phase açılınca “bir daha hiç spawn etmiyor” bug’ını önlemek için schedule reset:
            ScheduleNextSpawn(initial: true);
        }
    }

    private void Update()
    {
        if (!_enabled) return;
        if (policy == null) return;
        if (_runDistance == null || player == null) return;
        if (ItemPooler.Instance == null) return;

        // phase check (ek güvenlik)
        if (sessionDirector != null)
        {
            var phaseMask = ToMask(sessionDirector.CurrentPhase);
            if (!policy.activeInPhases.HasFlag(phaseMask))
                return;
        }

        float d = _runDistance.RunDistance;

        if (d < _nextSpawnAtDistance)
            return;

        // global separation
        if (d - _lastSpawnDistance < policy.globalMinSeparationMeters)
        {
            ScheduleNextSpawn(initial: false);
            return;
        }

        var def = PickEligibleItem(d);
        if (def == null)
        {
            // hiç eligible yoksa bir sonraki denemeyi ileri al
            ScheduleNextSpawn(initial: false);
            return;
        }

        // Spawn pos
        float ahead = Random.Range(policy.minAheadMeters, policy.maxAheadMeters);
        ahead = Mathf.Min(ahead, policy.hardMaxAheadMeters);

        float x = Random.Range(policy.minX, policy.maxX);
        float z = player.position.z + ahead + (policy.zJitter > 0f ? Random.Range(-policy.zJitter, policy.zJitter) : 0f);
        float y = policy.spawnY;

        Vector3 pos = new Vector3(x, y, z);

        // Spawn
        var go = ItemPooler.Instance.Spawn(def.poolId, pos, Quaternion.identity);
        if (go != null)
        {
            // cooldown & last spawn tracking
            if (def.cooldownMeters > 0f)
                _cooldownUntil[def.poolId] = d + def.cooldownMeters;

            _lastSpawnById[def.poolId] = d;
            _lastSpawnDistance = d;
        }

        ScheduleNextSpawn(initial: false);
    }

    private ItemDefinitionSO PickEligibleItem(float currentDistance)
    {
        if (policy.items == null || policy.items.Length == 0) return null;

        // Eligible list oluştur (GC istemiyorsan list pool yapılır; şimdilik sade)
        List<ItemDefinitionSO> eligible = null;

        var phase = sessionDirector != null ? sessionDirector.CurrentPhase : SessionDirector.Phase.Wave;
        var phaseMask = ToMask(phase);

        for (int i = 0; i < policy.items.Length; i++)
        {
            var it = policy.items[i];
            if (it == null) continue;
            if (string.IsNullOrEmpty(it.poolId)) continue;
            if (it.weight <= 0f) continue;

            // phase allow
            if (!it.allowedPhases.HasFlag(phaseMask))
                continue;

            // cooldown
            if (_cooldownUntil.TryGetValue(it.poolId, out var until) && currentDistance < until)
                continue;

            // per-item separation
            if (it.minSeparationMeters > 0f && _lastSpawnById.TryGetValue(it.poolId, out var lastD))
            {
                if (currentDistance - lastD < it.minSeparationMeters)
                    continue;
            }

            eligible ??= new List<ItemDefinitionSO>(8);
            eligible.Add(it);
        }

        if (eligible == null || eligible.Count == 0) return null;

        // Weighted pick
        return WeightedPicker.Pick(eligible.ToArray(), e => e.weight);
    }

    private void ScheduleNextSpawn(bool initial)
    {
        if (policy == null || _runDistance == null)
        {
            _nextSpawnAtDistance = 9999999f;
            return;
        }

        float d = _runDistance.RunDistance;

        float gap = Random.Range(policy.minGapMeters, policy.maxGapMeters);
        if (initial) gap = Mathf.Max(5f, gap * 0.5f); // ilk spawn çok gecikmesin

        _nextSpawnAtDistance = d + gap;
    }

    private static SpawnPhaseMask ToMask(SessionDirector.Phase p)
    {
        return p switch
        {
            SessionDirector.Phase.Starting => SpawnPhaseMask.Starting,
            SessionDirector.Phase.Wave => SpawnPhaseMask.Wave,
            SessionDirector.Phase.Break => SpawnPhaseMask.Break,
            SessionDirector.Phase.Ending => SpawnPhaseMask.Ending,
            _ => SpawnPhaseMask.None
        };
    }
}