using System;
using System.Collections;
using UnityEngine;

public class ShotTimeController : MonoBehaviour
{
    public enum TimeFlowState
    {
        Idle,
        DelayingBeforeSlowmo,
        SlowingDown,
        StableSlowmo,
        CountingDown,
        Restoring,
        Completed
    }

    [Header("Slow Motion Start")]
    [SerializeField] private float slowmoStartDelay = 0f;
    [SerializeField] private float slowMotionTargetScale = 0.15f;
    [SerializeField] private float slowdownDuration = 0.35f;

    [Header("Stable Slow Motion")]
    [SerializeField] private float stableSlowmoCountdownSeconds = 3f;

    [Header("Restore")]
    [SerializeField] private bool restoreInstantly = true;
    [SerializeField] private float restoreDuration = 0.45f;

    public TimeFlowState CurrentState { get; private set; } = TimeFlowState.Idle;
    public float CurrentCountdownRemaining { get; private set; }
    public bool IsRunningFlow => _flowRoutine != null;

    public event Action OnSlowmoStarted;
    public event Action OnReachedStableSlowmo;
    public event Action<int> OnCountdownTick;
    public event Action<float> OnCountdownValueChanged;
    public event Action OnCountdownFinished;
    public event Action OnRestoreStarted;
    public event Action OnFlowCompleted;

    private float _defaultFixedDeltaTime;
    private Coroutine _flowRoutine;

    private void Awake()
    {
        _defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void BeginShotTimeFlow()
    {
        StopCurrentFlow();
        _flowRoutine = StartCoroutine(ShotTimeFlowRoutine());
    }

    public void RestoreNormalTimeImmediate()
    {
        StopCurrentFlow();
        ApplyTimeScale(1f);
        CurrentState = TimeFlowState.Idle;
        CurrentCountdownRemaining = 0f;
    }

    private IEnumerator ShotTimeFlowRoutine()
    {
        CurrentState = TimeFlowState.DelayingBeforeSlowmo;

        if (slowmoStartDelay > 0f)
        {
            float delayElapsed = 0f;
            while (delayElapsed < slowmoStartDelay)
            {
                delayElapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        CurrentState = TimeFlowState.SlowingDown;
        OnSlowmoStarted?.Invoke();

        float startScale = Time.timeScale;
        float slowElapsed = 0f;

        while (slowElapsed < slowdownDuration)
        {
            slowElapsed += Time.unscaledDeltaTime;

            float t = slowdownDuration <= 0f ? 1f : Mathf.Clamp01(slowElapsed / slowdownDuration);
            t = EaseInOut(t);

            float scale = Mathf.Lerp(startScale, slowMotionTargetScale, t);
            ApplyTimeScale(scale);

            yield return null;
        }

        ApplyTimeScale(slowMotionTargetScale);

        CurrentState = TimeFlowState.StableSlowmo;
        OnReachedStableSlowmo?.Invoke();

        CurrentState = TimeFlowState.CountingDown;

        float remaining = stableSlowmoCountdownSeconds;
        CurrentCountdownRemaining = remaining;

        int lastWholeSecond = Mathf.CeilToInt(remaining);
        OnCountdownTick?.Invoke(lastWholeSecond);
        OnCountdownValueChanged?.Invoke(remaining);

        while (remaining > 0f)
        {
            remaining -= Time.unscaledDeltaTime;
            if (remaining < 0f)
                remaining = 0f;

            CurrentCountdownRemaining = remaining;
            OnCountdownValueChanged?.Invoke(remaining);

            int currentWholeSecond = Mathf.CeilToInt(remaining);
            if (currentWholeSecond != lastWholeSecond && currentWholeSecond > 0)
            {
                lastWholeSecond = currentWholeSecond;
                OnCountdownTick?.Invoke(currentWholeSecond);
            }

            yield return null;
        }

        OnCountdownFinished?.Invoke();

        CurrentState = TimeFlowState.Restoring;
        OnRestoreStarted?.Invoke();

        if (restoreInstantly)
        {
            ApplyTimeScale(1f);
        }
        else
        {
            float restoreElapsed = 0f;
            float restoreStartScale = Time.timeScale;

            while (restoreElapsed < restoreDuration)
            {
                restoreElapsed += Time.unscaledDeltaTime;

                float t = restoreDuration <= 0f ? 1f : Mathf.Clamp01(restoreElapsed / restoreDuration);
                t = EaseInOut(t);

                float scale = Mathf.Lerp(restoreStartScale, 1f, t);
                ApplyTimeScale(scale);

                yield return null;
            }

            ApplyTimeScale(1f);
        }

        CurrentState = TimeFlowState.Completed;
        _flowRoutine = null;
        OnFlowCompleted?.Invoke();
    }

    private void StopCurrentFlow()
    {
        if (_flowRoutine != null)
        {
            StopCoroutine(_flowRoutine);
            _flowRoutine = null;
        }
    }

    private void ApplyTimeScale(float scale)
    {
        scale = Mathf.Clamp(scale, 0f, 1f);

        Time.timeScale = scale;

        if (scale <= 0f)
            Time.fixedDeltaTime = 0f;
        else
            Time.fixedDeltaTime = _defaultFixedDeltaTime * scale;
    }

    private float EaseInOut(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void OnDisable()
    {
        RestoreNormalTimeImmediate();
    }
}