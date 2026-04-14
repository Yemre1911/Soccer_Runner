using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotCommitCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Disable While Commit Camera Active")]
    [SerializeField] private Behaviour[] componentsToDisable;

    [Header("Pull Back")]
    [SerializeField] private float pullBackDistance = 4f;
    [SerializeField] private float verticalOffset = 0f;

    [Header("Move")]
    [SerializeField] private float moveDuration = 0.35f;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private readonly List<Behaviour> _disabledComponents = new();
    private Coroutine _moveRoutine;
    private bool _isActive;

    private void Awake()
    {
        if (!cameraTransform)
            cameraTransform = transform;
    }

    public IEnumerator PullBackFromCurrentView()
    {
        if (cameraTransform == null)
            yield break;

        ActivateCommitCamera();

        StopActiveMove();
        _moveRoutine = StartCoroutine(PullBackRoutine());
        yield return _moveRoutine;
        _moveRoutine = null;
    }

    public void SnapPullBackFromCurrentView()
    {
        if (cameraTransform == null)
            return;

        ActivateCommitCamera();

        Vector3 targetPos = GetTargetPosition();
        cameraTransform.position = targetPos;
    }

    public void ActivateCommitCamera()
    {
        if (_isActive)
            return;

        _isActive = true;
        DisableExternalComponents();
    }

    public void DeactivateCommitCamera()
    {
        if (!_isActive)
            return;

        _isActive = false;
        ReEnableExternalComponents();
    }

    public void StopActiveMove()
    {
        if (_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }
    }

    private IEnumerator PullBackRoutine()
    {
        Vector3 startPos = cameraTransform.position;
        Quaternion lockedRotation = cameraTransform.rotation;

        Vector3 targetPos = GetTargetPosition();

        float duration = Mathf.Max(0.01f, moveDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float rawT = Mathf.Clamp01(elapsed / duration);
            float curvedT = moveCurve.Evaluate(rawT);

            cameraTransform.position = Vector3.Lerp(startPos, targetPos, curvedT);
            cameraTransform.rotation = lockedRotation;

            yield return null;
        }

        cameraTransform.position = targetPos;
        cameraTransform.rotation = lockedRotation;
    }

    private Vector3 GetTargetPosition()
    {
        return cameraTransform.position
             - cameraTransform.forward * pullBackDistance
             + Vector3.up * verticalOffset;
    }

    private void DisableExternalComponents()
    {
        _disabledComponents.Clear();

        if (componentsToDisable == null)
            return;

        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            Behaviour component = componentsToDisable[i];

            if (component == null || !component.enabled)
                continue;

            component.enabled = false;
            _disabledComponents.Add(component);
        }
    }

    private void ReEnableExternalComponents()
    {
        for (int i = 0; i < _disabledComponents.Count; i++)
        {
            Behaviour component = _disabledComponents[i];

            if (component != null)
                component.enabled = true;
        }

        _disabledComponents.Clear();
    }

    private void OnDisable()
    {
        StopActiveMove();
        ReEnableExternalComponents();
        _isActive = false;
    }
}