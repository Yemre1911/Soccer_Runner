
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShotSwipeVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera inputCamera;

    [Header("World Placement")]
    [SerializeField] private float distanceFromCamera = 8f;
    [SerializeField] private bool useFixedWorldPlane = false;
    [SerializeField] private Transform planeReference;
    [SerializeField] private float planeHeight = 1.2f;

    [Header("Line")]
    [SerializeField] private float lineWidth = 0.08f;
    [SerializeField] private int maxPointCount = 128;

    [Header("Resolved Extension Debug")]
    [SerializeField] private bool showResolvedExtension = true;

    private LineRenderer _lineRenderer;
    private readonly List<Vector3> _worldPoints = new List<Vector3>();

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();

        if (!inputCamera)
            inputCamera = Camera.main;

        _lineRenderer.positionCount = 0;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.widthMultiplier = lineWidth;
        _lineRenderer.enabled = false;
    }

    public void ShowSwipe(List<Vector2> screenPoints)
    {
        if (screenPoints == null || screenPoints.Count == 0)
        {
            Clear();
            return;
        }

        Camera cam = inputCamera != null ? inputCamera : Camera.main;
        if (cam == null)
            return;

        _worldPoints.Clear();

        int count = Mathf.Min(screenPoints.Count, maxPointCount);
        for (int i = 0; i < count; i++)
            _worldPoints.Add(ConvertScreenToWorld(screenPoints[i], cam));

        ApplyPoints();
    }

    public void ShowResolvedSwipe(RawSwipeData raw, ShotGoalTarget target)
    {
        if (raw == null || raw.Points == null || raw.Points.Count == 0)
        {
            Clear();
            return;
        }

        Camera cam = inputCamera != null ? inputCamera : Camera.main;
        if (cam == null)
            return;

        _worldPoints.Clear();

        int count = Mathf.Min(raw.Points.Count, maxPointCount);
        for (int i = 0; i < count; i++)
            _worldPoints.Add(ConvertScreenToWorld(raw.Points[i], cam));

        if (showResolvedExtension && target != null && target.UsedAutoExtension)
        {
            if (_worldPoints.Count == 0 || Vector3.Distance(_worldPoints[_worldPoints.Count - 1], target.AutoExtendedWorldPoint) > 0.001f)
                _worldPoints.Add(target.AutoExtendedWorldPoint);
        }

        ApplyPoints();
    }

    public void Clear()
    {
        if (_lineRenderer == null)
            return;

        _worldPoints.Clear();
        _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = false;
    }

    private void ApplyPoints()
    {
        _lineRenderer.enabled = true;
        _lineRenderer.positionCount = _worldPoints.Count;
        _lineRenderer.SetPositions(_worldPoints.ToArray());
    }

    private Vector3 ConvertScreenToWorld(Vector2 screenPoint, Camera cam)
    {
        if (!useFixedWorldPlane)
            return cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, distanceFromCamera));

        Ray ray = cam.ScreenPointToRay(screenPoint);
        float targetY = planeReference != null ? planeReference.position.y : planeHeight;
        Plane plane = new Plane(Vector3.up, new Vector3(0f, targetY, 0f));

        if (plane.Raycast(ray, out float enter))
            return ray.GetPoint(enter);

        return cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, distanceFromCamera));
    }
}
