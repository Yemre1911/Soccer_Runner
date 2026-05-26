using System;
using System.Collections.Generic;
using UnityEngine;

public class ShotSwipeInputReader : MonoBehaviour
{
    [Header("Sampling")]
    [SerializeField] private float minPointDistancePixels = 14f;

    public bool IsListening { get; private set; }
    public bool IsSwiping { get; private set; }

    public event Action OnSwipeStarted;
    public event Action<List<Vector2>> OnSwipeUpdated;
    public event Action<RawSwipeData> OnSwipeCompleted;

    [Header("Swipe Area Gate")]
    [SerializeField] private ShotSwipeAreaCircle swipeAreaCircle;
    [SerializeField] private bool requireValidSwipeAreaStart = true;

    private RawSwipeData _currentSwipe;

    public void BeginListening()
    {
        StopListening();
        IsListening = true;
    }

    public void StopListening()
    {
        IsListening = false;
        IsSwiping = false;
        _currentSwipe = null;
    }

    public bool TryForceCompleteCurrentSwipe(out RawSwipeData swipeData)
    {
        swipeData = null;

        if (!IsListening || !IsSwiping || _currentSwipe == null || _currentSwipe.Points.Count == 0)
            return false;

        _currentSwipe.EndRealtime = Time.realtimeSinceStartup;
        _currentSwipe.EndedByTimeout = true;

        swipeData = _currentSwipe.Clone();

        IsSwiping = false;
        IsListening = false;
        _currentSwipe = null;
        return true;
    }

    private void Update()
    {
        if (!IsListening)
            return;

        if (Input.touchCount > 0)
        {
            HandleTouch(Input.GetTouch(0));
            return;
        }

        HandleMouse();
    }

    private void HandleTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                StartSwipe(touch.position);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                ContinueSwipe(touch.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                FinishSwipe(touch.position);
                break;
        }
    }

    private void HandleMouse()
    {
        Vector2 mousePos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
            StartSwipe(mousePos);

        if (Input.GetMouseButton(0))
            ContinueSwipe(mousePos);

        if (Input.GetMouseButtonUp(0))
            FinishSwipe(mousePos);
    }

    /*private void StartSwipe(Vector2 screenPos)
    {
        _currentSwipe = new RawSwipeData
        {
            StartRealtime = Time.realtimeSinceStartup
        };

        _currentSwipe.Points.Add(screenPos);

        IsSwiping = true;
        OnSwipeStarted?.Invoke();
        OnSwipeUpdated?.Invoke(_currentSwipe.Points);
    }*/
    private void StartSwipe(Vector2 screenPos)
    {
        if (requireValidSwipeAreaStart &&
            swipeAreaCircle != null &&
            !swipeAreaCircle.IsScreenPointAllowed(screenPos))
        {
            return;
        }

        _currentSwipe = new RawSwipeData
        {
            StartRealtime = Time.realtimeSinceStartup
        };

        _currentSwipe.Points.Add(screenPos);

        IsSwiping = true;
        OnSwipeStarted?.Invoke();
        OnSwipeUpdated?.Invoke(_currentSwipe.Points);
    }

    private void ContinueSwipe(Vector2 screenPos)
    {
        if (!IsSwiping || _currentSwipe == null)
            return;

        if (_currentSwipe.Points.Count == 0)
        {
            _currentSwipe.Points.Add(screenPos);
            OnSwipeUpdated?.Invoke(_currentSwipe.Points);
            return;
        }

        float dist = Vector2.Distance(_currentSwipe.Points[_currentSwipe.Points.Count - 1], screenPos);
        if (dist < minPointDistancePixels)
            return;

        _currentSwipe.Points.Add(screenPos);
        OnSwipeUpdated?.Invoke(_currentSwipe.Points);
    }

    private void FinishSwipe(Vector2 screenPos)
    {
        if (!IsSwiping || _currentSwipe == null)
            return;

        ContinueSwipe(screenPos);

        _currentSwipe.EndRealtime = Time.realtimeSinceStartup;
        _currentSwipe.EndedByTimeout = false;

        RawSwipeData result = _currentSwipe.Clone();

        IsSwiping = false;
        IsListening = false;
        _currentSwipe = null;

        OnSwipeCompleted?.Invoke(result);
    }
}