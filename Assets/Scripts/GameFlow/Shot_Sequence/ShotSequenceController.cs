using System;
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
}