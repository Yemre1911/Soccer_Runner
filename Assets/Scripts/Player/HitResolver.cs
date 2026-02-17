using UnityEngine;
using System;
public class HitResolver : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerCombatState playerCombat;
    [SerializeField] private BallController ball;
    public event Action OnGameOver;

    public bool IsGameOver { get; private set; }

    public void NotifyBallHit(OpponentIdentity opp)
    {
        if (IsGameOver) return;

        // Rakip topa değdi -> top rakibe geçsin
        if (ball != null && opp != null && opp.BallSocket != null)
        {
            ball.transform.SetParent(opp.BallSocket, true);
            ball.transform.localPosition = Vector3.zero;
            ball.transform.localRotation = Quaternion.identity;
        }
        Debug.Log("Top Çalındı");

        EndGame();
    }

    public void NotifyPlayerHit(OpponentIdentity opp)
    {
        if (IsGameOver) return;

        // Player state kuralını çalıştır
        playerCombat?.OnHitByOpponent();

        // Eğer bu hit düşmeye sebep olduysa oyunu bitir
        if (playerCombat != null && playerCombat.IsFallen)
        {
            Debug.Log("Çarpışma");
            // TODO: top serbest gitsin (bir sonraki adım)
            EndGame();
        }
    }

    private void EndGame()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        // TEST: dünya hızı 0
        if (GameManager.Instance != null)
            GameManager.Instance.SetSpeedMultiplier(0f);

        OnGameOver?.Invoke();
    }
}