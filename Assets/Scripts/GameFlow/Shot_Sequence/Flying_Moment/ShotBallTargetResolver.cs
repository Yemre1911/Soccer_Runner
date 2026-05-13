using UnityEngine;

public class ShotBallTargetResolver : MonoBehaviour
{
    [Header("Invalid / Bad Shot Random Targets")]
    [Tooltip("Invalid / timeout / zigzag / kısa swipe gibi durumlarda top bu noktalardan rastgele birine gider.")]
    [SerializeField] private Transform[] invalidOutTargets = new Transform[5];

    [Header("Real Miss Continuation")]
    [Tooltip("Top gerçek auta gidiyorsa, hesaplanan son noktadan sonra kaç unit daha devam etsin?")]
    [SerializeField] private float missContinueDistance = 18f;

    [Tooltip("GroundedOut gibi yerde giden autlarda devam mesafesi.")]
    [SerializeField] private float groundedMissContinueDistance = 14f;

    [Header("Raw Fallback")]
    [Tooltip("Invalid targetlar atanmadıysa son çare olarak ileri kaç unit gitsin?")]
    [SerializeField] private float fallbackForwardDistance = 20f;

    [Tooltip("Fallback hedef yüksekliği.")]
    [SerializeField] private float fallbackHeight = 1.2f;

    [Header("Height Safety")]
    [Tooltip("Eğer gerçek aut devam hedefi çok aşağıdaysa minimum bu yüksekliğe çekilir.")]
    [SerializeField] private float minimumFlightTargetY = 0.35f;

    [Header("Debug")]
    [SerializeField] private bool logResolvedTarget = true;

    public Vector3 ResolveTarget(ShotDecisionResult decision, Vector3 startPoint, Transform directionReference)
    {
        if (decision == null)
        {
            Vector3 nullTarget = ResolveRandomInvalidOutOrFallback(startPoint, directionReference);
            Log(null, nullTarget, "Decision null -> random invalid out");
            return nullTarget;
        }

        Vector3 resolvedTarget;

        switch (decision.Code)
        {
            // ------------------------------------------------------------
            // GOAL / SAVE
            // Bunlarda mevcut hesaplanan hedef doğru.
            // ------------------------------------------------------------

            case ShotDecisionCode.GoalDirect:
            case ShotDecisionCode.GoalWideKeeperCurveBeat:

            case ShotDecisionCode.SavedByKeeperFront:
            case ShotDecisionCode.SavedByKeeperWideLowCurve:
            case ShotDecisionCode.SavedGroundedGoalLane:
            case ShotDecisionCode.SavedGroundedKeeperLane:
            case ShotDecisionCode.SavedByKeeperUpper:
                resolvedTarget = ResolveDecisionWorldPointOrFallback(decision, startPoint, directionReference);
                Log(decision, resolvedTarget, "Goal/Save -> decision target");
                return resolvedTarget;

            // ------------------------------------------------------------
            // REAL MISS / REAL OUT
            // Doğru çalışan kısım burası.
            // Referans target kullanılmaz.
            // Top hesaplanan world point yönünde devam eder.
            // ------------------------------------------------------------

            case ShotDecisionCode.MissedNoTarget:
                resolvedTarget = ResolveContinuedMissTarget(
                    decision,
                    startPoint,
                    directionReference,
                    missContinueDistance
                );

                Log(decision, resolvedTarget, "MissedNoTarget -> continue through target");
                return resolvedTarget;

            case ShotDecisionCode.GroundedOut:
                resolvedTarget = ResolveContinuedMissTarget(
                    decision,
                    startPoint,
                    directionReference,
                    groundedMissContinueDistance
                );

                Log(decision, resolvedTarget, "GroundedOut -> continue through grounded target");
                return resolvedTarget;

            // ------------------------------------------------------------
            // INVALID / BAD INPUT
            // Bunlarda swipe son noktasına güvenmiyoruz.
            // Top 5 invalid targettan rastgele birine gider.
            // ------------------------------------------------------------

            case ShotDecisionCode.InvalidSwipe:
            case ShotDecisionCode.Timeout:
            case ShotDecisionCode.ZigZagOut:
            case ShotDecisionCode.OverCurveOut:
            case ShotDecisionCode.MalformedOut:
                resolvedTarget = ResolveRandomInvalidOutOrFallback(startPoint, directionReference);
                Log(decision, resolvedTarget, "Invalid/Bad input -> random invalid target");
                return resolvedTarget;

            case ShotDecisionCode.None:
            default:
                resolvedTarget = ResolveRandomInvalidOutOrFallback(startPoint, directionReference);
                Log(decision, resolvedTarget, "Unknown code -> random invalid target");
                return resolvedTarget;
        }
    }

