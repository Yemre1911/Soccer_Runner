using System.Collections;
using UnityEngine;

public class BallFreeRollIndicator : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Transform followTarget;

    [Tooltip("Top pozisyonuna eklenecek world offset. İlk testte 0,0,0 kullan.")]
    [SerializeField] private Vector3 worldOffset = Vector3.zero;

    [Header("Animation")]
    [SerializeField] private Animator visualAnimator;
    [SerializeField] private string showTriggerName = "Show";
    [SerializeField] private string hideTriggerName = "Hide";
    [SerializeField] private float hideDuration = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Coroutine _hideRoutine;
    private bool _visible;

    private int _showTriggerHash;
    private int _hideTriggerHash;

    private void Awake()
    {
        _showTriggerHash = Animator.StringToHash(showTriggerName);
        _hideTriggerHash = Animator.StringToHash(hideTriggerName);

        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        Follow();
    }

    public void Show(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("[BallFreeRollIndicator] Show target is null.", this);
            return;
        }

        followTarget = target;
        _visible = true;

        if (_hideRoutine != null)
        {
            StopCoroutine(_hideRoutine);
            _hideRoutine = null;
        }

        gameObject.SetActive(true);

        Follow();

        if (visualAnimator != null)
        {
            visualAnimator.enabled = true;
            visualAnimator.ResetTrigger(_hideTriggerHash);
            visualAnimator.SetTrigger(_showTriggerHash);
        }

        if (debugLogs)
            Debug.Log($"[BallFreeRollIndicator] Following: {followTarget.name} | pos: {followTarget.position}", this);
    }

    public void Hide()
    {
        if (!_visible)
            return;

        if (_hideRoutine != null)
            StopCoroutine(_hideRoutine);

        _hideRoutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator HideRoutine()
    {
        _visible = false;

        if (visualAnimator != null)
        {
            visualAnimator.ResetTrigger(_showTriggerHash);
            visualAnimator.SetTrigger(_hideTriggerHash);
        }

        float timer = 0f;

        while (timer < hideDuration)
        {
            timer += Time.deltaTime;
            Follow();
            yield return null;
        }

        followTarget = null;
        gameObject.SetActive(false);
        _hideRoutine = null;
    }

    private void Follow()
    {
        if (followTarget == null)
            return;

        transform.position = followTarget.position + worldOffset;
    }
}