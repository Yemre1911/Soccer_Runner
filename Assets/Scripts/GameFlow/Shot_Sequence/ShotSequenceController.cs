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

    [Header("Optional References")]
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

    [Header("Decision / Commit")]
    [SerializeField] private bool allowEarlyCommitInput = false;
    [SerializeField] private KeyCode debugCommitKey = KeyCode.Return;

    public ShotFlowState CurrentState { get; private set; } = ShotFlowState.Idle;

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
    }

    private void OnEnable()
    {
        if (shotTimeController != null)
            shotTimeController.OnCountdownFinished += HandleCountdownFinished;
    }

    private void OnDisable()
    {
        if (shotTimeController != null)
            shotTimeController.OnCountdownFinished -= HandleCountdownFinished;

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

        if (shotTimeController != null)
            shotTimeController.RestoreNormalTimeImmediate();
    }

    private void Update()
    {
        if (!allowEarlyCommitInput)
            return;

        if (CurrentState != ShotFlowState.WaitingForShotInput)
            return;

        if (Input.GetKeyDown(debugCommitKey))
            Debug.Log("[ShotSequenceController] Early commit input is currently disabled logically. Countdown end controls world stop.");
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

        Debug.Log(
            $"[ShotSequence] EnterShotMode | Player={(_playerTransform ? _playerTransform.name : "NULL")} pos={(_playerTransform ? _playerTransform.position.ToString() : "NULL")} | Goal={(_goalTransform ? _goalTransform.name : "NULL")} pos={(_goalTransform ? _goalTransform.position.ToString() : "NULL")}"
        );

        if (_enterShotModeRoutine != null)
            StopCoroutine(_enterShotModeRoutine);

        if (_commitRoutine != null)
        {
            StopCoroutine(_commitRoutine);
            _commitRoutine = null;
        }

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

        if (shotTimeController != null)
            shotTimeController.RestoreNormalTimeImmediate();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetWorldMovementForcedStopped(false);
            GameManager.Instance.SetSimulationPaused(false);
            GameManager.Instance.SetSpeedMultiplier(1f);
        }

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

        if (ballController != null)
        {
            if (snapBallToSocketOnEnter)
                ballController.SnapToSocketImmediate();

            if (stopBallDribbleOnEnter)
                ballController.EnterIdle();
        }

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

        if (playerAnimationController != null)
            playerAnimationController.PlayShoot(mirrorShot);

        SetState(ShotFlowState.WaitingForShotInput);
        OnShotModeEntered?.Invoke();

        if (_pendingAutoCommitFromTimeout)
        {
            _pendingAutoCommitFromTimeout = false;
            StartCommitAfterTimeout();
        }

        Debug.Log("[ShotSequenceController] Shot mode entered.");
        _enterShotModeRoutine = null;
    }

    private void StartCommitAfterTimeout()
    {
        if (_commitStarted)
            return;

        if (CurrentState != ShotFlowState.WaitingForShotInput)
            return;

        _commitStarted = true;

        if (_commitRoutine != null)
            StopCoroutine(_commitRoutine);

        _commitRoutine = StartCoroutine(CommitShotDecisionRoutine());
    }

    private IEnumerator CommitShotDecisionRoutine()
    {
        SetState(ShotFlowState.CommittingShot);

        Debug.Log("[ShotSequenceController] Timeout commit started.");

        // DÜNYA SADECE BURADA, YANİ COUNTDOWN SONUNDA DURDURULUR.
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

        // Burada ileride post-shot / keeper / ball launch işlemleri başlayacak.

        SetState(ShotFlowState.ReadyToResolveShot);

        Debug.Log("[ShotSequenceController] Timeout commit complete. ReadyToResolveShot.");
        _commitRoutine = null;
    }

    private void HandleCountdownFinished()
    {
        if (CurrentState == ShotFlowState.WaitingForShotInput)
        {
            StartCommitAfterTimeout();
            return;
        }

        _pendingAutoCommitFromTimeout = true;
        Debug.Log("[ShotSequenceController] Countdown finished before WaitingForShotInput. Timeout commit queued.");
    }

    private void RestoreGameplayState()
    {
        if (playerLateralMovement != null)
            playerLateralMovement.UnlockAfterShot();

        if (playerInput != null)
            playerInput.SetActionInputEnabled(true);

        if (ballController != null)
        {
            ballController.SnapToSocketImmediate();
            ballController.EnterDribble();
        }
    }

    private void SetState(ShotFlowState newState)
    {
        CurrentState = newState;
    }
}

 /* Dünyayı terkar hareket ettirmek gerekirse kodlar:
    if (GameManager.Instance != null)
{
    GameManager.Instance.SetWorldMovementForcedStopped(false);
    GameManager.Instance.SetSimulationPaused(false);
    GameManager.Instance.SetSpeedMultiplier(1f);
}*/