using System.Collections;
using UnityEngine;

public class ShotKeeperController : MonoBehaviour
{
    [System.Serializable]
    private class KeeperReactionEntry
    {
        [Header("Decision")]
        public ShotDecisionCode code;

        [Header("Animator")]
        public string triggerName;

        [Tooltip("HighDive / GroundDive gibi sağ-sol aynalama isteyen animasyonlarda true olmalı.")]
        public bool mirrorByShotTargetX;

        [Tooltip("Mirror ters çalışırsa bunu aç.")]
        public bool invertMirror;

        [Header("Save Alignment")]
        [Tooltip("Save senaryolarında HoldSocket'i topun final target noktasına hizalar.")]
        public bool alignHoldSocketToShotTarget;

        [Tooltip("HoldSocket hizalaması kaç saniyede yapılsın? 0 verirsen anlık hizalar.")]
        [Min(0f)]
        public float alignDuration = 0.25f;

        [Tooltip("Hizalama bittikten sonra kaç saniye daha HoldSocket target üzerinde tutulacak.")]
        [Min(0f)]
        public float holdAfterAlignSeconds = 0.25f;

        public AnimationCurve alignCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Şut commit sırasında oyun/simulation durduğu için genelde true kalmalı.")]
        public bool useUnscaledTimeForAlign = true;

        [Header("Root Motion")]
        [Tooltip("Goal gibi kurtarılmayan senaryolarda animasyon kendi root motion'ını oynatsın mı?")]
        public bool useRootMotionForThisReaction;
        [Tooltip("Save hizalaması tamamlandıktan sonra root motion açılsın mı?")]
        public bool enableRootMotionAfterAlign;

        [Header("Lock")]
        public bool lockBasicMovement = true;

        [Tooltip("Reaction bittikten sonra basic movement tekrar açılsın mı? Şut sekansında genelde false kalmalı.")]
        public bool unlockBasicMovementAfterReaction = false;

        [Tooltip("Basic movement tekrar açılacaksa, kaç saniye sonra açılsın.")]
        [Min(0f)]
        public float unlockAfterSeconds = 1f;
    }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private KeeperBasicMovement basicMovement;

    [Tooltip("Kalecinin ana root/capsule transformu. KeeperBasicMovement hangi objeyi hareket ettiriyorsa bu da o olmalı.")]
    [SerializeField] private Transform keeperRoot;

    [Tooltip("Kalecinin topu yakalayacağı referans noktası. İlk test için keeperRoot altında empty child olabilir.")]
    [SerializeField] private Transform holdSocket;

    [Header("Animator Parameters")]
    [SerializeField] private string mirrorDiveParam = "MirrorDive";

    [Header("Reaction Entries")]
    [SerializeField] private KeeperReactionEntry[] reactions;

    [Header("Options")]
    [SerializeField] private bool stopPreviousReactionOnNewReact = true;
    [SerializeField] private bool defaultApplyRootMotion = false;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Coroutine _unlockRoutine;

    private bool _isAligning;
    private bool _isHoldingAfterAlign;

    private Vector3 _alignTargetWorld;
    private Vector3 _alignStartRootWorld;

    private float _alignElapsed;
    private float _holdElapsed;

    private KeeperReactionEntry _activeReaction;

    private void Awake()
    {
        if (!keeperRoot)
            keeperRoot = transform;

        if (!animator)
            animator = GetComponentInChildren<Animator>();

        if (!basicMovement)
            basicMovement = GetComponent<KeeperBasicMovement>();

        SetAnimatorRootMotion(defaultApplyRootMotion);
    }

    private void LateUpdate()
    {
        if (!_isAligning || _activeReaction == null)
            return;

        if (keeperRoot == null || holdSocket == null)
            return;

        float dt = _activeReaction.useUnscaledTimeForAlign
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        float duration = Mathf.Max(0f, _activeReaction.alignDuration);

        if (duration <= 0.0001f)
        {
            ForceHoldSocketToTarget();
            BeginHoldAfterAlignIfNeeded();
            return;
        }

        _alignElapsed += dt;

        float rawT = Mathf.Clamp01(_alignElapsed / duration);
        float curvedT = _activeReaction.alignCurve != null
            ? _activeReaction.alignCurve.Evaluate(rawT)
            : rawT;

        /*
         * Önemli:
         * Sadece X ekseninde hizalama yapıyoruz.
         * Y ve Z eksenlerine dokunmuyoruz.
         */
        Vector3 correction = _alignTargetWorld - holdSocket.position;
        Vector3 desiredRootWorld = keeperRoot.position + new Vector3(correction.x, 0f, 0f);

        keeperRoot.position = Vector3.Lerp(
            _alignStartRootWorld,
            desiredRootWorld,
            curvedT
        );

        if (rawT >= 1f)
        {
            ForceHoldSocketToTarget();
            BeginHoldAfterAlignIfNeeded();
        }
    }

