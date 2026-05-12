using UnityEngine;

public class BallIdleState : IBallState
{
    private readonly bool _keepSpin;
    private Vector3 _followVelocity;

    public BallIdleState(bool keepSpin)
    {
        _keepSpin = keepSpin;
    }

    public void OnEnter(BallController ball)
    {
        _followVelocity = Vector3.zero;
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        if (ball.PlayerSocket == null) return;

        Vector3 targetPosition = ball.PlayerSocket.position;

        ball.transform.position = Vector3.SmoothDamp(
            ball.transform.position,
            targetPosition,
            ref _followVelocity,
            ball.SocketSmoothTime,
            ball.SocketMaxSpeed,
            deltaTime
        );

        if ((ball.transform.position - targetPosition).sqrMagnitude <= ball.SocketSnapDistance * ball.SocketSnapDistance)
        {
            ball.transform.position = targetPosition;
        }

        if (_keepSpin)
        {
            ball.RotateVisualSpin(ball.ShotIdleSpinSpeed, 0f, deltaTime);
        }
    }

    public void OnExit(BallController ball)
    {
    }
}