using UnityEngine;

public class BallRainbowFreeRollState : IBallState
{
    private readonly Transform _playerReference;

    private PlayerLateralMovement _lateralMovement;

    private float _lockedX;
    private float _groundY;

    public BallRainbowFreeRollState(Transform playerReference)
    {
        _playerReference = playerReference;
    }

    public void OnEnter(BallController ball)
    {
        _lockedX = ball.transform.position.x;
        _groundY = ball.RainbowLandingY;

        if (ball.PlayerSocket != null)
            _lateralMovement = ball.PlayerSocket.GetComponentInParent<PlayerLateralMovement>();

        if (ball.FreeRollIndicator != null)
            ball.FreeRollIndicator.Show(ball.transform);
    }

    public void OnUpdate(BallController ball, float deltaTime)
    {
        if (ball.PlayerSocket == null)
            return;

        Vector3 pos = ball.transform.position;

        float targetZ = GetReturnZ(ball);

        pos.x = _lockedX;
        pos.y = _groundY;
        pos.z = Mathf.MoveTowards(
            pos.z,
            targetZ,
            ball.RainbowFreeRollZReturnSpeed * deltaTime
        );

        ball.transform.position = pos;

        ball.RotateVisualSpin(
            ball.RainbowFreeRollSpinSpeed,
            0f,
            deltaTime
        );

        if (CanRecoverDribble(ball))
        {
            ball.EnterDribble();
        }
    }

    public void OnExit(BallController ball)
    {

        if (ball.FreeRollIndicator != null)
            ball.FreeRollIndicator.Hide();
    }

    private float GetReturnZ(BallController ball)
    {
        // Sen "Z ekseni 0'a çekilsin" dedin.
        // Eğer PlayerSocket zaten Z=0 civarındaysa bu daha esnek olur.
        // Direkt 0 istersen return 0f yapabilirsin.
        return ball.PlayerSocket != null ? ball.PlayerSocket.position.z : 0f;
    }

    private bool CanRecoverDribble(BallController ball)
    {
        if (ball.PlayerSocket == null)
            return false;

        float playerX = GetPlayerX(ball);
        float playerZ = ball.PlayerSocket.position.z;

        Vector3 ballPos = ball.transform.position;

        float xDistance = Mathf.Abs(ballPos.x - playerX);
        float zDistance = Mathf.Abs(ballPos.z - playerZ);

        return
            xDistance <= ball.RainbowFreeRollRecoverXDistance &&
            zDistance <= ball.RainbowFreeRollRecoverZDistance;
    }

    private float GetPlayerX(BallController ball)
    {
        if (_lateralMovement != null)
            return _lateralMovement.TargetX;

        if (_playerReference != null)
            return _playerReference.position.x;

        if (ball.PlayerSocket != null)
            return ball.PlayerSocket.position.x;

        return ball.transform.position.x;
    }
}