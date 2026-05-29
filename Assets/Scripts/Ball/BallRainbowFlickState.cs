using UnityEngine;

public class BallRainbowFlickState : IBallState
{
    private enum Phase
    {
        MoveToPrep,
        FlickFlight
    }

    private readonly Transform _playerReference;

    private Phase _phase;

    private Vector3 _prepStartPoint;
    private Vector3 _prepTargetPoint;

    private Vector3 _flightStartPoint;
    private Vector3 _controlPointA;
    private Vector3 _controlPointB;
    private Vector3 _endPoint;

    private float _timer;

    public BallRainbowFlickState(Transform playerReference)
    {
        _playerReference = playerReference;
    }

    public void OnEnter(BallController ball)
    {
        _timer = 0f;

        if (ball.RainbowPrepPoint != null)
        {
            _phase = Phase.MoveToPrep;
            _prepStartPoint = ball.transform.position;
            _prepTargetPoint = ball.RainbowPrepPoint.position;
        }
        else
        {
            // Prep point verilmemişse eski davranışa benzer şekilde direkt flight başlat.
            BeginFlight(ball, ball.transform.position);
        }
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        switch (_phase)
        {
            case Phase.MoveToPrep:
                UpdateMoveToPrep(ball, deltaTime);
                break;

            case Phase.FlickFlight:
                UpdateFlickFlight(ball, deltaTime);
                break;
        }
    }

    public void OnExit(BallController ball)
    {
    }

    private void UpdateMoveToPrep(BallController ball, float deltaTime)
    {
        float duration = Mathf.Max(0.01f, ball.RainbowPrepDuration);

        _timer += deltaTime;

        float t = Mathf.Clamp01(_timer / duration);
        float easedT = ball.RainbowUseSmoothPrep ? SmoothStep01(t) : t;

        // PrepPoint animasyon sırasında karakterle birlikte hareket edebilir.
        // Bu yüzden hedefi her frame güncelliyoruz.
        if (ball.RainbowPrepPoint != null)
            _prepTargetPoint = ball.RainbowPrepPoint.position;

        ball.transform.position = Vector3.Lerp(_prepStartPoint, _prepTargetPoint, easedT);

        ball.RotateVisualSpin(
            ball.RainbowForwardSpinSpeed * 0.45f,
            ball.RainbowCurveSpinSpeed * 0.25f,
            deltaTime
        );

        if (t >= 1f)
        {
            BeginFlight(ball, _prepTargetPoint);
        }
    }

    private void BeginFlight(BallController ball, Vector3 startPoint)
    {
        _phase = Phase.FlickFlight;
        _timer = 0f;

        _flightStartPoint = startPoint;

        Vector3 playerPos = GetPlayerPosition(ball);
        Vector3 forward = GetStableForward();

        _controlPointA = _flightStartPoint + Vector3.up * (ball.RainbowPeakHeight * 0.75f);

        _controlPointB = playerPos
            + forward * (ball.RainbowForwardOffset * 0.55f)
            + Vector3.up * ball.RainbowPeakHeight;

        _endPoint = playerPos + forward * ball.RainbowForwardOffset;
        _endPoint.y = ball.RainbowLandingY;
    }

    private void UpdateFlickFlight(BallController ball, float deltaTime)
    {
        float duration = Mathf.Max(0.01f, ball.RainbowFlightDuration);

        _timer += deltaTime;

        float t = Mathf.Clamp01(_timer / duration);
        float easedT = EaseOutCubic(t);

        Vector3 position = CubicBezier(
            _flightStartPoint,
            _controlPointA,
            _controlPointB,
            _endPoint,
            easedT
        );

        ball.transform.position = position;

        float spinMultiplier = Mathf.Lerp(1.25f, 0.75f, t);

        ball.RotateVisualSpin(
            ball.RainbowForwardSpinSpeed * spinMultiplier,
            ball.RainbowCurveSpinSpeed,
            deltaTime
        );

        if (t >= 1f)
        {
            if (ball.RainbowUseFreeRollAfterFlight)
            {
                ball.EnterRainbowFreeRoll(_playerReference);
                return;
            }

            if (ball.RainbowReturnToDribble)
                ball.EnterDribble();
            else
                ball.EnterIdle(true);
        }
    }

    private Vector3 GetPlayerPosition(BallController ball)
    {
        if (_playerReference != null)
            return _playerReference.position;

        if (ball.PlayerSocket != null)
            return ball.PlayerSocket.position;

        return ball.transform.position;
    }

    private Vector3 GetStableForward()
    {
        // Senin oyunda ileri yön top/dribble tarafında +Z mantığıyla kurulmuş.
        // Bu yüzden ilk versiyonda en stabil seçenek Vector3.forward.
        return Vector3.forward;
    }

    private static Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;

        return
            u * u * u * p0 +
            3f * u * u * t * p1 +
            3f * u * t * t * p2 +
            t * t * t * p3;
    }

    private static float SmoothStep01(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private static float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}