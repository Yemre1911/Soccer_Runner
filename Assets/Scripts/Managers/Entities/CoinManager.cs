using UnityEngine;
using System;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    public int Coins { get; private set; }
    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        Coins = 0;
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins);
    }

    public void ResetCoins()
    {
        Coins = 0;
        OnCoinsChanged?.Invoke(Coins);
    }
}