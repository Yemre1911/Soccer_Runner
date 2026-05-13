/*using UnityEngine;

public class GoalNetBlendShapeController : MonoBehaviour
{
    public enum NetHitArea
    {
        Left,
        Right,
        Center
    }

    [Header("References")]
    [SerializeField] private SkinnedMeshRenderer netRenderer;

    [Header("BlendShape Indices")]
    [Tooltip("Oyuncu perspektifinden kalenin sol ağı. Genelde Key 1.")]
    [SerializeField] private int leftBlendShapeIndex = 0;

    [Tooltip("Oyuncu perspektifinden kalenin sağ ağı. Genelde Key 2.")]
    [SerializeField] private int rightBlendShapeIndex = 1;

    [Tooltip("Kalenin orta ağı. Genelde Key 3.")]
    [SerializeField] private int centerBlendShapeIndex = 2;

    [Header("Motion Settings")]
    [Tooltip("BlendShape'in çıkacağı maksimum değer. Unity BlendShape genelde 0-100 arasıdır.")]
    [SerializeField] private float maxWeight = 100f;

    [Tooltip("Ağın ne kadar hızlı şişeceği.")]
    [SerializeField] private float expandSpeed = 850f;

    [Tooltip("Ağın ne kadar yavaş geri söneceği.")]
    [SerializeField] private float relaxSpeed = 180f;

    [Tooltip("Tetiklenince hedefte kısa süre beklesin mi?")]
    [SerializeField] private float holdAtMaxTime = 0.05f;

    [Header("Behaviour")]
    [SerializeField] private bool resetOtherAreasOnHit = false;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private BlendShapeMotion _left;
    private BlendShapeMotion _right;
    private BlendShapeMotion _center;

    private void Awake()
    {
        if (!netRenderer)
            netRenderer = GetComponent<SkinnedMeshRenderer>();

        if (!netRenderer)
        {
            Debug.LogError("[GoalNetBlendShapeController] SkinnedMeshRenderer missing.");
            enabled = false;
            return;
        }

        netRenderer.updateWhenOffscreen = true;

        _left = new BlendShapeMotion(leftBlendShapeIndex);
        _right = new BlendShapeMotion(rightBlendShapeIndex);
        _center = new BlendShapeMotion(centerBlendShapeIndex);

        ResetAllWeights();
    }

    private void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        UpdateMotion(_left, dt);
        UpdateMotion(_right, dt);
        UpdateMotion(_center, dt);
    }

    public void PlayHit(NetHitArea area)
    {
        if (!netRenderer)
            return;

        if (resetOtherAreasOnHit)
            StopAllExcept(area);

        BlendShapeMotion motion = GetMotion(area);

        motion.TargetWeight = maxWeight;
        motion.HoldTimer = holdAtMaxTime;
        motion.IsExpanding = true;

        if (debugLogs)
            Debug.Log($"[GoalNetBlendShapeController] Net hit: {area}");
    }

    public void ResetAllWeights()
    {
        ForceWeight(_left, 0f);
        ForceWeight(_right, 0f);
        ForceWeight(_center, 0f);
    }

    private void UpdateMotion(BlendShapeMotion motion, float dt)
    {
        if (!IsValidBlendShapeIndex(motion.Index))
            return;

        float current = motion.CurrentWeight;

        if (motion.IsExpanding)
        {
            current = Mathf.MoveTowards(current, motion.TargetWeight, expandSpeed * dt);

            if (Mathf.Approximately(current, motion.TargetWeight))
            {
                if (motion.HoldTimer > 0f)
                {
                    motion.HoldTimer -= dt;
                }
                else
                {
                    motion.IsExpanding = false;
                    motion.TargetWeight = 0f;
                }
            }
        }
        else
        {
            current = Mathf.MoveTowards(current, 0f, relaxSpeed * dt);
        }

        motion.CurrentWeight = current;
        netRenderer.SetBlendShapeWeight(motion.Index, current);
    }

    private BlendShapeMotion GetMotion(NetHitArea area)
    {
        switch (area)
        {
            case NetHitArea.Left:
                return _left;

            case NetHitArea.Right:
                return _right;

            case NetHitArea.Center:
                return _center;

            default:
                return _center;
        }
    }

    private void StopAllExcept(NetHitArea area)
    {
        if (area != NetHitArea.Left)
        {
            _left.IsExpanding = false;
            _left.TargetWeight = 0f;
            _left.HoldTimer = 0f;
        }

        if (area != NetHitArea.Right)
        {
            _right.IsExpanding = false;
            _right.TargetWeight = 0f;
            _right.HoldTimer = 0f;
        }

        if (area != NetHitArea.Center)
        {
            _center.IsExpanding = false;
            _center.TargetWeight = 0f;
            _center.HoldTimer = 0f;
        }
    }

    private void ForceWeight(BlendShapeMotion motion, float weight)
    {
        motion.CurrentWeight = weight;
        motion.TargetWeight = 0f;
        motion.HoldTimer = 0f;
        motion.IsExpanding = false;

        if (netRenderer && IsValidBlendShapeIndex(motion.Index))
            netRenderer.SetBlendShapeWeight(motion.Index, weight);
    }

    private bool IsValidBlendShapeIndex(int index)
    {
        if (!netRenderer || !netRenderer.sharedMesh)
            return false;

        return index >= 0 && index < netRenderer.sharedMesh.blendShapeCount;
    }

    private class BlendShapeMotion
    {
        public int Index;
        public float CurrentWeight;
        public float TargetWeight;
        public float HoldTimer;
        public bool IsExpanding;

        public BlendShapeMotion(int index)
        {
            Index = index;
        }
    }
}*/

