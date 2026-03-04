using UnityEngine;

public class HUDRuntimeBinder : MonoBehaviour
{
    [Header("Refs (optional, auto-find if null)")]
    [SerializeField] private GameHUDController hud;
    [SerializeField] private CoinManager coinManager;

    [Header("Score")]
    [Tooltip("Score = RunDistance / scoreDivisor")]
    [SerializeField] private float scoreDivisor = 10f;

    private int _lastScore = -1;

    private void Awake()
    {
        if (!hud) hud = FindFirstObjectByType<GameHUDController>();
        if (!coinManager) coinManager = CoinManager.Instance ? CoinManager.Instance : FindFirstObjectByType<CoinManager>();
    }

    private void OnEnable()
    {
        if (coinManager != null)
            coinManager.OnCoinsChanged += HandleCoinsChanged;
    }

    private void OnDisable()
    {
        if (coinManager != null)
            coinManager.OnCoinsChanged -= HandleCoinsChanged;
    }

    private void Start()
    {
        // Başlangıçta UI'ı doğru state'e çek
        if (hud != null)
        {
            if (coinManager != null) hud.SetGold(coinManager.Coins);
            hud.SetScore(0);
        }
    }

    private void Update()
    {
        if (hud == null) return;
        if (GameManager.Instance == null) return;

        float d = GameManager.Instance.RunDistance;

        int score = (scoreDivisor <= 0.0001f) ? Mathf.FloorToInt(d) : Mathf.FloorToInt(d / scoreDivisor);

        // Gereksiz UI set spam'ini azalt
        if (score != _lastScore)
        {
            _lastScore = score;
            hud.SetScore(score);
        }
    }

    private void HandleCoinsChanged(int totalCoins)
    {
        if (hud == null) return;
        hud.SetGold(totalCoins);
    }
}