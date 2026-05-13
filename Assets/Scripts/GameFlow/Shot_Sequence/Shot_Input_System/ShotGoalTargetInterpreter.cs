using UnityEngine;

public class ShotGoalTargetInterpreter : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera inputCamera;

    [Header("Shot Colliders")]
    [SerializeField] private BoxCollider goalAreaCollider;
    [SerializeField] private Collider keeperFrontCollider;
    [SerializeField] private Collider keeperWideCollider;
    [SerializeField] private Collider groundCollider;
    [SerializeField] private Collider keeperUpperCollider;

    [Header("Goal Bounds")]
    [Tooltip("Goal bounds kontrolünde çok küçük tolerans. Kenar çizgisinde false dönmesini engeller.")]
    [SerializeField] private float goalBoundsTolerance = 0.03f;

    [Header("Ground Auto Extension")]
    [SerializeField] private bool enableGroundAutoExtension = true;
    [SerializeField] private float autoExtendStepPixels = 30f;
    [SerializeField] private int maxAutoExtendSteps = 40;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    [SerializeField] private bool debugDrawRay = true;
    [SerializeField] private float debugRayLength = 300f;

    public ShotGoalTarget Interpret(ShotSwipeIntent intent, Transform goalTransform)
    {
        ShotGoalTarget target = new ShotGoalTarget
        {
            FinalHitType = ShotHitType.None,
            InitialHitType = ShotHitType.None,
            AutoExtendedResolvedHitType = ShotHitType.None,
            DebugReason = "No interpretation"
        };

        Camera cam = inputCamera != null ? inputCamera : Camera.main;
        if (cam == null)
        {
            Log("Camera not found.");
            target.DebugReason = "Camera not found";
            return target;
        }

        if (goalAreaCollider == null)
        {
            Log("GoalAreaCollider is NULL.");
            target.DebugReason = "GoalAreaCollider is NULL";
            return target;
        }

        target.InitialScreenPoint = intent != null ? intent.ScreenEnd : Vector2.zero;
        target.FinalScreenPoint = target.InitialScreenPoint;
        target.ExtensionDirection = intent != null ? intent.FinalScreenDirection : Vector2.up;

        EvaluateScreenPoint(target.InitialScreenPoint, cam, target, true);

        if (target.InitialHitType == ShotHitType.Ground && enableGroundAutoExtension)
            TryResolveGroundExtension(intent, cam, target);
        else
            target.FinalHitType = target.InitialHitType;

        target.WorldPoint = target.UsedAutoExtension
            ? target.AutoExtendedWorldPoint
            : target.InitialWorldPoint;

        ResolveGoalBoundsData(cam, target);

        Log(
            $"Interpret done | InitialHit={target.InitialHitType} | FinalHit={target.FinalHitType} | " +
            $"InsideGoal={target.InsideGoal} | GroundInitial={target.HitGroundInitially} | " +
            $"UsedExtension={target.UsedAutoExtension} | Steps={target.AutoExtendSteps} | " +
            $"FinalScreen={target.FinalScreenPoint} | Normalized={target.Normalized} | Reason={target.DebugReason}");

        return target;
    }

    private void TryResolveGroundExtension(ShotSwipeIntent intent, Camera cam, ShotGoalTarget target)
    {
        target.HitGroundInitially = true;

        Vector2 direction = intent != null ? intent.FinalScreenDirection : Vector2.up;
        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.up;

        direction.Normalize();

        Vector2 current = target.InitialScreenPoint;

        for (int i = 1; i <= maxAutoExtendSteps; i++)
        {
            current += direction * autoExtendStepPixels;

            target.UsedAutoExtension = true;
            target.AutoExtendSteps = i;
            target.AutoExtendDistancePixels = i * autoExtendStepPixels;
            target.AutoExtendedScreenPoint = current;
            target.FinalScreenPoint = current;

            ShotHitType hit = EvaluateScreenPoint(current, cam, target, false);
            target.AutoExtendedResolvedHitType = hit;

            if (hit != ShotHitType.Ground)
            {
                target.ExtensionStillOnGround = false;
                target.FinalHitType = hit;
                target.DebugReason = $"Ground extension escaped ground after {i} steps and resolved to {hit}";
                return;
            }
        }

        target.ExtensionStillOnGround = true;
        target.FinalHitType = ShotHitType.None;
        target.DebugReason = "Ground extension never escaped ground; treat as out";
    }

    private ShotHitType EvaluateScreenPoint(Vector2 screenPoint, Camera cam, ShotGoalTarget target, bool isInitial)
    {
        Ray ray = cam.ScreenPointToRay(screenPoint);

        if (debugDrawRay)
            Debug.DrawRay(ray.origin, ray.direction * debugRayLength, isInitial ? Color.yellow : Color.cyan, 1.75f);

        ShotHitType hitType = RaycastPriority(ray, out RaycastHit hit);

        Vector3 worldPoint;

        if (hitType != ShotHitType.None)
        {
            worldPoint = hit.point;
        }
        else if (TryProjectToGoalPlane(ray, out Vector3 planePoint))
        {
            worldPoint = planePoint;
        }
        else
        {
            worldPoint = ray.origin + ray.direction * debugRayLength;
        }

        if (isInitial)
        {
            target.InitialHitType = hitType;
            target.InitialWorldPoint = worldPoint;
        }
        else
        {
            target.AutoExtendedWorldPoint = worldPoint;
        }

        if (hitType == ShotHitType.Ground)
            target.HitGroundInitially |= isInitial;

        Log($"ScreenPoint={screenPoint} | Initial={isInitial} | Hit={hitType} | World={worldPoint}");

        return hitType;
    }


    private ShotHitType RaycastPriority(Ray ray, out RaycastHit bestHit)
    {
        bestHit = default;

        /*
         * Upper'ı önce kontrol ediyoruz.
         * Eğer upper collider front/wide ile çakışıyorsa,
         * swipe noktası upper alanına geldiğinde JumpCatch kararının kaçmaması için bu daha güvenli.
         */
        if (TryRaycast(keeperUpperCollider, ray, out bestHit))
            return ShotHitType.KeeperUpper;

        if (TryRaycast(keeperFrontCollider, ray, out bestHit))
            return ShotHitType.KeeperFront;

        if (TryRaycast(keeperWideCollider, ray, out bestHit))
            return ShotHitType.KeeperWide;

        if (TryRaycast(goalAreaCollider, ray, out bestHit))
            return ShotHitType.Goal;

        if (TryRaycast(groundCollider, ray, out bestHit))
            return ShotHitType.Ground;

        return ShotHitType.None;
    }

    private bool TryRaycast(Collider col, Ray ray, out RaycastHit hit)
    {
        if (col == null)
        {
            hit = default;
            return false;
        }

        return col.Raycast(ray, out hit, debugRayLength);
    }

    private void ResolveGoalBoundsData(Camera cam, ShotGoalTarget target)
    {
        Ray finalRay = cam.ScreenPointToRay(target.FinalScreenPoint);

        if (TryProjectToGoalPlane(finalRay, out Vector3 goalPlanePoint))
        {
            target.InsideGoal = IsPointInsideGoalBounds(goalPlanePoint);
            FillGoalDataFromWorld(target, goalPlanePoint);

            if (!target.InsideGoal)
            {
                target.DebugReason =
                    $"Final screen point projects outside fixed goal bounds. FinalHit={target.FinalHitType}";
            }

            return;
        }

        target.InsideGoal = false;
        FillGoalDataFromWorld(target, target.WorldPoint);

        target.DebugReason = "Could not project final ray to goal plane; treat as outside goal";
    }

    private bool IsPointInsideGoalBounds(Vector3 worldPoint)
    {
        if (goalAreaCollider == null)
            return false;

        Transform t = goalAreaCollider.transform;

        Vector3 localPoint = t.InverseTransformPoint(worldPoint);
        Vector3 centered = localPoint - goalAreaCollider.center;
        Vector3 halfSize = goalAreaCollider.size * 0.5f;

        bool insideX = Mathf.Abs(centered.x) <= halfSize.x + goalBoundsTolerance;
        bool insideY = Mathf.Abs(centered.y) <= halfSize.y + goalBoundsTolerance;

        return insideX && insideY;
    }

    private void FillGoalDataFromWorld(ShotGoalTarget target, Vector3 worldPoint)
    {
        if (goalAreaCollider == null)
            return;

        Transform t = goalAreaCollider.transform;

        Vector3 localPoint = t.InverseTransformPoint(worldPoint);
        Vector3 centered = localPoint - goalAreaCollider.center;
        Vector3 halfSize = goalAreaCollider.size * 0.5f;

        float nx = halfSize.x > 0.0001f
            ? centered.x / halfSize.x
            : 0f;

        float ny = halfSize.y > 0.0001f
            ? Mathf.InverseLerp(-halfSize.y, halfSize.y, centered.y)
            : 0.5f;

        target.Normalized = new Vector2(nx, ny);

        float xCenter = 1f - Mathf.Clamp01(Mathf.Abs(nx));
        float yCenter = 1f - Mathf.Clamp01(Mathf.Abs((ny * 2f) - 1f));

        target.Centrality01 = Mathf.Clamp01((xCenter + yCenter) * 0.5f);
    }

    private bool TryProjectToGoalPlane(Ray ray, out Vector3 point)
    {
        if (goalAreaCollider == null)
        {
            point = Vector3.zero;
            return false;
        }

        Plane plane = new Plane(goalAreaCollider.transform.forward, goalAreaCollider.transform.position);

        if (plane.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    private void Log(string message)
    {
        if (debugLogs)
            Debug.Log($"[ShotTarget] {message}");
    }
}