using UnityEngine;

public class GoalNetBlendShapeController : MonoBehaviour
{
    public enum NetHitArea
    {
        Left,
        Right,
        Center
    }

    private enum MotionPhase
    {
        Idle,
        Attack,
        Hold,
        Release
    }

    [Header("References")]
    [SerializeField] private SkinnedMeshRenderer netRenderer;

    [Header("BlendShape Indices")]
    [Tooltip("Oyuncu perspektifinden kalenin sol ağı. Genelde Key 1.")]
    [SerializeField] private int leftBlendShapeIndex = 0;

    [Tooltip("Oyuncu perspektifinden kalenin sağ ağı. Genelde Key 2.")]
    [SerializeField] private int rightBlendShapeIndex = 1;

    [Tooltip("Kalenin orta ağı. Genelde Key 3.")]
    [SerializeField] private int centerBlendShapeIndex = 2;

    [Header("Weight")]
    [Tooltip("Ana blendshape hedef değeri. Unity BlendShape genelde 0-100 arası kullanılır.")]
    [SerializeField] private float maxWeight = 100f;

    [Tooltip("Impact anında hafif punch etkisi için hedef değeri kısa süreli max üstüne çıkarabilir. Güvenli istemiyorsan 100 bırak.")]
    [SerializeField] private float punchWeight = 105f;

    [Tooltip("BlendShape değerini 0-100 aralığında sınırlar.")]
    [SerializeField] private bool clampToHundred = true;

    [Header("Timing")]
    [Tooltip("Ağın impact sonrası şişme süresi. Küçük değer = daha sert vuruş.")]
    [SerializeField] private float attackDuration = 0.09f;

    [Tooltip("Ağ maksimum şişkinlikte çok kısa bekler.")]
    [SerializeField] private float holdDuration = 0.035f;

    [Tooltip("Ağın eski haline dönme süresi. Attack'tan daha uzun olmalı.")]
    [SerializeField] private float releaseDuration = 0.55f;