    private Vector3 ResolveDecisionWorldPointOrFallback(
        ShotDecisionResult decision,
        Vector3 startPoint,
        Transform directionReference
    )
    {
        if (HasUsableWorldPoint(decision))
            return decision.Target.WorldPoint;

        return ResolveRandomInvalidOutOrFallback(startPoint, directionReference);
    }

    private Vector3 ResolveContinuedMissTarget(
        ShotDecisionResult decision,
        Vector3 startPoint,
        Transform directionReference,
        float continueDistance
    )
    {
        if (!HasUsableWorldPoint(decision))
            return ResolveRandomInvalidOutOrFallback(startPoint, directionReference);

        Vector3 passPoint = decision.Target.WorldPoint;

        Vector3 direction = passPoint - startPoint;

        if (direction.sqrMagnitude < 0.001f)
            direction = ResolveForward(directionReference);

        direction.Normalize();

        Vector3 continuedTarget = passPoint + direction * Mathf.Max(0f, continueDistance);

        if (continuedTarget.y < minimumFlightTargetY)
            continuedTarget.y = minimumFlightTargetY;

        return continuedTarget;
    }

    private Vector3 ResolveRandomInvalidOutOrFallback(Vector3 startPoint, Transform directionReference)
    {
        if (TryGetRandomInvalidTarget(out Vector3 randomTarget))
            return randomTarget;

        Debug.LogWarning(
            "[ShotBallTargetResolver] InvalidOutTargets missing or empty. Using raw forward fallback."
        );

        Vector3 forward = ResolveForward(directionReference);

        Vector3 fallbackTarget = startPoint + forward * Mathf.Max(1f, fallbackForwardDistance);
        fallbackTarget.y = Mathf.Max(fallbackTarget.y, fallbackHeight);

        return fallbackTarget;
    }

    private bool TryGetRandomInvalidTarget(out Vector3 position)
    {
        position = Vector3.zero;

        if (invalidOutTargets == null || invalidOutTargets.Length == 0)
            return false;

        int validCount = 0;

        for (int i = 0; i < invalidOutTargets.Length; i++)
        {
            if (invalidOutTargets[i] != null)
                validCount++;
        }

        if (validCount == 0)
            return false;

        int randomValidIndex = Random.Range(0, validCount);
        int currentValidIndex = 0;

        for (int i = 0; i < invalidOutTargets.Length; i++)
        {
            Transform target = invalidOutTargets[i];

            if (target == null)
                continue;

            if (currentValidIndex == randomValidIndex)
            {
                position = target.position;
                return true;
            }

            currentValidIndex++;
        }

        return false;
    }

    private Vector3 ResolveForward(Transform directionReference)
    {
        Vector3 forward = directionReference ? directionReference.forward : transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;

        return forward.normalized;
    }

    private bool HasUsableWorldPoint(ShotDecisionResult decision)
    {
        if (decision == null)
            return false;

        if (decision.Target == null)
            return false;

        return decision.Target.WorldPoint.sqrMagnitude > 0.001f;
    }

    private void Log(ShotDecisionResult decision, Vector3 resolvedTarget, string reason)
    {
        if (!logResolvedTarget)
            return;

        Debug.Log(
            $"[ShotBallTargetResolver] Code: {decision?.Code}, " +
            $"Outcome: {decision?.Outcome}, " +
            $"ResolvedTarget: {resolvedTarget}, " +
            $"Reason: {reason}"
        );
    }
}