    public void React(ShotDecisionResult decision)
    {
        if (decision == null)
        {
            if (debugLogs)
                Debug.LogWarning("[ShotKeeperController] Decision is null.");

            return;
        }

        KeeperReactionEntry reaction = FindReaction(decision.Code);

        if (reaction == null)
        {
            if (debugLogs)
                Debug.Log($"[ShotKeeperController] No keeper reaction for code: {decision.Code}");

            return;
        }

        if (stopPreviousReactionOnNewReact)
            StopCurrentReactionRuntime();

        _activeReaction = reaction;

        if (reaction.lockBasicMovement && basicMovement != null)
            basicMovement.SetLocked(true);

        SetAnimatorRootMotion(reaction.useRootMotionForThisReaction);

        bool targetIsLeft = IsShotTargetLeftOfKeeper(decision);

        ApplyMirror(reaction, targetIsLeft);
        PlayTrigger(reaction);

        if (reaction.alignHoldSocketToShotTarget)
        {
            BeginHoldSocketAlignment(decision, reaction);
        }

        if (reaction.unlockBasicMovementAfterReaction && basicMovement != null)
        {
            _unlockRoutine = StartCoroutine(UnlockBasicMovementRoutine(reaction));
        }
    }

    public void ResetKeeperReaction()
    {
        StopCurrentReactionRuntime();

        if (basicMovement != null)
            basicMovement.SetLocked(false);

        if (animator != null)
        {
            animator.SetBool(mirrorDiveParam, false);
            SetAnimatorRootMotion(defaultApplyRootMotion);
        }
    }

    private void StopCurrentReactionRuntime()
    {
        _isAligning = false;
        _isHoldingAfterAlign = false;
        _activeReaction = null;

        _alignElapsed = 0f;
        _holdElapsed = 0f;

        if (_unlockRoutine != null)
        {
            StopCoroutine(_unlockRoutine);
            _unlockRoutine = null;
        }
    }

    private void BeginHoldSocketAlignment(
        ShotDecisionResult decision,
        KeeperReactionEntry reaction)
    {
        if (keeperRoot == null)
        {
            Debug.LogWarning("[ShotKeeperController] KeeperRoot is missing.");
            return;
        }

        if (holdSocket == null)
        {
            Debug.LogWarning("[ShotKeeperController] HoldSocket is missing. Cannot align keeper.");
            return;
        }

        if (decision == null || decision.Target == null)
        {
            Debug.LogWarning("[ShotKeeperController] Decision target is missing.");
            return;
        }

        _alignTargetWorld = decision.Target.WorldPoint;
        _alignStartRootWorld = keeperRoot.position;

        _alignElapsed = 0f;
        _holdElapsed = 0f;

        _isAligning = true;
        _isHoldingAfterAlign = false;

        if (debugLogs)
        {
            Debug.Log(
                $"[ShotKeeperController] Begin alignment | " +
                $"Code={reaction.code} | " +
                $"RootStart={_alignStartRootWorld} | " +
                $"HoldSocketStart={holdSocket.position} | " +
                $"TargetWorld={_alignTargetWorld} | " +
                $"Distance={Vector3.Distance(holdSocket.position, _alignTargetWorld):F3} | " +
                $"Duration={reaction.alignDuration:F3}"
            );
        }
    }

