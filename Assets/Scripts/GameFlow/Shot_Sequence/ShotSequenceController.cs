/*using System;
using UnityEngine;

public class ShotSequenceController : MonoBehaviour
{
    public enum ShotFlowState
    {
        Idle,
        EnteringShotMode,
        WaitingForShotInput,
        ResolvingShot,
        ShowingResult,
        Completed
    }

    [Header("UI")]
    [SerializeField] private GameObject retryButtonRoot;

    [Header("Optional References")]
    [SerializeField] private MonoBehaviour shotInputSystem;
    [SerializeField] private MonoBehaviour playerShotModeController;
    [SerializeField] private PlayerAnimationController playerAnimationController;

    [Header("Slow Motion")]
    [SerializeField] private bool useSlowMotionOnEnter = true;
    [SerializeField] private float slowMotionScale = 0.15f;

    public ShotFlowState CurrentState { get; private set; } = ShotFlowState.Idle;

    public event Action OnShotModeEntered;
    public event Action<bool> OnShotResolved;
    public event Action OnRetryRequested;

    private float _defaultFixedDeltaTime;

    private void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;

        if (!playerAnimationController)
            playerAnimationController = FindFirstObjectByType<PlayerAnimationController>();
    }

    public void EnterShotMode()
    {
        SetState(ShotFlowState.EnteringShotMode);

        if (retryButtonRoot) retryButtonRoot.SetActive(false);

        if (playerAnimationController != null)
            playerAnimationController.PlayShoot();

        if (useSlowMotionOnEnter)
            ApplySlowMotion();

        SetState(ShotFlowState.WaitingForShotInput);
        OnShotModeEntered?.Invoke();

        Debug.Log("[ShotSequenceController] Shot mode entered.");
    }

    public void ResolveShot(bool isGoal)
    {
        if (CurrentState != ShotFlowState.WaitingForShotInput &&
            CurrentState != ShotFlowState.ResolvingShot)
            return;

        SetState(ShotFlowState.ResolvingShot);

        OnShotResolved?.Invoke(isGoal);

        SetState(ShotFlowState.ShowingResult);

        if (retryButtonRoot) retryButtonRoot.SetActive(true);

        SetState(ShotFlowState.Completed);

        Debug.Log($"[ShotSequenceController] Shot resolved. Goal = {isGoal}");
    }

    public void RetryTraining()
    {
        RestoreTimeScale();
        OnRetryRequested?.Invoke();
        SessionBootstrap.ReloadLastConfig("GameScene");
    }

    public void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime;
    }

    private void ApplySlowMotion()
    {
        Time.timeScale = Mathf.Clamp(slowMotionScale, 0.01f, 1f);
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
    }

    private void SetState(ShotFlowState newState)
    {
        CurrentState = newState;
    }

    private void OnDisable()
    {
        RestoreTimeScale();
    }
}*/

