using UnityEngine;

public class BallIdleState : IBallState
{
    public void OnEnter(BallController ball)
    {
        ball.SnapToSocketImmediate();
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        ball.SnapToSocketImmediate();
    }

    public void OnExit(BallController ball)
    {
    }
}