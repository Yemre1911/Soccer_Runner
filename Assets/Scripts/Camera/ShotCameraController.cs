using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform shotCameraAnchor;

    [Header("Disable While Shot Camera Active")]
    [SerializeField] private Behaviour[] componentsToDisable;

    [Header("Position")]
    [SerializeField] private float backwardOffset = 8f;
    [SerializeField] private float verticalOffset = 2.5f;

    [Header("Rotation")]
    [SerializeField] private float pitch = 12f;
    [SerializeField] private float yawOffset = 10f;
    [SerializeField] private bool mirrorYawOffsetByShotSide = true;
    [SerializeField] private float centerDeadZoneX = 3.5f;
    [SerializeField] private float roll = 0f;

    [Header("Move")]
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Animation Sync")]
    [SerializeField, Range(0.01f, 1f)] private float shootTriggerProgress = 0.82f;

    private readonly List<Behaviour> _disabledByShotCamera = new();
    private Coroutine _moveRoutine;
    private bool _shotCameraActive;

    private void Awake()
    {
        if (!cameraTransform)
            cameraTransform = transform;
    }

    public void SetAnchor(Transform newAnchor)
    {
        shotCameraAnchor = newAnchor;
    }

    public void StopActiveMove()
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
    }

    public IEnumerator MoveToShotViewUntilShootPoint(Transform playerTransform)
    {
        if (cameraTransform == null || playerTransform == null || shotCameraAnchor == null)
            yield break;

        ActivateShotCamera();

        StopActiveMove();
        _moveRoutine = StartCoroutine(MoveRoutine(playerTransform));
        yield return _moveRoutine;
        _moveRoutine = null;
    }

    public void SnapToShotView(Transform playerTransform)
    {
        if (cameraTransform == null || playerTransform == null || shotCameraAnchor == null)
            return;

        ActivateShotCamera();

        cameraTransform.position = GetTargetPosition(playerTransform);
        cameraTransform.rotation = GetTargetRotation(playerTransform);
    }

    public void ActivateShotCamera()
    {
        if (_shotCameraActive)
            return;

        _shotCameraActive = true;
        DisableExternalCameraComponents();
    }

    public void DeactivateShotCamera()
    {
        if (!_shotCameraActive)
            return;

        _shotCameraActive = false;
        ReEnableExternalCameraComponents();
    }

    private IEnumerator MoveRoutine(Transform playerTransform)
    {
        Vector3 startPos = cameraTransform.position;
        Quaternion startRot = cameraTransform.rotation;

        Vector3 targetPos = GetTargetPosition(playerTransform);
        Quaternion targetRot = GetTargetRotation(playerTransform);

        float duration = Mathf.Max(0.01f, moveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float rawT = Mathf.Clamp01(elapsed / duration);
            float curvedT = moveCurve.Evaluate(rawT);

            cameraTransform.position = Vector3.Lerp(startPos, targetPos, curvedT);
            cameraTransform.rotation = Quaternion.Slerp(startRot, targetRot, curvedT);

            if (rawT >= shootTriggerProgress)
                yield break;

            yield return null;
        }

        cameraTransform.position = targetPos;
        cameraTransform.rotation = targetRot;
    }

    private Vector3 GetTargetPosition(Transform playerTransform)
    {
        Vector3 anchorWorldPos = shotCameraAnchor.position;

        return anchorWorldPos
             - playerTransform.forward * backwardOffset
             + Vector3.up * verticalOffset;
    }

    private Quaternion GetTargetRotation(Transform playerTransform)
    {
        float playerX = playerTransform.position.x;
        float finalYawOffset = 0f;

        if (Mathf.Abs(playerX) > centerDeadZoneX)
        {
            finalYawOffset = yawOffset;

            if (mirrorYawOffsetByShotSide)
                finalYawOffset *= playerX < 0f ? 1f : -1f;
        }

        float yaw = playerTransform.eulerAngles.y + finalYawOffset;
        return Quaternion.Euler(pitch, yaw, roll);
    }

    private void DisableExternalCameraComponents()
    {
        _disabledByShotCamera.Clear();

        if (componentsToDisable == null)
            return;

        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            Behaviour component = componentsToDisable[i];

            if (component == null)
                continue;

            if (!component.enabled)
                continue;

            component.enabled = false;
            _disabledByShotCamera.Add(component);
        }
    }

    private void ReEnableExternalCameraComponents()
    {
        for (int i = 0; i < _disabledByShotCamera.Count; i++)
        {
            Behaviour component = _disabledByShotCamera[i];

            if (component != null)
                component.enabled = true;
        }

        _disabledByShotCamera.Clear();
    }

    private void OnDisable()
    {
        StopActiveMove();
        ReEnableExternalCameraComponents();
        _shotCameraActive = false;
    }
}