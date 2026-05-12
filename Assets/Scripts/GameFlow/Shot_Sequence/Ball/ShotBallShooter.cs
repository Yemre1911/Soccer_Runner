using UnityEngine;

public class ShotBallShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BallController ball;

    [Tooltip("Eğer decision içinde geçerli world target yoksa fallback olarak kullanılabilir.")]
    [SerializeField] private Transform fallbackTarget;

    [Header("Flight Duration")]
    [SerializeField] private float slowestDuration = 0.85f;
    [SerializeField] private float fastestDuration = 0.45f;

    [Header("Arc")]
    [SerializeField] private float minArcHeight = 1.2f;
    [SerializeField] private float maxArcHeight = 3.2f;

    [Header("Curve")]
    [SerializeField] private float maxCurveOffset = 2.2f;
    [SerializeField] private bool invertCurveDirection = true;

    [Header("Fallback Target")]
    [SerializeField] private float fallbackForwardDistance = 22f;
    [SerializeField] private float fallbackHeight = 1.5f;

    [Header("Spin")]
    [SerializeField] private float spinSpeed = 1500f;

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime = true;

    private void Awake()
    {
        if (!ball)
            ball = GetComponent<BallController>();
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

        float power01 = ResolvePower01(decision);

        float duration = Mathf.Lerp(slowestDuration, fastestDuration, power01);
        float arcHeight = Mathf.Lerp(maxArcHeight, minArcHeight, power01);

        float curveSigned01 = ResolveCurveSigned01(decision);
        if (invertCurveDirection)
            curveSigned01 *= -1f;

        float curveOffset = curveSigned01 * maxCurveOffset;

        Debug.Log(
    $"[ShotBallShooter] Curve Debug | " +
    $"Kind: {decision?.Intent?.Kind}, " +
    $"IsValid: {decision?.Intent?.IsValid}, " +
    $"IsCurveAcceptable: {decision?.Intent?.IsCurveAcceptable}, " +
    $"CurveSigned01: {decision?.Intent?.CurveSigned01}, " +
    $"CurveAbs01: {decision?.Intent?.CurveAbs01}, " +
    $"FinalCurveOffset: {curveOffset}"
);

        return new ShotBallFlightData
        {
            StartPoint = startPoint,
            TargetPoint = targetPoint,

            Duration = duration,
            ArcHeight = arcHeight,
            CurveOffset = curveOffset,
            SpinSpeed = spinSpeed,

            UseUnscaledTime = useUnscaledTime,

            Decision = decision
        };
    }

    private Vector3 ResolveTargetPoint(ShotDecisionResult decision, Vector3 startPoint)
    {
        if (HasUsableDecisionTarget(decision))
            return decision.Target.WorldPoint;

        if (fallbackTarget != null)
            return fallbackTarget.position;

        Vector3 fallbackDirection = transform.forward;
        fallbackDirection.y = 0f;

        if (fallbackDirection.sqrMagnitude < 0.001f)
            fallbackDirection = Vector3.forward;

        fallbackDirection.Normalize();

        return startPoint + fallbackDirection * fallbackForwardDistance + Vector3.up * fallbackHeight;
    }

    private bool HasUsableDecisionTarget(ShotDecisionResult decision)
    {
        if (decision == null)
            return false;

        if (decision.Target == null)
            return false;

        if (decision.Target.FinalHitType != ShotHitType.None)
            return true;

        if (decision.Target.InitialHitType != ShotHitType.None)
            return true;

        if (decision.Target.AutoExtendedResolvedHitType != ShotHitType.None)
            return true;

        if (decision.Target.InsideGoal)
            return true;

        // WorldPoint gerçekten set edilmiş mi diye son güvenlik.
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

        // Görsel curve güçlendirmesi.
        // 0.14 gibi küçük ama Curve sayılan swipe'lar ekranda belli olsun.
        float visualCurve = Mathf.Lerp(0.35f, 1f, absCurve);

        return sign * visualCurve;
    }
}