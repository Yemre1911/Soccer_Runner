using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class KeeperShotPostAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Fallback")]
    [Tooltip("Save/dive animasyon kliplerine animation event koymazsan bu süre sonunda post animasyonu oynar.")]
    [SerializeField] private bool useFallbackDelay = true;

    [SerializeField] private float fallbackDelay = 0.75f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs;

    private static readonly int Victory1Trigger = Animator.StringToHash("V1");
    private static readonly int Victory2Trigger = Animator.StringToHash("V2");
    private static readonly int Victory3Trigger = Animator.StringToHash("V3");
    private static readonly int Defeat1Trigger = Animator.StringToHash("D1");

    private Coroutine _fallbackRoutine;

    private bool _hasPendingPostResult;
    private bool _pendingSavedResult;

    private void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();
    }

    public void PlayVictoryNow()
    {
        ClearPendingPostResult();
        ResetPostResultTriggers();

        int random = Random.Range(0, 3);

        switch (random)
        {
            case 0:
                animator.SetTrigger(Victory1Trigger);
                break;

            case 1:
                animator.SetTrigger(Victory2Trigger);
                break;

            default:
                animator.SetTrigger(Victory3Trigger);
                break;
        }

        if (debugLogs)
            Debug.Log("[KeeperShotPostAnimationController] Victory played now.");
    }

    public void PlayDefeatNow()
    {
        ClearPendingPostResult();
        ResetPostResultTriggers();

        animator.SetTrigger(Defeat1Trigger);

        if (debugLogs)
            Debug.Log("[KeeperShotPostAnimationController] Defeat played now.");
    }

    public void QueuePostResultAfterKeeperAction(bool saved)
    {
        _hasPendingPostResult = true;
        _pendingSavedResult = saved;

        if (_fallbackRoutine != null)
            StopCoroutine(_fallbackRoutine);

        if (useFallbackDelay)
            _fallbackRoutine = StartCoroutine(FallbackRoutine());

        if (debugLogs)
            Debug.Log($"[KeeperShotPostAnimationController] Queued post result. Saved={saved}");
    }

    // Bunu kalecinin save/dive/miss animation clip'lerinin SONUNA Animation Event olarak ekleyebilirsin.
    public void OnKeeperActionAnimationFinished()
    {
        PlayPendingPostResult();
    }

    private void PlayPendingPostResult()
    {
        if (!_hasPendingPostResult)
            return;

        bool saved = _pendingSavedResult;

        ClearPendingPostResult();

        if (saved)
            PlayVictoryNow();
        else
            PlayDefeatNow();
    }

    private IEnumerator FallbackRoutine()
    {
        yield return new WaitForSeconds(fallbackDelay);
        _fallbackRoutine = null;
        PlayPendingPostResult();
    }

    private void ClearPendingPostResult()
    {
        _hasPendingPostResult = false;
        _pendingSavedResult = false;

        if (_fallbackRoutine != null)
        {
            StopCoroutine(_fallbackRoutine);
            _fallbackRoutine = null;
        }
    }

    private void ResetPostResultTriggers()
    {
        animator.ResetTrigger(Victory1Trigger);
        animator.ResetTrigger(Victory2Trigger);
        animator.ResetTrigger(Victory3Trigger);
        animator.ResetTrigger(Defeat1Trigger);
    }
}