    [Header("Easing Curves")]
    [Tooltip("0'dan 1'e attack eğrisi. Hızlı başlayıp yumuşak bitmesi iyi görünür.")]
    [SerializeField]
    private AnimationCurve attackCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 5f),
        new Keyframe(0.55f, 0.88f, 1.5f, 1.5f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    [Tooltip("1'den 0'a dönüş eğrisi. Başta yavaş, sonra yumuşak sönmesi doğal görünür.")]
    [SerializeField]
    private AnimationCurve releaseCurve = new AnimationCurve(
        new Keyframe(0f, 1f, 0f, -1.2f),
        new Keyframe(0.45f, 0.38f, -1.4f, -1.4f),
        new Keyframe(1f, 0f, -0.15f, 0f)
    );

    [Header("Secondary Net Reaction")]
    [Tooltip("Sol/sağ vuruşlarda merkezin de azıcık hareket etmesini sağlar.")]
    [SerializeField] private bool enableSecondaryReaction = true;

    [Range(0f, 1f)]
    [SerializeField] private float centerSecondaryMultiplier = 0.28f;

    [Range(0f, 1f)]
    [SerializeField] private float sideSecondaryMultiplier = 0.18f;

    [Header("Behaviour")]
    [Tooltip("Aynı anda sadece son vurulan bölge oynasın istiyorsan aç. Daha organik görüntü için kapalı kalabilir.")]
    [SerializeField] private bool resetOtherAreasOnHit = false;

    [SerializeField] private bool useUnscaledTime = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private BlendShapeMotion _left;
    private BlendShapeMotion _right;
    private BlendShapeMotion _center;

    private void Awake()
    {
        if (!netRenderer)
            netRenderer = GetComponent<SkinnedMeshRenderer>();

        if (!netRenderer)
        {
            Debug.LogError("[GoalNetBlendShapeController] SkinnedMeshRenderer missing.");
            enabled = false;
            return;
        }

        netRenderer.updateWhenOffscreen = true;

        _left = new BlendShapeMotion(leftBlendShapeIndex);
        _right = new BlendShapeMotion(rightBlendShapeIndex);
        _center = new BlendShapeMotion(centerBlendShapeIndex);

        ResetAllWeights();
    }

    private void Update()
    {
        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        UpdateMotion(_left, dt);
        UpdateMotion(_right, dt);
        UpdateMotion(_center, dt);
    }

    public void PlayHit(NetHitArea area)
    {
        PlayHit(area, 1f);
    }

    public void PlayHit(NetHitArea area, float intensity01)
    {
        if (!netRenderer)
            return;

        intensity01 = Mathf.Clamp01(intensity01);

        if (resetOtherAreasOnHit)
            StopAllExcept(area);

        TriggerArea(area, intensity01, 1f);

        if (enableSecondaryReaction)
            TriggerSecondaryAreas(area, intensity01);

        if (debugLogs)
            Debug.Log($"[GoalNetBlendShapeController] Net hit: {area}, Intensity: {intensity01}");
    }

    public void ResetAllWeights()
    {
        ForceWeight(_left, 0f);
        ForceWeight(_right, 0f);
        ForceWeight(_center, 0f);
    }

    private void TriggerArea(NetHitArea area, float intensity01, float multiplier)
    {
        BlendShapeMotion motion = GetMotion(area);

        if (motion == null)
            return;

        if (!IsValidBlendShapeIndex(motion.Index))
            return;

        float target = Mathf.Lerp(maxWeight * 0.45f, punchWeight, intensity01) * multiplier;

        motion.StartWeight = motion.CurrentWeight;
        motion.PeakWeight = SanitizeWeight(target);
        motion.ReleaseStartWeight = motion.PeakWeight;

        motion.Timer = 0f;
        motion.HoldTimer = holdDuration;
        motion.Phase = MotionPhase.Attack;
    }

    private void TriggerSecondaryAreas(NetHitArea mainArea, float intensity01)
    {
        switch (mainArea)
        {
            case NetHitArea.Left:
                TriggerArea(NetHitArea.Center, intensity01, centerSecondaryMultiplier);
                break;

            case NetHitArea.Right:
                TriggerArea(NetHitArea.Center, intensity01, centerSecondaryMultiplier);
                break;

            case NetHitArea.Center:
                TriggerArea(NetHitArea.Left, intensity01, sideSecondaryMultiplier);
                TriggerArea(NetHitArea.Right, intensity01, sideSecondaryMultiplier);
                break;
        }
    }

    private void UpdateMotion(BlendShapeMotion motion, float dt)
    {
        if (motion == null)
            return;

        if (!IsValidBlendShapeIndex(motion.Index))
            return;

        switch (motion.Phase)
        {
            case MotionPhase.Idle:
                ApplyWeight(motion, motion.CurrentWeight);
                break;

            case MotionPhase.Attack:
                UpdateAttack(motion, dt);
                break;

            case MotionPhase.Hold:
                UpdateHold(motion, dt);
                break;

            case MotionPhase.Release:
                UpdateRelease(motion, dt);
                break;
        }
    }

    private void UpdateAttack(BlendShapeMotion motion, float dt)
    {
        float duration = Mathf.Max(0.001f, attackDuration);

        motion.Timer += dt;

        float t = Mathf.Clamp01(motion.Timer / duration);
        float eased = Mathf.Clamp01(attackCurve.Evaluate(t));

        float weight = Mathf.LerpUnclamped(motion.StartWeight, motion.PeakWeight, eased);

        ApplyWeight(motion, weight);

        if (t >= 1f)
        {
            motion.Timer = 0f;
            motion.ReleaseStartWeight = motion.CurrentWeight;
            motion.Phase = MotionPhase.Hold;
        }
    }

    private void UpdateHold(BlendShapeMotion motion, float dt)
    {
        ApplyWeight(motion, motion.PeakWeight);

        motion.HoldTimer -= dt;

        if (motion.HoldTimer <= 0f)
        {
            motion.Timer = 0f;
            motion.ReleaseStartWeight = motion.CurrentWeight;
            motion.Phase = MotionPhase.Release;
        }
    }

    private void UpdateRelease(BlendShapeMotion motion, float dt)
    {
        float duration = Mathf.Max(0.001f, releaseDuration);

        motion.Timer += dt;

        float t = Mathf.Clamp01(motion.Timer / duration);
        float eased = releaseCurve.Evaluate(t);

        float weight = Mathf.LerpUnclamped(0f, motion.ReleaseStartWeight, eased);

        ApplyWeight(motion, weight);

        if (t >= 1f)
        {
            ApplyWeight(motion, 0f);

            motion.Timer = 0f;
            motion.HoldTimer = 0f;
            motion.StartWeight = 0f;
            motion.PeakWeight = 0f;
            motion.ReleaseStartWeight = 0f;
            motion.Phase = MotionPhase.Idle;
        }
    }

    private BlendShapeMotion GetMotion(NetHitArea area)
    {
        switch (area)
        {
            case NetHitArea.Left:
                return _left;

            case NetHitArea.Right:
                return _right;

            case NetHitArea.Center:
                return _center;

            default:
                return _center;
        }
    }

    private void StopAllExcept(NetHitArea area)
    {
        if (area != NetHitArea.Left)
            BeginReleaseImmediately(_left);

        if (area != NetHitArea.Right)
            BeginReleaseImmediately(_right);

        if (area != NetHitArea.Center)
            BeginReleaseImmediately(_center);
    }

    private void BeginReleaseImmediately(BlendShapeMotion motion)
    {
        if (motion == null)
            return;

        motion.Timer = 0f;
        motion.HoldTimer = 0f;
        motion.ReleaseStartWeight = motion.CurrentWeight;
        motion.Phase = MotionPhase.Release;
    }

    private void ForceWeight(BlendShapeMotion motion, float weight)
    {
        if (motion == null)
            return;

        motion.CurrentWeight = SanitizeWeight(weight);
        motion.StartWeight = motion.CurrentWeight;
        motion.PeakWeight = motion.CurrentWeight;
        motion.ReleaseStartWeight = motion.CurrentWeight;
        motion.Timer = 0f;
        motion.HoldTimer = 0f;
        motion.Phase = MotionPhase.Idle;

        if (netRenderer && IsValidBlendShapeIndex(motion.Index))
            netRenderer.SetBlendShapeWeight(motion.Index, motion.CurrentWeight);
    }

    private void ApplyWeight(BlendShapeMotion motion, float weight)
    {
        motion.CurrentWeight = SanitizeWeight(weight);
        netRenderer.SetBlendShapeWeight(motion.Index, motion.CurrentWeight);
    }

    private float SanitizeWeight(float value)
    {
        if (clampToHundred)
            return Mathf.Clamp(value, 0f, 100f);

        return Mathf.Max(0f, value);
    }

    private bool IsValidBlendShapeIndex(int index)
    {
        if (!netRenderer || !netRenderer.sharedMesh)
            return false;

        return index >= 0 && index < netRenderer.sharedMesh.blendShapeCount;
    }

    private class BlendShapeMotion
    {
        public int Index;

        public float CurrentWeight;

        public float StartWeight;
        public float PeakWeight;
        public float ReleaseStartWeight;

        public float Timer;
        public float HoldTimer;

        public MotionPhase Phase;

        public BlendShapeMotion(int index)
        {
            Index = index;
            Phase = MotionPhase.Idle;
        }
    }
}