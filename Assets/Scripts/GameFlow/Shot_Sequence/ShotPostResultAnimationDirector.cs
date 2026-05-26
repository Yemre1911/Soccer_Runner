using UnityEngine;

public class ShotPostResultAnimationDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAnimationController playerAnimation;
    [SerializeField] private KeeperShotPostAnimationController keeperPostAnimation;

    [Header("Goal Keeper Action Rules")]
    [Tooltip("GoalDirect sonucunda kaleci dive/miss animasyonu oynuyorsa açık kalsın.")]
    [SerializeField] private bool goalDirectHasKeeperAction = true;

    [Tooltip("GoalWideKeeperCurveBeat sonucunda kaleci dive/miss animasyonu oynuyorsa açık kalsın.")]
    [SerializeField] private bool goalWideKeeperCurveBeatHasKeeperAction = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private bool _alreadyPlayed;

    public void PlayPostResult(ShotDecisionResult decision)
    {
        if (_alreadyPlayed)
            return;

        _alreadyPlayed = true;

        if (decision == null)
        {
            PlayFailSafe();
            return;
        }

        ShotDecisionCode code = decision.Code;

        bool isGoal = IsGoal(code);
        bool isSave = IsKeeperSave(code);
        bool hasKeeperAction = HasKeeperAction(code);

        PlayPlayerPostResult(isGoal);
        PlayKeeperPostResult(isGoal, isSave, hasKeeperAction);

        if (debugLogs)
        {
            Debug.Log(
                $"[ShotPostResultAnimationDirector] Result played. " +
                $"Code={code}, Goal={isGoal}, Save={isSave}, KeeperAction={hasKeeperAction}"
            );
        }
    }

    public void ResetForNewShot()
    {
        _alreadyPlayed = false;
    }

    private void PlayPlayerPostResult(bool isGoal)
    {
        if (!playerAnimation)
            return;

        if (isGoal)
            playerAnimation.PlayShotVictoryRandom();
        else
            playerAnimation.PlayShotDefeat();
    }

    private void PlayKeeperPostResult(bool isGoal, bool isSave, bool hasKeeperAction)
    {
        if (!keeperPostAnimation)
            return;

        // Top auta gittiyse, invalid ise, keeper animasyonu yoksa:
        // Keeper hemen sevinsin.
        if (!hasKeeperAction)
        {
            keeperPostAnimation.PlayVictoryNow();
            return;
        }

        // Keeper aksiyonu var ve kurtardıysa:
        // Save animasyonu bitince victory.
        if (isSave)
        {
            keeperPostAnimation.QueuePostResultAfterKeeperAction(saved: true);
            return;
        }

        // Keeper aksiyonu var ve gol yediyse:
        // Dive/miss animasyonu bitince defeat.
        if (isGoal)
        {
            keeperPostAnimation.QueuePostResultAfterKeeperAction(saved: false);
            return;
        }

        // Güvenli fallback.
        keeperPostAnimation.PlayVictoryNow();
    }

    private void PlayFailSafe()
    {
        if (playerAnimation)
            playerAnimation.PlayShotDefeat();

        if (keeperPostAnimation)
            keeperPostAnimation.PlayVictoryNow();
    }

    private bool IsGoal(ShotDecisionCode code)
    {
        switch (code)
        {
            case ShotDecisionCode.GoalDirect:
            case ShotDecisionCode.GoalWideKeeperCurveBeat:
                return true;

            default:
                return false;
        }
    }

    private bool IsKeeperSave(ShotDecisionCode code)
    {
        switch (code)
        {
            case ShotDecisionCode.SavedByKeeperUpper:
            case ShotDecisionCode.SavedByKeeperFront:
            case ShotDecisionCode.SavedGroundedKeeperLane:
            case ShotDecisionCode.SavedByKeeperWideLowCurve:
            case ShotDecisionCode.SavedGroundedGoalLane:
                return true;

            default:
                return false;
        }
    }

    private bool HasKeeperAction(ShotDecisionCode code)
    {
        switch (code)
        {
            case ShotDecisionCode.SavedByKeeperUpper:
            case ShotDecisionCode.SavedByKeeperFront:
            case ShotDecisionCode.SavedGroundedKeeperLane:
            case ShotDecisionCode.SavedByKeeperWideLowCurve:
            case ShotDecisionCode.SavedGroundedGoalLane:
                return true;

            case ShotDecisionCode.GoalDirect:
                return goalDirectHasKeeperAction;

            case ShotDecisionCode.GoalWideKeeperCurveBeat:
                return goalWideKeeperCurveBeatHasKeeperAction;

            case ShotDecisionCode.MissedNoTarget:
            case ShotDecisionCode.GroundedOut:
            case ShotDecisionCode.InvalidSwipe:
            case ShotDecisionCode.Timeout:
            case ShotDecisionCode.OverCurveOut:
            case ShotDecisionCode.ZigZagOut:
            case ShotDecisionCode.MalformedOut:
            case ShotDecisionCode.None:
            default:
                return false;
        }
    }
}