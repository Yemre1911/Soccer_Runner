/*using UnityEngine;

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

    [Tooltip("Rainbow gibi özel hareketlerde sadece karakter hitlerini geçici olarak görmezden gelir. Top hitleri etkilenmez.")]
    [SerializeField] private bool playerHitIgnored;

    public bool ShieldActive => shieldActive;
    public bool PlayerHitIgnored => playerHitIgnored;

    public void SetShieldActive(bool active)
    {
        shieldActive = active;
        Debug.Log($"[HitResolver] Shield active -> {shieldActive}", this);
    }

    public void SetPlayerHitIgnored(bool ignored)
    {
        playerHitIgnored = ignored;
        Debug.Log($"[HitResolver] Player hit ignored -> {playerHitIgnored}", this);
    }

    public void NotifyBallHit(OpponentIdentity opp)
    {
        if (GameOver.Instance.IsGameOver) return;
        if (shieldActive) return;

        if (cameraShake != null)
            cameraShake.TriggerShake(0.5f, 0.5f);

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
        if (playerHitIgnored) return;

        playerCombat?.OnHitByOpponent();

        if (playerCombat != null && playerCombat.IsFallen)
        {
            if (cameraShake != null)
                cameraShake.TriggerShake(0.3f, 0.5f);

            GameOver.Instance.TriggerGameOver();
        }
    }

    public void NotifyGroundHazardHit()
    {
        if (GameOver.Instance.IsGameOver) return;
        if (shieldActive) return;
        if (playerHitIgnored) return;

        playerCombat?.OnHitByOpponent();

        if (playerCombat != null && playerCombat.IsFallen)
        {
            if (cameraShake != null)
                cameraShake.TriggerShake(0.3f, 0.5f);

            GameOver.Instance.TriggerGameOver();
        }
    }
}