/*using UnityEngine;

public class HitResolver : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerCombatState playerCombat;
    [SerializeField] private BallController ball;
    [SerializeField] private SimpleCameraShake cameraShake;

    public void NotifyBallHit(OpponentIdentity opp)
    {
        if (GameOver.Instance.IsGameOver) return;

        // Kamera sarsıntısı (GameFeel)
        if (cameraShake != null) cameraShake.TriggerShake(0.5f, 0.5f);

        // Topu rakibe ver (Görsel efekt için geçici süreliğine)
        // GameOver çalışınca top zaten Destroy olacak ama o milisaniyede görsel kopmasın.
        if (ball != null && opp != null && opp.BallSocket != null)
        {
            ball.transform.SetParent(opp.BallSocket, true);
            ball.transform.localPosition = Vector3.zero;
        }

        // FİŞİ ÇEK
        GameOver.Instance.TriggerGameOver();
    }

    public void NotifyPlayerHit(OpponentIdentity opp)
    {
        if (GameOver.Instance.IsGameOver) return;

        playerCombat?.OnHitByOpponent();

        if (playerCombat != null && playerCombat.IsFallen)
        {
            if (cameraShake != null) cameraShake.TriggerShake(0.3f, 0.5f);

            // FİŞİ ÇEK
            GameOver.Instance.TriggerGameOver();
        }
    }
}*/

using UnityEngine;

public class HitResolver : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerCombatState playerCombat;
    [SerializeField] private BallController ball;
    [SerializeField] private SimpleCameraShake cameraShake;

    [Header("Runtime")]
    [SerializeField] private bool shieldActive;

    public bool ShieldActive => shieldActive;

    public void SetShieldActive(bool active)
    {
        shieldActive = active;
        Debug.Log($"[HitResolver] Shield active -> {shieldActive}", this);
    }

    public void NotifyBallHit(OpponentIdentity opp)
    {
        if (GameOver.Instance.IsGameOver) return;
        if (shieldActive) return;

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
        if (shieldActive) return;

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
        if (shieldActive) return;

        playerCombat?.OnHitByOpponent();

        if (playerCombat != null && playerCombat.IsFallen)
        {
            if (cameraShake != null) cameraShake.TriggerShake(0.3f, 0.5f);
            GameOver.Instance.TriggerGameOver();
        }
    }
}