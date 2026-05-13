using System;
using System.Collections;
using UnityEngine;

public class ShotSequenceController : MonoBehaviour
{
    public enum ShotFlowState
    {
        Idle,
        EnteringShotMode,
        WaitingForShotInput,
        CommittingShot,
        TransitioningToCommitView,
        ReadyToResolveShot,
        ResolvingShot,
        ShowingResult,
        Completed
    }

    [Header("UI")]
    [SerializeField] private GameObject retryButtonRoot;

    [Header("Core References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private BallController ballController;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private PlayerShotAlignController playerShotAlignController;
    [SerializeField] private PlayerLateralMovement playerLateralMovement;

    [Header("Swipe Modules")]
    [SerializeField] private ShotSwipeInputReader shotSwipeInputReader;
    [SerializeField] private ShotSwipeAnalyzer shotSwipeAnalyzer;
    [SerializeField] private ShotGoalTargetInterpreter shotGoalTargetInterpreter;
    [SerializeField] private ShotResolver shotResolver;

    [Header("Swipe Visual")]
    [SerializeField] private ShotSwipeVisualizer shotSwipeVisualizer;

    [Header("Optional Legacy References")]
    [SerializeField] private MonoBehaviour shotInputSystem;
    [SerializeField] private MonoBehaviour playerShotModeController;

    [Header("Time Control")]
    [SerializeField] private ShotTimeController shotTimeController;

    [Header("Shot Camera")]
    [SerializeField] private ShotCameraController shotCameraController;
    [SerializeField] private Transform shotCameraAnchor;

    [Header("Commit Camera")]
    [SerializeField] private ShotCommitCameraController shotCommitCameraController;

    [Header("Shot Preparation")]
    [SerializeField] private bool disableNormalPlayerInputOnEnter = true;
    [SerializeField] private bool snapBallToSocketOnEnter = true;
    [SerializeField] private bool stopBallDribbleOnEnter = true;
    [SerializeField] private bool stopRunningAnimationOnEnter = true;

    [Header("Resolve")]
    [SerializeField] private bool autoResolveImmediately = true;

    [Header("Decision / Commit")]
    [SerializeField] private bool allowEarlyCommitInput = false;
    [SerializeField] private KeyCode debugCommitKey = KeyCode.Return;

    [Header("Shot Ball")]
    [SerializeField] private ShotBallShooter shotBallShooter;

    [Header("Shot Keeper")]
    [SerializeField] private ShotKeeperController shotKeeperController;

    public ShotFlowState CurrentState { get; private set; } = ShotFlowState.Idle;
    public ShotDecisionResult LastDecision { get; private set; }

    public event Action OnShotModeEntered;
    public event Action OnShotCommitted;
    public event Action<bool> OnShotResolved;
    public event Action OnRetryRequested;

    private Coroutine _enterShotModeRoutine;
    private Coroutine _commitRoutine;

    private Transform _playerTransform;
    private Transform _goalTransform;

    private bool _commitStarted;
    private bool _pendingAutoCommitFromTimeout;

    [SerializeField] private CurveChangeController curveChangeController;


    private void Awake()
    {
        if (!playerAnimationController)
            playerAnimationController = FindFirstObjectByType<PlayerAnimationController>();

        if (!playerInput)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (!ballController)
            ballController = FindFirstObjectByType<BallController>();

        if (!playerShotAlignController)
            playerShotAlignController = FindFirstObjectByType<PlayerShotAlignController>();

        if (!playerLateralMovement)
            playerLateralMovement = FindFirstObjectByType<PlayerLateralMovement>();

        if (!shotCameraController)
            shotCameraController = FindFirstObjectByType<ShotCameraController>();

        if (!shotTimeController)
            shotTimeController = FindFirstObjectByType<ShotTimeController>();

        if (!shotCommitCameraController)
            shotCommitCameraController = FindFirstObjectByType<ShotCommitCameraController>();

        if (!shotSwipeInputReader)
            shotSwipeInputReader = FindFirstObjectByType<ShotSwipeInputReader>();

        if (!shotSwipeAnalyzer)
            shotSwipeAnalyzer = FindFirstObjectByType<ShotSwipeAnalyzer>();

        if (!shotGoalTargetInterpreter)
            shotGoalTargetInterpreter = FindFirstObjectByType<ShotGoalTargetInterpreter>();

        if (!shotResolver)
            shotResolver = FindFirstObjectByType<ShotResolver>();

        if (!shotSwipeVisualizer)
            shotSwipeVisualizer = FindFirstObjectByType<ShotSwipeVisualizer>();

        if (!curveChangeController) curveChangeController = FindFirstObjectByType<CurveChangeController>();

        if (!shotKeeperController)
            shotKeeperController = FindFirstObjectByType<ShotKeeperController>();

    }

    private void OnEnable()
    {
        if (shotTimeController != null)
            shotTimeController.OnCountdownFinished += HandleCountdownFinished;

        if (shotSwipeInputReader != null)
            shotSwipeInputReader.OnSwipeCompleted += HandleSwipeCompleted;

        if (shotSwipeInputReader != null)
            shotSwipeInputReader.OnSwipeUpdated += HandleSwipeUpdated;
    }

    private void OnDisable()
    {
        if (shotTimeController != null)
            shotTimeController.OnCountdownFinished -= HandleCountdownFinished;

        if (shotSwipeInputReader != null)
            shotSwipeInputReader.OnSwipeCompleted -= HandleSwipeCompleted;

        if (_enterShotModeRoutine != null)
        {
            StopCoroutine(_enterShotModeRoutine);
            _enterShotModeRoutine = null;
        }

        if (_commitRoutine != null)
        {
            StopCoroutine(_commitRoutine);
            _commitRoutine = null;
        }

        if (shotSwipeInputReader != null)
            shotSwipeInputReader.StopListening();

        if (shotTimeController != null)
            shotTimeController.RestoreNormalTimeImmediate();

        if (shotSwipeInputReader != null)
            shotSwipeInputReader.OnSwipeUpdated -= HandleSwipeUpdated;

        if (shotSwipeVisualizer != null)
            shotSwipeVisualizer.Clear();
    }

    private void Update()
    {
        if (!allowEarlyCommitInput)
            return;

        if (CurrentState != ShotFlowState.WaitingForShotInput)
            return;

        if (Input.GetKeyDown(debugCommitKey))
            Debug.Log("[ShotSequenceController] Debug commit key pressed. Countdown still controls final stop.");
    }

    public void EnterShotMode()
    {
        EnterShotMode(null, null);
    }

    public void EnterShotMode(Transform playerTransform, Transform goalTransform)
    {
        if (playerLateralMovement != null)
            playerLateralMovement.LockForShot();

        _playerTransform = playerTransform;
        _goalTransform = goalTransform;
        _commitStarted = false;
        _pendingAutoCommitFromTimeout = false;
        LastDecision = null;

        if (shotKeeperController != null)
            shotKeeperController.ResetKeeperReaction();

        Debug.Log(
            $"[ShotSequence] EnterShotMode | Player={(_playerTransform ? _playerTransform.name : "NULL")} | Goal={(_goalTransform ? _goalTransform.name : "NULL")}"
        );

        if (_enterShotModeRoutine != null)
            StopCoroutine(_enterShotModeRoutine);

        if (_commitRoutine != null)
        {
            StopCoroutine(_commitRoutine);
            _commitRoutine = null;
        }

        if (shotSwipeInputReader != null)
            shotSwipeInputReader.StopListening();

        _enterShotModeRoutine = StartCoroutine(EnterShotModeRoutine());
    }

    public void ResolveShot(bool isGoal)
    {
        if (CurrentState != ShotFlowState.ReadyToResolveShot &&
            CurrentState != ShotFlowState.ResolvingShot)
            return;

        SetState(ShotFlowState.ResolvingShot);

        OnShotResolved?.Invoke(isGoal);

        SetState(ShotFlowState.ShowingResult);
        SetState(ShotFlowState.Completed);

        Debug.Log($"[ShotSequenceController] Shot resolved. Goal = {isGoal}");
    }

    public void RetryTraining()
    {
        RestoreGameplayState();

        if (shotSwipeInputReader != null)
            shotSwipeInputReader.StopListening();

        if (shotTimeController != null)
            shotTimeController.RestoreNormalTimeImmediate();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetWorldMovementForcedStopped(false);
            GameManager.Instance.SetSimulationPaused(false);
            GameManager.Instance.SetSpeedMultiplier(1f);
        }

        if (shotSwipeVisualizer != null)
            shotSwipeVisualizer.Clear();

        OnRetryRequested?.Invoke();
        SessionBootstrap.ReloadLastConfig("GameScene");
    }

    private IEnumerator EnterShotModeRoutine()
    {
        SetState(ShotFlowState.EnteringShotMode);

        if (disableNormalPlayerInputOnEnter && playerInput != null)
            playerInput.SetActionInputEnabled(false);

        if (stopRunningAnimationOnEnter && playerAnimationController != null)
            playerAnimationController.SetRunningAnimation(false);

        if (playerShotAlignController != null && _playerTransform != null && _goalTransform != null)
            yield return playerShotAlignController.AlignToGoal(_playerTransform, _goalTransform);

        if (shotTimeController != null)
            shotTimeController.BeginShotTimeFlow();

        if (shotCameraController != null && shotCameraAnchor != null)
        {
            shotCameraController.SetAnchor(shotCameraAnchor);
            yield return shotCameraController.MoveToShotViewUntilShootPoint(_playerTransform);
        }

        bool mirrorShot = _playerTransform != null && _playerTransform.position.x < -1f;

        if (ballController != null && stopBallDribbleOnEnter)
            ballController.EnterIdle(true);

        if (playerAnimationController != null)
            playerAnimationController.PlayShoot(mirrorShot);

        SetState(ShotFlowState.WaitingForShotInput);

        if (shotSwipeInputReader != null)
            shotSwipeInputReader.BeginListening();

        OnShotModeEntered?.Invoke();

        if (_pendingAutoCommitFromTimeout)
        {
            _pendingAutoCommitFromTimeout = false;
            HandleCountdownFinished();
        }

        Debug.Log("[ShotSequenceController] Shot mode entered.");
        _enterShotModeRoutine = null;
    }

    private void HandleSwipeCompleted(RawSwipeData rawSwipe)
    {
        if (shotSwipeVisualizer != null)
            shotSwipeVisualizer.Clear();

        if (_commitStarted)
            return;

        if (CurrentState != ShotFlowState.WaitingForShotInput)
            return;

        //PrepareDecisionFromSwipe(rawSwipe);
        //StartCommitWithPreparedDecision();
        PrepareDecisionFromSwipe(rawSwipe);
        StartCommitWithPreparedDecision(true);
    }

    private void HandleCountdownFinished()
    {
        if (CurrentState != ShotFlowState.WaitingForShotInput)
        {
            _pendingAutoCommitFromTimeout = true;
            Debug.Log("[ShotSequenceController] Countdown finished before WaitingForShotInput. Timeout commit queued.");
            return;
        }

        if (_commitStarted)
            return;

        if (shotSwipeVisualizer != null)
            shotSwipeVisualizer.Clear();

        if (shotSwipeInputReader != null &&
            shotSwipeInputReader.TryForceCompleteCurrentSwipe(out RawSwipeData forcedSwipe))
        {
            Debug.Log("[ShotSequenceController] Countdown ended while finger still down. Current swipe captured.");
            PrepareDecisionFromSwipe(forcedSwipe);
            StartCommitWithPreparedDecision(false);
            return;
        }

        PrepareTimeoutFallbackDecision();
        //StartCommitWithPreparedDecision();
        StartCommitWithPreparedDecision(false);
    }
    private void HandleSwipeUpdated(System.Collections.Generic.List<Vector2> points)
    {
        if (shotSwipeVisualizer != null)
            shotSwipeVisualizer.ShowSwipe(points);
    }

    private void PrepareDecisionFromSwipe(RawSwipeData rawSwipe)
    {
        ShotSwipeIntent intent = shotSwipeAnalyzer != null
            ? shotSwipeAnalyzer.Analyze(rawSwipe)
            : CreateFallbackIntent(rawSwipe);

        ShotGoalTarget target = shotGoalTargetInterpreter != null
            ? shotGoalTargetInterpreter.Interpret(intent, _goalTransform)
            : new ShotGoalTarget();

        ShotDecisionResult decision = shotResolver != null
            ? shotResolver.Resolve(intent, target, _goalTransform)
            : CreateFallbackDecision(intent, target);

        LastDecision = decision;

        Debug.Log($"[ShotSequenceController] Swipe decision prepared | Outcome={decision.Outcome} | Goal={decision.IsGoal} | Reason={decision.Reason}");

        Debug.Log(
    $"[ShotSequenceController] Swipe metrics | " +
    $"Valid={intent.IsValid} | Kind={intent.Kind} | " +
    $"TotalLength={intent.TotalLength:F1} | DirectLength={intent.DirectLength:F1} | " +
    $"Power01={intent.Power01:F2} | CurveSigned01={intent.CurveSigned01:F2} | " +
    $"CurveAbs01={intent.CurveAbs01:F2} | Quality01={intent.Quality01:F2} | " +
    $"TooShort={intent.TooShort} | TooLong={intent.TooLong} | Timeout={intent.EndedByTimeout}"
);
    }

    private void PrepareTimeoutFallbackDecision()
    {
        RawSwipeData timeoutSwipe = new RawSwipeData
        {
            StartRealtime = Time.realtimeSinceStartup,
            EndRealtime = Time.realtimeSinceStartup,
            EndedByTimeout = true
        };

        PrepareDecisionFromSwipe(timeoutSwipe);
    }

    private ShotSwipeIntent CreateFallbackIntent(RawSwipeData rawSwipe)
    {
        return new ShotSwipeIntent
        {
            IsValid = rawSwipe != null && rawSwipe.Points != null && rawSwipe.Points.Count > 1,
            EndedByTimeout = rawSwipe != null && rawSwipe.EndedByTimeout,
            Kind = ShotSwipeKind.Straight,
            Power01 = 0.5f,
            Quality01 = 0.5f,
            CurveAbs01 = 0f,
            CurveSigned01 = 0f,
            ScreenStart = rawSwipe != null ? rawSwipe.StartPoint : Vector2.zero,
            ScreenEnd = rawSwipe != null ? rawSwipe.EndPoint : Vector2.zero
        };
    }

    private ShotDecisionResult CreateFallbackDecision(ShotSwipeIntent intent, ShotGoalTarget target)
    {
        return new ShotDecisionResult
        {
            Intent = intent,
            Target = target,
            Outcome = intent.IsValid ? ShotOutcomeType.Saved : ShotOutcomeType.TimeoutOut,
            IsGoal = false,
            Reason = intent.IsValid ? "Fallback saved" : "Fallback timeout"
        };
    }


    private void StartCommitWithPreparedDecision(bool restoreShotTimeImmediately)
    {
        if (_commitStarted)
            return;

        if (CurrentState != ShotFlowState.WaitingForShotInput)
            return;

        _commitStarted = true;

        // ✅ Asıl bug fix burası.
        // Swipe erken tamamlandıysa countdown coroutine'i beklenmez.
        // Time.timeScale hemen 1'e çekilir.
        if (restoreShotTimeImmediately && shotTimeController != null)
            shotTimeController.CancelCountdownAndRestoreForCommit();

        if (shotSwipeInputReader != null)
            shotSwipeInputReader.StopListening();

        if (_commitRoutine != null)
            StopCoroutine(_commitRoutine);

        if (shotSwipeVisualizer != null)
            shotSwipeVisualizer.Clear();

        _commitRoutine = StartCoroutine(CommitShotDecisionRoutine());
    }

    /*private IEnumerator CommitShotDecisionRoutine()
    {
        SetState(ShotFlowState.CommittingShot);

        if (shotBallShooter != null && LastDecision != null)
            shotBallShooter.Shoot(LastDecision);
        else
            Debug.LogWarning("[ShotSequenceController] ShotBallShooter or LastDecision is missing.");

        Debug.Log("[ShotSequenceController] Commit started.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetWorldMovementForcedStopped(true);
            GameManager.Instance.SetSpeedMultiplier(0f);
            GameManager.Instance.SetSimulationPaused(true);
        }

        OnShotCommitted?.Invoke();

        SetState(ShotFlowState.TransitioningToCommitView);

        if (shotCommitCameraController != null)
            yield return shotCommitCameraController.PullBackFromCurrentView();

        SetState(ShotFlowState.ReadyToResolveShot);

        Debug.Log("[ShotSequenceController] Commit complete. ReadyToResolveShot.");

        if (autoResolveImmediately && LastDecision != null)
            ResolveShot(LastDecision.IsGoal);

        _commitRoutine = null;
    }*/

    private IEnumerator CommitShotDecisionRoutine()
    {
        SetState(ShotFlowState.CommittingShot);

        if (LastDecision != null)
        {
            if (shotKeeperController != null)
                shotKeeperController.React(LastDecision);

            if (shotBallShooter != null)
                shotBallShooter.Shoot(LastDecision);
            else
                Debug.LogWarning("[ShotSequenceController] ShotBallShooter is missing.");
        }
        else
        {
            Debug.LogWarning("[ShotSequenceController] LastDecision is missing.");
        }

        Debug.Log("[ShotSequenceController] Commit started.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetWorldMovementForcedStopped(true);
            GameManager.Instance.SetSpeedMultiplier(0f);
            GameManager.Instance.SetSimulationPaused(true);
        }

        OnShotCommitted?.Invoke();

        SetState(ShotFlowState.TransitioningToCommitView);

        if (shotCommitCameraController != null)
            yield return shotCommitCameraController.PullBackFromCurrentView();

        SetState(ShotFlowState.ReadyToResolveShot);

        Debug.Log("[ShotSequenceController] Commit complete. ReadyToResolveShot.");

        if (autoResolveImmediately && LastDecision != null)
            ResolveShot(LastDecision.IsGoal);

        _commitRoutine = null;
    }

    private void RestoreGameplayState()
    {
        if (playerLateralMovement != null)
            playerLateralMovement.UnlockAfterShot();

        if (playerInput != null)
            playerInput.SetActionInputEnabled(true);

        if (ballController != null)
        {
            if (snapBallToSocketOnEnter)
                ballController.SnapToSocketImmediate();

            ballController.EnterDribble();
        }
        if (curveChangeController != null)
            curveChangeController.RestoreDefaults();

    }

    private void SetState(ShotFlowState newState)
    {
        CurrentState = newState;
    }
}