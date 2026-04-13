using System.Collections;
using UnityEngine;

public class ShotCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private CameraDynamicFOV cameraDynamicFOV;
    [SerializeField] private CinematicEndCam cinematicEndCam;

    [Header("Shot Blend")]
    [SerializeField] private float blendDuration = 0.18f;
    [SerializeField] private Vector3 shotHolderWorldOffset = new Vector3(0f, 0.15f, 0f);
    [SerializeField] private Vector3 shotCameraLocalPosition = new Vector3(0f, 2.1f, -4.25f);
    [SerializeField] private Vector3 shotCameraLocalEuler = new Vector3(10f, 0f, 0f);

    [Header("Look Target")]
    [SerializeField] private float shotLookHeight = 1.15f;
    [SerializeField] [Range(0f, 1f)] private float goalLookWeight = 0.55f;

    private Coroutine _blendRoutine;

    private bool _hasCachedState;
    private Vector3 _cachedHolderPosition;
    private Quaternion _cachedHolderRotation;
    private Vector3 _cachedCameraLocalPosition;
    private Quaternion _cachedCameraLocalRotation;
    private bool _cachedFollowEnabled;
    private bool _cachedFovEnabled;
    private bool _cachedCinematicEnabled;

    private void Awake()
    {
        AutoResolveReferences();
    }

    public void EnterShotView(Transform playerTransform, Transform goalTransform)
    {
        AutoResolveReferences();

        if (cameraHolder == null || mainCamera == null || playerTransform == null)
        {
            Debug.LogWarning("[ShotCameraController] Missing required references.");
            return;
        }

        if (_blendRoutine != null)
            StopCoroutine(_blendRoutine);

        _blendRoutine = StartCoroutine(BlendToShotView(playerTransform, goalTransform));
    }

    public void ExitShotView()
    {
        if (_blendRoutine != null)
        {
            StopCoroutine(_blendRoutine);
            _blendRoutine = null;
        }

        RestoreCachedState();
    }

    private IEnumerator BlendToShotView(Transform playerTransform, Transform goalTransform)
    {
        CacheCurrentState();
        DisableNormalCameraControllers();

        Vector3 startHolderPos = cameraHolder.position;
        Quaternion startHolderRot = cameraHolder.rotation;
        Vector3 startCamLocalPos = mainCamera.transform.localPosition;
        Quaternion startCamLocalRot = mainCamera.transform.localRotation;

        Vector3 targetHolderPos = playerTransform.position + shotHolderWorldOffset;
        Vector3 lookTarget = GetLookTarget(playerTransform, goalTransform);
        Quaternion targetHolderRot = Quaternion.LookRotation(lookTarget - targetHolderPos, Vector3.up);

        Vector3 targetCamLocalPos = shotCameraLocalPosition;
        Quaternion targetCamLocalRot = Quaternion.Euler(shotCameraLocalEuler);

        float duration = Mathf.Max(0.01f, blendDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            cameraHolder.position = Vector3.Lerp(startHolderPos, targetHolderPos, t);
            cameraHolder.rotation = Quaternion.Slerp(startHolderRot, targetHolderRot, t);

            mainCamera.transform.localPosition = Vector3.Lerp(startCamLocalPos, targetCamLocalPos, t);
            mainCamera.transform.localRotation = Quaternion.Slerp(startCamLocalRot, targetCamLocalRot, t);

            yield return null;
        }

        cameraHolder.position = targetHolderPos;
        cameraHolder.rotation = targetHolderRot;
        mainCamera.transform.localPosition = targetCamLocalPos;
        mainCamera.transform.localRotation = targetCamLocalRot;

        _blendRoutine = null;
    }

    private Vector3 GetLookTarget(Transform playerTransform, Transform goalTransform)
    {
        Vector3 playerPoint = playerTransform.position + Vector3.up * shotLookHeight;

        if (goalTransform == null)
            return playerPoint + playerTransform.forward * 8f;

        Vector3 goalPoint = goalTransform.position + Vector3.up * shotLookHeight;
        return Vector3.Lerp(playerPoint, goalPoint, goalLookWeight);
    }

    private void AutoResolveReferences()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null && cameraHolder == null)
            cameraHolder = mainCamera.transform.parent;

        if (mainCamera != null && cameraFollow == null)
            cameraFollow = mainCamera.GetComponent<CameraFollow>();

        if (mainCamera != null && cameraDynamicFOV == null)
            cameraDynamicFOV = mainCamera.GetComponent<CameraDynamicFOV>();

        if (mainCamera != null && cinematicEndCam == null)
            cinematicEndCam = mainCamera.GetComponent<CinematicEndCam>();
    }

    private void CacheCurrentState()
    {
        if (_hasCachedState || cameraHolder == null || mainCamera == null)
            return;

        _cachedHolderPosition = cameraHolder.position;
        _cachedHolderRotation = cameraHolder.rotation;
        _cachedCameraLocalPosition = mainCamera.transform.localPosition;
        _cachedCameraLocalRotation = mainCamera.transform.localRotation;

        _cachedFollowEnabled = cameraFollow != null && cameraFollow.enabled;
        _cachedFovEnabled = cameraDynamicFOV != null && cameraDynamicFOV.enabled;
        _cachedCinematicEnabled = cinematicEndCam != null && cinematicEndCam.enabled;

        _hasCachedState = true;
    }

    private void DisableNormalCameraControllers()
    {
        if (cameraFollow != null) cameraFollow.enabled = false;
        if (cameraDynamicFOV != null) cameraDynamicFOV.enabled = false;
        if (cinematicEndCam != null) cinematicEndCam.enabled = false;
    }

    private void RestoreCachedState()
    {
        if (!_hasCachedState || cameraHolder == null || mainCamera == null)
            return;

        cameraHolder.position = _cachedHolderPosition;
        cameraHolder.rotation = _cachedHolderRotation;
        mainCamera.transform.localPosition = _cachedCameraLocalPosition;
        mainCamera.transform.localRotation = _cachedCameraLocalRotation;

        if (cameraFollow != null) cameraFollow.enabled = _cachedFollowEnabled;
        if (cameraDynamicFOV != null) cameraDynamicFOV.enabled = _cachedFovEnabled;
        if (cinematicEndCam != null) cinematicEndCam.enabled = _cachedCinematicEnabled;

        _hasCachedState = false;
    }

    private void OnDisable()
    {
        ExitShotView();
    }
}