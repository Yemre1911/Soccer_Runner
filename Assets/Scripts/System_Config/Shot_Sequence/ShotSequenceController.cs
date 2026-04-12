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
    [SerializeField] private MonoBehaviour shotInputSystem; // sonra gerçek tipe çevrilir
    [SerializeField] private MonoBehaviour playerShotModeController; // sonra gerçek tipe çevrilir

    public ShotFlowState CurrentState { get; private set; } = ShotFlowState.Idle;

    public event Action OnShotModeEntered;
    public event Action<bool> OnShotResolved;
    public event Action OnRetryRequested;

    public void EnterShotMode()
    {
        SetState(ShotFlowState.EnteringShotMode);

        if (retryButtonRoot) retryButtonRoot.SetActive(false);

        // Burada ileride:
        // - player'ı shot pose/state'e sokacağız
        // - koşu sistemini yavaşlatacağız / sabitleyeceğiz
        // - kamera değişecek
        // - swipe input aktif olacak

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

        // Burada ileride:
        // top animasyonu, keeper reaction, gol / auta çıkma, sesler vs

        OnShotResolved?.Invoke(isGoal);

        SetState(ShotFlowState.ShowingResult);

        if (retryButtonRoot) retryButtonRoot.SetActive(true);

        SetState(ShotFlowState.Completed);

        Debug.Log($"[ShotSequenceController] Shot resolved. Goal = {isGoal}");
    }

    public void RetryTraining()
    {
        OnRetryRequested?.Invoke();
        SessionBootstrap.ReloadLastConfig("GameScene");
    }

    private void SetState(ShotFlowState newState)
    {
        CurrentState = newState;
    }
}