using System.Collections;
using UnityEngine;

public class ShotPostResultAnimationDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAnimationController playerAnimation;
    [SerializeField] private KeeperShotPostAnimationController keeperPostAnimation;

    [Header("Camera")]
    [SerializeField] private ShotPostResultCameraDirector cameraDirector;

    [Header("Player Goal Turn")]
    [Tooltip("Gol sonrası kendi etrafında döndürülecek karakter/root transform.")]
    [SerializeField] private Transform playerRootToRotate;

    [Tooltip("Gol sonrası karakter kaç derece dönecek? 180 önerilir.")]
    [SerializeField] private float goalTurnDegrees = 180f;

    [Tooltip("Gol sonrası dönüş kaç saniyede tamamlanacak?")]
    [SerializeField] private float goalTurnDuration = 1.25f;

    [Tooltip("Dönüş local Y ekseninde mi yapılsın? Açık kalması önerilir.")]
    [SerializeField] private bool useLocalRotation = true;

    [Tooltip("Dönüş Time.timeScale'den etkilenmesin mi? Shot/result akışında açık kalması önerilir.")]
    [SerializeField] private bool useUnscaledTimeForTurn = true;

    [Tooltip("Yeni şut başlarken karakter rotasyonu başlangıç haline dönsün mü?")]
    [SerializeField] private bool resetRotationOnNewShot = true;

    [Tooltip("Dönüş easing eğrisi. Başta/sonda yumuşak olması için EaseInOut önerilir.")]
    [SerializeField] private AnimationCurve goalTurnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Goal Keeper Action Rules")]
    [Tooltip("GoalDirect sonucunda kaleci dive/miss animasyonu oynuyorsa açık kalsın.")]
    [SerializeField] private bool goalDirectHasKeeperAction = true;

    [Tooltip("GoalWideKeeperCurveBeat sonucunda kaleci dive/miss animasyonu oynuyorsa açık kalsın.")]
    [SerializeField] private bool goalWideKeeperCurveBeatHasKeeperAction = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private bool _alreadyPlayed;
    private Coroutine _playerGoalTurnRoutine;

    private Quaternion _initialPlayerLocalRotation;
    private Quaternion _initialPlayerWorldRotation;
    private bool _hasInitialPlayerRotation;

    private void Awake()
    {
        CacheInitialPlayerRotation();
    }

    public void PlayPostResult(ShotDecisionResult decision)
    {
        if (_alreadyPlayed)
            return;

        _alreadyPlayed = true;

        if (decision == null)
        {
            PlayFailSafe();
            PlayCameraPostResult(null);
            return;
        }

        ShotDecisionCode code = decision.Code;

        bool isGoal = IsGoal(code);
        bool isSave = IsKeeperSave(code);
        bool hasKeeperAction = HasKeeperAction(code);

        PlayPlayerPostResult(isGoal);
        PlayKeeperPostResult(isGoal, isSave, hasKeeperAction);
        PlayCameraPostResult(decision);

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

        StopPlayerGoalTurn();

        if (resetRotationOnNewShot)
            RestoreInitialPlayerRotation();

        if (cameraDirector)
            cameraDirector.ResetForNewShot();
    }

    private void PlayPlayerPostResult(bool isGoal)
    {
        if (!playerAnimation)
            return;

        if (isGoal)
        {
            playerAnimation.PlayShotVictoryRandom();
            StartPlayerGoalTurn();
        }
        else
        {
            playerAnimation.PlayShotDefeat();
        }
    }

    private void PlayKeeperPostResult(bool isGoal, bool isSave, bool hasKeeperAction)
    {
        if (!keeperPostAnimation)
            return;

        // Top auta gittiyse, invalid ise veya keeper'ın önceden başlayan aksiyonu yoksa:
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

    private void PlayCameraPostResult(ShotDecisionResult decision)
    {
        if (!cameraDirector)
            return;

        cameraDirector.PlayPostResultCamera(decision);
    }

    private void PlayFailSafe()
    {
        if (playerAnimation)
            playerAnimation.PlayShotDefeat();

        if (keeperPostAnimation)
            keeperPostAnimation.PlayVictoryNow();

        if (debugLogs)
            Debug.LogWarning("[ShotPostResultAnimationDirector] PlayFailSafe used because decision was null.");
    }

    private void StartPlayerGoalTurn()
    {
        if (!playerRootToRotate)
        {
            if (debugLogs)
                Debug.LogWarning("[ShotPostResultAnimationDirector] Player goal turn skipped. playerRootToRotate is null.");

            return;
        }

        CacheInitialPlayerRotation();
        StopPlayerGoalTurn();

        _playerGoalTurnRoutine = StartCoroutine(PlayerGoalTurnRoutine());
    }

    private IEnumerator PlayerGoalTurnRoutine()
    {
        float duration = Mathf.Max(0.01f, goalTurnDuration);

        Quaternion startRotation = useLocalRotation
            ? playerRootToRotate.localRotation
            : playerRootToRotate.rotation;

        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, goalTurnDegrees, 0f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float deltaTime = useUnscaledTimeForTurn ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += deltaTime;

            float rawT = Mathf.Clamp01(elapsed / duration);
            float easedT = goalTurnCurve != null ? goalTurnCurve.Evaluate(rawT) : rawT;

            Quaternion currentRotation = Quaternion.Slerp(startRotation, targetRotation, easedT);

            if (useLocalRotation)
                playerRootToRotate.localRotation = currentRotation;
            else
                playerRootToRotate.rotation = currentRotation;

            yield return null;
        }

        if (useLocalRotation)
            playerRootToRotate.localRotation = targetRotation;
        else
            playerRootToRotate.rotation = targetRotation;

        _playerGoalTurnRoutine = null;
    }

    private void StopPlayerGoalTurn()
    {
        if (_playerGoalTurnRoutine == null)
            return;

        StopCoroutine(_playerGoalTurnRoutine);
        _playerGoalTurnRoutine = null;
    }

    private void CacheInitialPlayerRotation()
    {
        if (_hasInitialPlayerRotation)
            return;

        if (!playerRootToRotate)
            return;

        _initialPlayerLocalRotation = playerRootToRotate.localRotation;
        _initialPlayerWorldRotation = playerRootToRotate.rotation;
        _hasInitialPlayerRotation = true;
    }

    private void RestoreInitialPlayerRotation()
    {
        if (!playerRootToRotate || !_hasInitialPlayerRotation)
            return;

        if (useLocalRotation)
            playerRootToRotate.localRotation = _initialPlayerLocalRotation;
        else
            playerRootToRotate.rotation = _initialPlayerWorldRotation;
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

    private void OnDisable()
    {
        StopPlayerGoalTurn();
    }
}