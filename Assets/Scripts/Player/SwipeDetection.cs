using UnityEngine;

public class SwipeDetection : MonoBehaviour
{
    private Vector2 _startPosition;
    private float _startTime;

    [SerializeField] private float _swipeThreshold = 50f;
    [SerializeField] private float _longPressDuration = 0.5f;

    public System.Action<Vector2> OnSwipe;
    public System.Action<bool> OnLongPress;
    public System.Action OnAnyTouchBegan;

    private void Update()
    {
        if (Input.touchCount <= 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            _startPosition = touch.position;
            _startTime = Time.time;
            OnAnyTouchBegan?.Invoke();
        }

        if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
        {
            if (Time.time - _startTime > _longPressDuration)
                OnLongPress?.Invoke(true);
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            OnLongPress?.Invoke(false);

            Vector2 delta = touch.position - _startPosition;
            if (delta.magnitude > _swipeThreshold)
                OnSwipe?.Invoke(delta.normalized);
        }
    }
}