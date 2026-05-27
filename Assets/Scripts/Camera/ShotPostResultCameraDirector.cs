using System.Collections;
using UnityEngine;

public class ShotPostResultCameraDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GoalCameraSway goalCameraSway;
    [SerializeField] private CinematicEndCam cinematicEndCam;

    [Header("Goal Sway")]
    [SerializeField] private bool swayOnGoal = true;

    [Tooltip("Gol sonrası yumuşak kamera sallantısı süresi.")]
    [SerializeField] private float goalSwayDuration = 1.1f;

    [Tooltip("Sallantı bittikten sonra zoom başlamadan önce küçük bekleme.")]
    [SerializeField] private float delayAfterSway = 0.05f;

    [Header("Cinematic End Cam Safety")]
    [Tooltip("TriggerZoom çağrılmadan önce CinematicEndCam component'i aktif edilsin mi?")]
    [SerializeField] private bool enableCinematicEndCamBeforeZoom = true;

    [Tooltip("CinematicEndCam'in GameObject'i inactive ise zoom öncesi aktif edilsin mi?")]
    [SerializeField] private bool activateCinematicEndCamGameObjectBeforeZoom = true;

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime = true;

    [Tooltip("Gol olduktan sonra sway başlamadan önce beklenecek süre.")]
    [SerializeField] private float delayBeforeSway = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private Coroutine _sequenceRoutine;

    public void PlayPostResultCamera(ShotDecisionResult decision)
    {
        StopActiveSequence();

        bool isGoal = decision != null && IsGoal(decision.Code);

        _sequenceRoutine = StartCoroutine(PostResultCameraRoutine(isGoal));
    }

    public void ResetForNewShot()
    {
        StopActiveSequence();

        if (goalCameraSway)
            goalCameraSway.Stop();

        // CinematicEndCam'e burada dokunmuyoruz.
        // Çünkü aynı script başka sistemlerde de kullanılıyor.
    }

    private IEnumerator PostResultCameraRoutine(bool isGoal)
    {
        if (debugLogs)
            Debug.Log($"[ShotPostResultCameraDirector] Camera sequence started. Goal={isGoal}");

        if (isGoal && swayOnGoal && goalCameraSway)
        {
            if (delayBeforeSway > 0f)
            {
                if (useUnscaledTime)
                    yield return new WaitForSecondsRealtime(delayBeforeSway);
                else
                    yield return new WaitForSeconds(delayBeforeSway);
            }

            goalCameraSway.Play(goalSwayDuration);

            float waitTime = Mathf.Max(0f, goalSwayDuration + delayAfterSway);

            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(waitTime);
            else
                yield return new WaitForSeconds(waitTime);
        }

        TriggerCinematicEndCamSafely();

        _sequenceRoutine = null;
    }

    private void TriggerCinematicEndCamSafely()
    {
        if (!cinematicEndCam)
        {
            if (debugLogs)
                Debug.LogWarning("[ShotPostResultCameraDirector] CinematicEndCam is missing.");

            return;
        }

        if (activateCinematicEndCamGameObjectBeforeZoom && !cinematicEndCam.gameObject.activeSelf)
        {
            cinematicEndCam.gameObject.SetActive(true);

            if (debugLogs)
                Debug.Log("[ShotPostResultCameraDirector] CinematicEndCam GameObject activated before zoom.");
        }

        if (enableCinematicEndCamBeforeZoom && !cinematicEndCam.enabled)
        {
            cinematicEndCam.enabled = true;

            if (debugLogs)
                Debug.Log("[ShotPostResultCameraDirector] CinematicEndCam component enabled before zoom.");
        }

        cinematicEndCam.TriggerZoom();

        if (debugLogs)
            Debug.Log("[ShotPostResultCameraDirector] CinematicEndCam.TriggerZoom called.");
    }

    private void StopActiveSequence()
    {
        if (_sequenceRoutine == null)
            return;

        StopCoroutine(_sequenceRoutine);
        _sequenceRoutine = null;
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

    private void OnDisable()
    {
        StopActiveSequence();

        if (goalCameraSway)
            goalCameraSway.Stop();
    }
}