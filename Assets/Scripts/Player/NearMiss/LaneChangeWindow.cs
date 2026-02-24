using System;
using UnityEngine;

public class LaneChangeWindow : MonoBehaviour
{
    [SerializeField] private PlayerLateralMovement lateral;
    [SerializeField] private float endEpsilon = 0.03f;
    [SerializeField] private float maxWindow = 0.5f;

    public bool IsOpen { get; private set; }
    public int Direction { get; private set; } // -1/+1

    public event Action OnWindowOpened;
    public event Action OnWindowClosed;

    private float _t;

    private void Awake()
    {
        if (lateral == null) lateral = GetComponentInParent<PlayerLateralMovement>();
    }

    private void OnEnable()
    {
        if (lateral != null) lateral.OnLaneChangeRequested += Open;
    }

    private void OnDisable()
    {
        if (lateral != null) lateral.OnLaneChangeRequested -= Open;
    }

    private void Open(int dir)
    {
        Direction = dir;
        IsOpen = true;
        _t = 0f;
        OnWindowOpened?.Invoke();
        Debug.Log("Lane window OPEN dir=" + dir);
    }

    private void Update()
    {
        if (!IsOpen || lateral == null) return;

        _t += Time.deltaTime;
        float dx = Mathf.Abs(lateral.transform.position.x - lateral.TargetX);

        if (dx <= endEpsilon || _t >= maxWindow)
        {
            IsOpen = false;
            OnWindowClosed?.Invoke();
            Debug.Log("Lane window CLOSE");
        }
    }
}