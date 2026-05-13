using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GoalNetImpactZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GoalNetBlendShapeController netController;

    [Header("Zone")]
    [SerializeField] private GoalNetBlendShapeController.NetHitArea hitArea;

    [Header("Filter")]
    [SerializeField] private string ballTag = "Ball";
    [SerializeField] private bool acceptUntaggedBall = true;

    [Header("Anti Spam")]
    [SerializeField] private float cooldown = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private float _lastHitTime = -999f;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        if (!netController)
            netController = GetComponentInParent<GoalNetBlendShapeController>();

        if (!netController)
            netController = FindFirstObjectByType<GoalNetBlendShapeController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - _lastHitTime < cooldown)
            return;

        if (!IsBall(other))
            return;

        _lastHitTime = Time.time;

        if (netController)
        {
            netController.PlayHit(hitArea);

            if (debugLogs)
                Debug.Log($"[GoalNetImpactZone] Hit area triggered: {hitArea} by {other.name}");
        }
    }

    private bool IsBall(Collider other)
    {
        if (other.CompareTag(ballTag))
            return true;

        if (!acceptUntaggedBall)
            return false;

        BallController ball = other.GetComponentInParent<BallController>();
        return ball != null;
    }
}