using UnityEngine;

public class ShotBallShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BallController ball;

    [Tooltip("Decision code'a göre topun gerçek uçuş hedefini çözer.")]
    [SerializeField] private ShotBallTargetResolver targetResolver;

    [Tooltip("Topun ileri/sağ yönünü çözmek için referans. Genelde player root veya shot camera pivot olabilir.")]
    [SerializeField] private Transform directionReference;

    [Header("Flight Duration")]
    [SerializeField] private float slowestDuration = 0.85f;
    [SerializeField] private float fastestDuration = 0.45f;

    [Header("Invalid / Weak Shot Duration")]
    [Tooltip("Kısa swipe, timeout, invalid gibi kötü şutlarda top biraz daha zayıf hissettirsin.")]
    [SerializeField] private float invalidShotDuration = 0.75f;

    [Header("Arc")]
    [SerializeField] private float minArcHeight = 1.2f;
    [SerializeField] private float maxArcHeight = 3.2f;

    [Header("Invalid / Weak Shot Arc")]
    [SerializeField] private float invalidShotArcHeight = 0.9f;

    [Header("Curve")]
    [SerializeField] private float maxCurveOffset = 2.2f;
    [SerializeField] private bool invertCurveDirection = true;

    [Header("Raw Fallback")]
    [Tooltip("Resolver yoksa veya hiçbir veri yoksa kullanılacak ileri mesafe.")]
    [SerializeField] private float rawFallbackForwardDistance = 22f;

    [SerializeField] private float rawFallbackHeight = 1.5f;

    [Header("Spin")]
    [SerializeField] private float spinSpeed = 1500f;
    [SerializeField] private float invalidShotSpinSpeed = 850f;

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private void Awake()
    {
        if (!ball)
            ball = GetComponent<BallController>();

        if (!directionReference)
            directionReference = transform;
    }

    public void Shoot(ShotDecisionResult decision)
    {
        if (ball == null)
        {
            Debug.LogWarning("[ShotBallShooter] Ball reference is missing.");
            return;
        }

        ShotBallFlightData flightData = BuildFlightData(decision);

        ball.EnterShotFlight(flightData);
    }

    private ShotBallFlightData BuildFlightData(ShotDecisionResult decision)
    {
        Vector3 startPoint = ball.transform.position;
        Vector3 targetPoint = ResolveTargetPoint(decision, startPoint);

        bool invalidOrBadShot = IsInvalidOrBadShot(decision);

        float power01 = ResolvePower01(decision);

        float duration = invalidOrBadShot
            ? invalidShotDuration
            : Mathf.Lerp(slowestDuration, fastestDuration, power01);

        float arcHeight = invalidOrBadShot
            ? invalidShotArcHeight
            : Mathf.Lerp(maxArcHeight, minArcHeight, power01);

        float curveSigned01 = ResolveCurveSigned01(decision);

        if (invertCurveDirection)
            curveSigned01 *= -1f;

        float curveOffset = invalidOrBadShot
            ? 0f
            : curveSigned01 * maxCurveOffset;

        float resolvedSpinSpeed = invalidOrBadShot
            ? invalidShotSpinSpeed
            : spinSpeed;

        if (logDebug)
        {
            Debug.Log(
                $"[ShotBallShooter] BuildFlightData | " +
                $"Code: {decision?.Code}, " +
                $"Outcome: {decision?.Outcome}, " +
                $"Start: {startPoint}, " +
                $"Target: {targetPoint}, " +
                $"Power01: {power01}, " +
                $"Duration: {duration}, " +
                $"Arc: {arcHeight}, " +
                $"CurveOffset: {curveOffset}, " +
                $"Spin: {resolvedSpinSpeed}"
            );

            Debug.Log(
                $"[ShotBallShooter] Curve Debug | " +
                $"Kind: {decision?.Intent?.Kind}, " +
                $"IsValid: {decision?.Intent?.IsValid}, " +
                $"IsCurveAcceptable: {decision?.Intent?.IsCurveAcceptable}, " +
                $"CurveSigned01: {decision?.Intent?.CurveSigned01}, " +
                $"CurveAbs01: {decision?.Intent?.CurveAbs01}, " +
                $"FinalCurveOffset: {curveOffset}"
            );
        }

        return new ShotBallFlightData
        {
            StartPoint = startPoint,
            TargetPoint = targetPoint,

            Duration = duration,
            ArcHeight = arcHeight,
            CurveOffset = curveOffset,
            SpinSpeed = resolvedSpinSpeed,

            UseUnscaledTime = useUnscaledTime,

            Decision = decision
        };
    }

    private Vector3 ResolveTargetPoint(ShotDecisionResult decision, Vector3 startPoint)
    {
        if (targetResolver != null)
            return targetResolver.ResolveTarget(decision, startPoint, directionReference);

        if (ShouldUseDecisionTargetWithoutResolver(decision))
            return decision.Target.WorldPoint;

        return ResolveRawFallbackTarget(startPoint);
    }

    private bool ShouldUseDecisionTargetWithoutResolver(ShotDecisionResult decision)
    {
        if (!HasUsableDecisionTarget(decision))
            return false;

        switch (decision.Code)
        {
            case ShotDecisionCode.GoalDirect:
            case ShotDecisionCode.GoalWideKeeperCurveBeat:

            case ShotDecisionCode.SavedByKeeperFront:
            case ShotDecisionCode.SavedByKeeperWideLowCurve:
            case ShotDecisionCode.SavedGroundedGoalLane:
            case ShotDecisionCode.SavedGroundedKeeperLane:
            case ShotDecisionCode.SavedByKeeperUpper:
                return true;

            default:
                return false;
        }
    }

    private Vector3 ResolveRawFallbackTarget(Vector3 startPoint)
    {
        Vector3 forward = directionReference ? directionReference.forward : transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        forward.Normalize();

        Vector3 target = startPoint + forward * rawFallbackForwardDistance;
        target.y = Mathf.Max(target.y, rawFallbackHeight);

        return target;
    }

    private bool HasUsableDecisionTarget(ShotDecisionResult decision)
    {
        if (decision == null)
            return false;

        if (decision.Target == null)
            return false;

        if (decision.Target.WorldPoint.sqrMagnitude > 0.001f)
            return true;

        return false;
    }

    private float ResolvePower01(ShotDecisionResult decision)
    {
        if (decision == null || decision.Intent == null)
            return 0.65f;

        return Mathf.Clamp01(decision.Intent.Power01);
    }

    private float ResolveCurveSigned01(ShotDecisionResult decision)
    {
        if (decision == null || decision.Intent == null)
            return 0f;

        ShotSwipeIntent intent = decision.Intent;

        if (!intent.IsValid)
            return 0f;

        if (intent.MalformedPath)
            return 0f;

        if (intent.Kind == ShotSwipeKind.ZigZag)
            return 0f;

        if (intent.Kind == ShotSwipeKind.OverCurve)
            return 0f;

        float rawCurve = Mathf.Clamp(intent.CurveSigned01, -1f, 1f);

        if (Mathf.Abs(rawCurve) < 0.001f)
            return 0f;

        float sign = Mathf.Sign(rawCurve);
        float absCurve = Mathf.Abs(rawCurve);

        float visualCurve = Mathf.Lerp(0.35f, 1f, absCurve);

        return sign * visualCurve;
    }

    private bool IsInvalidOrBadShot(ShotDecisionResult decision)
    {
        if (decision == null)
            return true;

        switch (decision.Code)
        {
            case ShotDecisionCode.InvalidSwipe:
            case ShotDecisionCode.Timeout:
            case ShotDecisionCode.OverCurveOut:
            case ShotDecisionCode.ZigZagOut:
            case ShotDecisionCode.MalformedOut:
                return true;

            default:
                return false;
        }
    }
}