using UnityEngine;

public class ShotResolver : MonoBehaviour
{
    [Header("Curve Decision")]
    [SerializeField] private float keeperWideGoalCurveMin = 0.16f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public ShotDecisionResult Resolve(ShotSwipeIntent intent, ShotGoalTarget target, Transform goalTransform)
    {
        ShotDecisionResult result = new ShotDecisionResult
        {
            Intent = intent,
            Target = target,
            Outcome = ShotOutcomeType.MissOut,
            Code = ShotDecisionCode.None,
            IsGoal = false,
            Reason = "Default miss"
        };

        if (intent == null)
        {
            result.Code = ShotDecisionCode.InvalidSwipe;
            result.Reason = "Intent is null";
            Log(result);
            return result;
        }

        if (!intent.IsValid)
        {
            result.Outcome = intent.EndedByTimeout ? ShotOutcomeType.TimeoutOut : ShotOutcomeType.MissOut;
            result.Code = intent.EndedByTimeout ? ShotDecisionCode.Timeout : ShotDecisionCode.InvalidSwipe;
            result.Reason = intent.EndedByTimeout ? "Timeout / incomplete swipe" : "Invalid swipe";
            Log(result);
            return result;
        }

        if (intent.MalformedPath)
        {
            result.Outcome = ShotOutcomeType.MissOut;
            result.Code = ShotDecisionCode.MalformedOut;
            result.Reason = "Malformed path / loopy swipe";
            Log(result);
            return result;
        }

        if (intent.Kind == ShotSwipeKind.OverCurve)
        {
            result.Outcome = ShotOutcomeType.MissWide;
            result.Code = ShotDecisionCode.OverCurveOut;
            result.Reason = "Curve amount too high";
            Log(result);
            return result;
        }

        if (intent.Kind == ShotSwipeKind.ZigZag)
        {
            result.Outcome = ShotOutcomeType.MissOut;
            result.Code = ShotDecisionCode.ZigZagOut;
            result.Reason = "Zigzag / unstable swipe";
            Log(result);
            return result;
        }

        if (target == null)
        {
            result.Outcome = ShotOutcomeType.MissOut;
            result.Code = ShotDecisionCode.MissedNoTarget;
            result.Reason = "Target data missing";
            Log(result);
            return result;
        }

        switch (target.InitialHitType)
        {
            case ShotHitType.KeeperFront:
                result.Outcome = ShotOutcomeType.Saved;
                result.Code = ShotDecisionCode.SavedByKeeperFront;
                result.Reason = "Front keeper collider: guaranteed save";
                Log(result);
                return result;

            case ShotHitType.KeeperWide:
                ResolveWideKeeper(intent, result);
                Log(result);
                return result;

            case ShotHitType.Ground:
                ResolveGroundedSwipe(target, result);
                Log(result);
                return result;

            case ShotHitType.Goal:
                result.IsGoal = true;
                result.Outcome = ShotOutcomeType.Goal;
                result.Code = ShotDecisionCode.GoalDirect;
                result.Reason = "Direct goal collider hit";
                Log(result);
                return result;

            default:
                result.Outcome = ShotOutcomeType.MissOut;
                result.Code = ShotDecisionCode.MissedNoTarget;
                result.Reason = "Did not hit keeper, ground, or goal";
                Log(result);
                return result;
        }
    }

    private void ResolveWideKeeper(ShotSwipeIntent intent, ShotDecisionResult result)
    {
        if (intent.CurveAbs01 >= keeperWideGoalCurveMin)
        {
            result.IsGoal = true;
            result.Outcome = ShotOutcomeType.Goal;
            result.Code = ShotDecisionCode.GoalWideKeeperCurveBeat;
            result.Reason = "Wide keeper collider but curve is sufficient: goal";
        }
        else
        {
            result.IsGoal = false;
            result.Outcome = ShotOutcomeType.Saved;
            result.Code = ShotDecisionCode.SavedByKeeperWideLowCurve;
            result.Reason = "Wide keeper collider and curve is too low: saved";
        }
    }

    private void ResolveGroundedSwipe(ShotGoalTarget target, ShotDecisionResult result)
    {
        switch (target.AutoExtendedResolvedHitType)
        {
            case ShotHitType.KeeperFront:
                result.IsGoal = false;
                result.Outcome = ShotOutcomeType.Saved;
                result.Code = ShotDecisionCode.SavedGroundedKeeperLane;
                result.Reason = "Grounded swipe auto-extended into front keeper: saved";
                return;

            case ShotHitType.KeeperWide:
                result.IsGoal = false;
                result.Outcome = ShotOutcomeType.Saved;
                result.Code = ShotDecisionCode.SavedGroundedKeeperLane;
                result.Reason = "Grounded swipe auto-extended into wide keeper lane: saved";
                return;

            case ShotHitType.Goal:
                result.IsGoal = false;
                result.Outcome = ShotOutcomeType.Saved;
                result.Code = ShotDecisionCode.SavedGroundedGoalLane;
                result.Reason = "Grounded swipe reached goal lane after extension, but grounded shots cannot score: saved";
                return;

            default:
                result.IsGoal = false;
                result.Outcome = ShotOutcomeType.MissOut;
                result.Code = ShotDecisionCode.GroundedOut;
                result.Reason = "Grounded swipe auto-extension did not reach keeper/goal lane: out";
                return;
        }
    }

    private void Log(ShotDecisionResult result)
    {
        if (!debugLogs)
            return;

        string kind = result.Intent != null ? result.Intent.Kind.ToString() : "NULL";
        string initialHit = result.Target != null ? result.Target.InitialHitType.ToString() : "NULL";
        string finalHit = result.Target != null ? result.Target.FinalHitType.ToString() : "NULL";
        string autoExtHit = result.Target != null ? result.Target.AutoExtendedResolvedHitType.ToString() : "NULL";

        Debug.Log(
            $"[ShotResolver] Outcome={result.Outcome} | Code={result.Code} | IsGoal={result.IsGoal} | " +
            $"Kind={kind} | InitialHit={initialHit} | FinalHit={finalHit} | AutoExtHit={autoExtHit} | " +
            $"Reason={result.Reason}");
    }
}