/*
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
    [SerializeField] private ShotCameraController shotCameraController;

    [Header("Optional References")]
    [SerializeField] private MonoBehaviour shotInputSystem;
    [SerializeField] private MonoBehaviour playerShotModeController;

    [Header("Slow Motion")]
    [SerializeField] private bool useSlowMotionOnEnter = true;
    [SerializeField] private float slowMotionScale = 0.15f;

    [Header("Shot Preparation")]
    [SerializeField] private bool disableNormalPlayerInputOnEnter = true;
    [SerializeField] private bool snapBallToSocketOnEnter = true;
    [SerializeField] private bool stopBallDribbleOnEnter = true;
    [SerializeField] private bool stopRunningAnimationOnEnter = true;

    public ShotFlowState CurrentState { get; private set; } = ShotFlowState.Idle;

    public event Action OnShotModeEntered;
    public event Action<bool> OnShotResolved;
    public event Action OnRetryRequested;

    private float _defaultFixedDeltaTime;
    private Coroutine _enterShotModeRoutine;

    private Transform _playerTransform;
    private Transform _goalTransform;

    private void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;

        if (!playerAnimationController)
            playerAnimationController = FindFirstObjectByType<PlayerAnimationController>();

        if (!playerInput)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (!ballController)
            ballController = FindFirstObjectByType<BallController>();

        if (!shotCameraController)
            shotCameraController = FindFirstObjectByType<ShotCameraController>();
    }

    public void EnterShotMode()
    {
        EnterShotMode(null, null);
    }

    public void EnterShotMode(Transform playerTransform, Transform goalTransform)
    {
        _playerTransform = playerTransform;
        _goalTransform = goalTransform;

        if (_enterShotModeRoutine != null)
            StopCoroutine(_enterShotModeRoutine);

        _enterShotModeRoutine = StartCoroutine(EnterShotModeRoutine());
    }

    public void ResolveShot(bool isGoal)
    {
        if (CurrentState != ShotFlowState.WaitingForShotInput &&
            CurrentState != ShotFlowState.ResolvingShot)
            return;

        SetState(ShotFlowState.ResolvingShot);

        OnShotResolved?.Invoke(isGoal);

        SetState(ShotFlowState.ShowingResult);

        // Retry butonunu burada da ellemedim.
        // Şimdilik görünür kalmasını istiyorsun.

        SetState(ShotFlowState.Completed);

        Debug.Log($"[ShotSequenceController] Shot resolved. Goal = {isGoal}");
    }

    public void RetryTraining()
    {
        RestoreGameplayState();
        RestoreTimeScale();

        OnRetryRequested?.Invoke();
        SessionBootstrap.ReloadLastConfig("GameScene");
    }

    public void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime;
    }

    private IEnumerator EnterShotModeRoutine()
    {
        SetState(ShotFlowState.EnteringShotMode);

        PreparePlayerAndBallForShot();

        if (useSlowMotionOnEnter)
            ApplySlowMotion();

        if (shotCameraController != null)
            shotCameraController.EnterShotView(_playerTransform, _goalTransform);

        // Şimdilik kamera blend süresini burada beklemiyoruz.
        // Sonra istersek event/tabanlı daha temiz hale getiririz.
        yield return null;

        SetState(ShotFlowState.WaitingForShotInput);
        OnShotModeEntered?.Invoke();

        Debug.Log("[ShotSequenceController] Shot mode entered.");
        _enterShotModeRoutine = null;
    }

    private void PreparePlayerAndBallForShot()
    {
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

        if (playerAnimationController != null)
            playerAnimationController.PlayShoot();
    }

    private void ApplySlowMotion()
    {
        Time.timeScale = Mathf.Clamp(slowMotionScale, 0.01f, 1f);
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
    }

    private void RestoreGameplayState()
    {
        if (playerInput != null)
            playerInput.SetActionInputEnabled(true);

        if (ballController != null)
        {
            ballController.SnapToSocketImmediate();
            ballController.EnterDribble();
        }

        if (shotCameraController != null)
            shotCameraController.ExitShotView();
    }

    private void SetState(ShotFlowState newState)
    {
        CurrentState = newState;
    }

    private void OnDisable()
    {
        if (_enterShotModeRoutine != null)
        {
            StopCoroutine(_enterShotModeRoutine);
            _enterShotModeRoutine = null;
        }

        if (shotCameraController != null)
            shotCameraController.ExitShotView();

        RestoreTimeScale();
    }
}*/


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

    [Header("Slow Motion")]
    [SerializeField] private bool useSlowMotionOnEnter = true;
    [SerializeField] private float slowMotionScale = 0.15f;

    [Header("Shot Preparation")]
    [SerializeField] private bool disableNormalPlayerInputOnEnter = true;
    [SerializeField] private bool snapBallToSocketOnEnter = true;
    [SerializeField] private bool stopBallDribbleOnEnter = true;
    [SerializeField] private bool stopRunningAnimationOnEnter = true;

    public ShotFlowState CurrentState { get; private set; } = ShotFlowState.Idle;

    public event Action OnShotModeEntered;
    public event Action<bool> OnShotResolved;
    public event Action OnRetryRequested;

    private float _defaultFixedDeltaTime;
    private Coroutine _enterShotModeRoutine;

    private Transform _playerTransform;
    private Transform _goalTransform;

    private void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;

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

        Debug.Log(
        $"[ShotSequence] EnterShotMode | Player={(_playerTransform ? _playerTransform.name : "NULL")} pos={(_playerTransform ? _playerTransform.position.ToString() : "NULL")} | Goal={(_goalTransform ? _goalTransform.name : "NULL")} pos={(_goalTransform ? _goalTransform.position.ToString() : "NULL")}"
    );

        if (_enterShotModeRoutine != null)
            StopCoroutine(_enterShotModeRoutine);

        _enterShotModeRoutine = StartCoroutine(EnterShotModeRoutine());
    }

    public void ResolveShot(bool isGoal)
    {
        if (CurrentState != ShotFlowState.WaitingForShotInput &&
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
        RestoreTimeScale();

        OnRetryRequested?.Invoke();
        SessionBootstrap.ReloadLastConfig("GameScene");
    }

    public void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = _defaultFixedDeltaTime;
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

        bool mirrorShot = _playerTransform != null && _playerTransform.position.x < -1f;

        if (playerAnimationController != null)
            playerAnimationController.PlayShoot(mirrorShot);

        if (useSlowMotionOnEnter)
            ApplySlowMotion();

        SetState(ShotFlowState.WaitingForShotInput);
        OnShotModeEntered?.Invoke();

        Debug.Log("[ShotSequenceController] Shot mode entered.");
        _enterShotModeRoutine = null;
    }

    private void ApplySlowMotion()
    {
        Time.timeScale = Mathf.Clamp(slowMotionScale, 0.01f, 1f);
        Time.fixedDeltaTime = _defaultFixedDeltaTime * Time.timeScale;
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

    private void OnDisable()
    {
        if (_enterShotModeRoutine != null)
        {
            StopCoroutine(_enterShotModeRoutine);
            _enterShotModeRoutine = null;
        }

        RestoreTimeScale();
    }
}