    private void ForceHoldSocketToTarget()
    {
        if (keeperRoot == null || holdSocket == null)
            return;

        Vector3 correction = _alignTargetWorld - holdSocket.position;

        // Sadece world X ekseninde düzeltme yap.
        Vector3 xOnlyCorrection = new Vector3(correction.x, 0f, 0f);

        keeperRoot.position += xOnlyCorrection;

        if (debugLogs)
        {
            Debug.Log(
                $"[ShotKeeperController] Force socket X correction | " +
                $"FullCorrection={correction} | " +
                $"XOnlyCorrection={xOnlyCorrection} | " +
                $"Root={keeperRoot.position} | " +
                $"HoldSocket={holdSocket.position} | " +
                $"Target={_alignTargetWorld} | " +
                $"XDistanceAfter={Mathf.Abs(holdSocket.position.x - _alignTargetWorld.x):F4}"
            );
        }
    }

    private void BeginHoldAfterAlignIfNeeded()
    {
        if (_activeReaction == null)
        {
            _isAligning = false;
            _isHoldingAfterAlign = false;
            return;
        }

        if (_activeReaction.enableRootMotionAfterAlign)
        {
            SetAnimatorRootMotion(true);

            if (debugLogs)
                Debug.Log("[ShotKeeperController] Root motion enabled after save alignment.");
        }

        if (_activeReaction.holdAfterAlignSeconds <= 0f)
        {
            _isAligning = false;
            _isHoldingAfterAlign = false;
            return;
        }

        _isHoldingAfterAlign = true;

        float dt = _activeReaction.useUnscaledTimeForAlign
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        _holdElapsed += dt;

        ForceHoldSocketToTarget();

        if (_holdElapsed >= _activeReaction.holdAfterAlignSeconds)
        {
            _isAligning = false;
            _isHoldingAfterAlign = false;

            if (debugLogs)
                Debug.Log("[ShotKeeperController] Alignment finished.");
        }
    }

    private IEnumerator UnlockBasicMovementRoutine(KeeperReactionEntry reaction)
    {
        float elapsed = 0f;

        while (elapsed < reaction.unlockAfterSeconds)
        {
            elapsed += reaction.useUnscaledTimeForAlign
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            yield return null;
        }

        if (basicMovement != null)
            basicMovement.SetLocked(false);

        SetAnimatorRootMotion(defaultApplyRootMotion);

        _unlockRoutine = null;
    }

    private void ApplyMirror(KeeperReactionEntry reaction, bool targetIsLeft)
    {
        if (animator == null)
            return;

        if (!reaction.mirrorByShotTargetX)
        {
            animator.SetBool(mirrorDiveParam, false);
            return;
        }

        bool mirror = reaction.invertMirror ? !targetIsLeft : targetIsLeft;

        animator.SetBool(mirrorDiveParam, mirror);

        if (debugLogs)
        {
            Debug.Log(
                $"[ShotKeeperController] Mirror applied | " +
                $"Code={reaction.code} | " +
                $"TargetIsLeft={targetIsLeft} | " +
                $"Invert={reaction.invertMirror} | " +
                $"Mirror={mirror}"
            );
        }
    }

    private bool IsShotTargetLeftOfKeeper(ShotDecisionResult decision)
    {
        if (decision == null || decision.Target == null)
            return false;

        Vector3 targetWorld = decision.Target.WorldPoint;
        Vector3 keeperWorld = keeperRoot != null ? keeperRoot.position : transform.position;

        return targetWorld.x < keeperWorld.x;
    }

    private void PlayTrigger(KeeperReactionEntry reaction)
    {
        if (animator == null)
            return;

        if (string.IsNullOrWhiteSpace(reaction.triggerName))
        {
            if (debugLogs)
                Debug.LogWarning($"[ShotKeeperController] Empty trigger for code: {reaction.code}");

            return;
        }

        animator.ResetTrigger(reaction.triggerName);
        animator.SetTrigger(reaction.triggerName);

        if (debugLogs)
            Debug.Log($"[ShotKeeperController] Trigger played: {reaction.triggerName}");
    }

    private void SetAnimatorRootMotion(bool enabled)
    {
        if (animator == null)
            return;

        animator.applyRootMotion = enabled;

        if (debugLogs)
            Debug.Log($"[ShotKeeperController] Animator applyRootMotion = {enabled}");
    }

    private KeeperReactionEntry FindReaction(ShotDecisionCode code)
    {
        if (reactions == null)
            return null;

        for (int i = 0; i < reactions.Length; i++)
        {
            if (reactions[i] != null && reactions[i].code == code)
                return reactions[i];
        }

        return null;
    }
}