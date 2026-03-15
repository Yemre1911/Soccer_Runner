using UnityEngine;

public class HitResolver : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerCombatState playerCombat;
    [SerializeField] private BallController ball;
    [SerializeField] private SimpleCameraShake cameraShake;

    public void NotifyBallHit(OpponentIdentity opp)
    {
        if (GameOver.Instance.IsGameOver) return;

        if (cameraShake != null) cameraShake.TriggerShake(0.5f, 0.5f);

        if (ball != null && opp != null && opp.BallSocket != null)
        {
            ball.transform.SetParent(opp.BallSocket, true);
            ball.transform.localPosition = Vector3.zero;
        }

        GameOver.Instance.TriggerGameOver();
    }

    public void NotifyPlayerHit(OpponentIdentity opp)
    {
        if (GameOver.Instance.IsGameOver) return;

        playerCombat?.OnHitByOpponent();

        if (playerCombat != null && playerCombat.IsFallen)
        {
            if (cameraShake != null) cameraShake.TriggerShake(0.3f, 0.5f);
            GameOver.Instance.TriggerGameOver();
        }
    }

    public void NotifyGroundHazardHit()
    {
        if (GameOver.Instance.IsGameOver) return;

        playerCombat?.OnHitByOpponent();

        if (playerCombat != null && playerCombat.IsFallen)
        {
            if (cameraShake != null) cameraShake.TriggerShake(0.3f, 0.5f);
            GameOver.Instance.TriggerGameOver();
        }
    }
}