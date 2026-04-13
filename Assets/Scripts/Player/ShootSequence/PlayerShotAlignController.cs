using System.Collections;
using UnityEngine;

public class PlayerShotAlignController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool useSmoothRotation = true;
    [SerializeField] private float rotateDuration = 0.12f;
    [SerializeField] private float finishAngleTolerance = 0.5f;

    [Header("Optional Goal Aim")]
    [SerializeField] private string goalAimPointName = "ShotAimPoint";
    [SerializeField] private bool logAlignment = true;


    public IEnumerator AlignToGoal(Transform playerRoot, Transform goalTransform)
    {
        if (playerRoot == null || goalTransform == null)
        {
            Debug.LogWarning("[PlayerShotAlign] playerRoot or goalTransform is NULL.");
            yield break;
        }

        Transform aimTarget = ResolveAimTarget(goalTransform);

        Vector3 targetPos = aimTarget.position;
        Vector3 playerPos = playerRoot.position;

        Vector3 toGoal = targetPos - playerPos;
        Vector3 flatToGoal = toGoal;
        flatToGoal.y = 0f;

        Debug.Log(
            $"[PlayerShotAlign] Player={playerRoot.name} pos={playerPos} rotY={playerRoot.eulerAngles.y:F2} | " +
            $"Goal={goalTransform.name} pos={goalTransform.position} | " +
            $"AimTarget={aimTarget.name} pos={targetPos} | " +
            $"RawDir={toGoal} | FlatDir={flatToGoal} | FlatMag={flatToGoal.magnitude:F4}"
        );

        if (flatToGoal.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning("[PlayerShotAlign] Flat direction too small. Rotation skipped.");
            yield break;
        }

        float signedAngle = Vector3.SignedAngle(playerRoot.forward, flatToGoal.normalized, Vector3.up);
        Debug.Log($"[PlayerShotAlign] SignedAngleToTarget = {signedAngle:F2}");

        Quaternion targetRotation = Quaternion.LookRotation(flatToGoal.normalized, Vector3.up);
        Debug.Log(
            $"[PlayerShotAlign] CurrentRotY={playerRoot.eulerAngles.y:F2} | TargetRotY={targetRotation.eulerAngles.y:F2}"
        );

        if (!useSmoothRotation)
        {
            playerRoot.rotation = targetRotation;
            Debug.Log($"[PlayerShotAlign] Snap rotate applied. NewRotY={playerRoot.eulerAngles.y:F2}");
            yield break;
        }

        Quaternion startRotation = playerRoot.rotation;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, rotateDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            playerRoot.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        playerRoot.rotation = targetRotation;
        Debug.Log($"[PlayerShotAlign] Smooth rotate finished. FinalRotY={playerRoot.eulerAngles.y:F2}");
    }

    private Transform ResolveAimTarget(Transform goalTransform)
    {
        Transform aim = goalTransform.Find(goalAimPointName);
        return aim != null ? aim : goalTransform;
    }
}