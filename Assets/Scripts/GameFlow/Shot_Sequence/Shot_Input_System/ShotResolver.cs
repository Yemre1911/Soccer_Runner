using UnityEngine;

public class ShotResolver : MonoBehaviour
{
    [Header("Curve Decision")]
    [SerializeField] private float keeperWideGoalCurveMin = 0.16f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

  
    public ShotDecisionResult Resolve(
    ShotSwipeIntent intent,
    ShotGoalTarget target,
    Transform goalTransform)
    {
        ShotDecisionResult result = new ShotDecisionResult
        {
            Intent = intent,
            Target = target,
            IsGoal = false,
            Outcome = ShotOutcomeType.MissOut,
            Code = ShotDecisionCode.None,
            Reason = "Unresolved"
        };

        if (intent == null || !intent.IsValid)
        {
            result.Outcome = ShotOutcomeType.TimeoutOut;
            result.Code = intent != null && intent.EndedByTimeout
                ? ShotDecisionCode.Timeout
                : ShotDecisionCode.InvalidSwipe;
            result.Reason = "Invalid swipe";
            Log(result);
            return result;
        }

        if (intent.MalformedPath)
        {
            result.Outcome = ShotOutcomeType.MissOut;
            result.Code = ShotDecisionCode.MalformedOut;
            result.Reason = "Malformed swipe path";
            Log(result);
            return result;
        }

        if (intent.Kind == ShotSwipeKind.ZigZag)
        {
            result.Outcome = ShotOutcomeType.MissOut;
            result.Code = ShotDecisionCode.ZigZagOut;
            result.Reason = "Zigzag swipe";
            Log(result);
            return result;
        }

        if (intent.Kind == ShotSwipeKind.OverCurve)
        {
            result.Outcome = ShotOutcomeType.MissOut;
            result.Code = ShotDecisionCode.OverCurveOut;
            result.Reason = "Over curve swipe";
            Log(result);
            return result;
        }

        /*
         * KRİTİK FIX:
         * Ground başlayan swipe'ları InsideGoal kontrolünden önce çöz.
         * Çünkü ground swipe'larda final projected point dışarıda olabilir,
         * ama auto-extension keeper/goal collider'a ulaşmış olabilir.
         */
        if (target != null && target.InitialHitType == ShotHitType.Ground)
        {
            ResolveGroundedSwipe(target, result);
            Log(result);
            return result;
        }

        if (target == null || !target.InsideGoal)
        {
            result.IsGoal = false;
            result.Outcome = ShotOutcomeType.MissOut;
            result.Code = ShotDecisionCode.MissedNoTarget;
            result.Reason = "Shot target is outside fixed goal bounds: out";
            Log(result);
            return result;
        }

        switch (target.InitialHitType)
        {
            case ShotHitType.KeeperUpper:
                result.IsGoal = false;
                result.Outcome = ShotOutcomeType.Saved;
                result.Code = ShotDecisionCode.SavedByKeeperUpper;
                result.Reason = "Upper keeper collider inside goal bounds: jump catch save";
                Log(result);
                return result;

            case ShotHitType.KeeperFront:
                result.IsGoal = false;
                result.Outcome = ShotOutcomeType.Saved;
                result.Code = ShotDecisionCode.SavedByKeeperFront;
                result.Reason = "Front keeper collider inside goal bounds: guaranteed save";
                Log(result);
                return result;

            case ShotHitType.KeeperWide:
                ResolveWideKeeper(intent, target, result);
                Log(result);
                return result;

            case ShotHitType.Goal:
                result.IsGoal = true;
                result.Outcome = ShotOutcomeType.Goal;
                result.Code = ShotDecisionCode.GoalDirect;
                result.Reason = "Direct goal collider hit inside fixed goal bounds";
                Log(result);
                return result;

            default:
                result.IsGoal = false;
                result.Outcome = ShotOutcomeType.MissOut;
                result.Code = ShotDecisionCode.MissedNoTarget;
                result.Reason = "Inside goal bounds but did not hit keeper or goal";
                Log(result);
                return result;
        }
    }

    private void ResolveOutsideGoal(ShotGoalTarget target, ShotDecisionResult result)
    {
        result.IsGoal = false;

        if (target.InitialHitType == ShotHitType.Ground)
        {
            result.Outcome = ShotOutcomeType.MissOut;
            result.Code = ShotDecisionCode.GroundedOut;
            result.Reason = "Swipe started on ground and final projected point is outside fixed goal bounds: out";
            return;
        }

        float normalizedX = target.Normalized.x;
        float normalizedY = target.Normalized.y;

        bool isWide = Mathf.Abs(normalizedX) > 1f;
        bool isHigh = normalizedY > 1f;

        if (isWide)
        {
            result.Outcome = ShotOutcomeType.MissWide;
            result.Code = ShotDecisionCode.MissedNoTarget;
            result.Reason =
                $"Final projected point is outside fixed goal bounds on X. " +
                $"Keeper hit ignored for goal decision. Normalized={target.Normalized}";
            return;
        }

        if (isHigh)
        {
            result.Outcome = ShotOutcomeType.MissOut;
            result.Code = ShotDecisionCode.MissedNoTarget;
            result.Reason =
                $"Final projected point is above fixed goal bounds. " +
                $"Keeper hit ignored for goal decision. Normalized={target.Normalized}";
            return;
        }

        result.Outcome = ShotOutcomeType.MissOut;
        result.Code = ShotDecisionCode.MissedNoTarget;
        result.Reason =
            $"Final projected point is outside fixed goal bounds. " +
            $"Keeper hit ignored for goal decision. Normalized={target.Normalized}";
    }

    private void ResolveWideKeeper(ShotSwipeIntent intent, ShotGoalTarget target, ShotDecisionResult result)
    {
        if (!target.InsideGoal)
        {
            result.IsGoal = false;
            result.Outcome = ShotOutcomeType.MissWide;
            result.Code = ShotDecisionCode.MissedNoTarget;
            result.Reason = "Wide keeper collider hit, but final projected point is outside fixed goal bounds: no goal";
            return;
        }

        if (intent.CurveAbs01 >= keeperWideGoalCurveMin)
        {
            result.IsGoal = true;
            result.Outcome = ShotOutcomeType.Goal;
            result.Code = ShotDecisionCode.GoalWideKeeperCurveBeat;
            result.Reason = "Wide keeper collider inside goal bounds and curve is sufficient: goal";
        }
        else
        {
            result.IsGoal = false;
            result.Outcome = ShotOutcomeType.Saved;
            result.Code = ShotDecisionCode.SavedByKeeperWideLowCurve;
            result.Reason = "Wide keeper collider inside goal bounds but curve is too low: saved";
        }
    }


    private void ResolveGroundedSwipe(ShotGoalTarget target, ShotDecisionResult result)
    {
        switch (target.AutoExtendedResolvedHitType)
        {
            case ShotHitType.KeeperUpper:
                result.IsGoal = false;
                result.Outcome = ShotOutcomeType.Saved;
                result.Code = ShotDecisionCode.SavedByKeeperUpper;
                result.Reason = "Grounded swipe auto-extended into upper keeper lane: jump catch save";
                return;

            case ShotHitType.KeeperFront:
                result.IsGoal = false;
                result.Outcome = ShotOutcomeType.Saved;
                result.Code = ShotDecisionCode.SavedGroundedKeeperLane;
                result.Reason = "Grounded swipe auto-extended into front keeper lane: ground catch save";
                return;

            case ShotHitType.KeeperWide:
                result.IsGoal = false;
                result.Outcome = ShotOutcomeType.Saved;
                result.Code = ShotDecisionCode.SavedGroundedKeeperLane;
                result.Reason = "Grounded swipe auto-extended into wide keeper lane: ground catch save";
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
        string insideGoal = result.Target != null ? result.Target.InsideGoal.ToString() : "NULL";
        string normalized = result.Target != null ? result.Target.Normalized.ToString() : "NULL";

        Debug.Log(
            $"[ShotResolver] Outcome={result.Outcome} | Code={result.Code} | IsGoal={result.IsGoal} | " +
            $"Kind={kind} | InitialHit={initialHit} | FinalHit={finalHit} | AutoExtHit={autoExtHit} | " +
            $"InsideGoal={insideGoal} | Normalized={normalized} | Reason={result.Reason}");
    }
}