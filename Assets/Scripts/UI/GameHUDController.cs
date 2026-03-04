using TMPro;
using UnityEngine;

public class GameHUDController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text difficultyText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text goldText;

    private int _score;
    private int _gold;

    private void Awake()
    {
        RefreshAll();
    }

    public void SetDifficulty(Difficulty difficulty)
    {
        if (difficultyText)
            difficultyText.text = $"{difficulty}";
    }

    // Şimdilik test. Sonra ScoreSystem / Wallet eventlerinden çağıracağız.
    public void SetScore(int score)
    {
        _score = Mathf.Max(0, score);
        if (scoreText)
            scoreText.text = $"{_score}";
    }

    public void SetGold(int gold)
    {
        _gold = Mathf.Max(0, gold);
        if (goldText)
            goldText.text = $"{_gold}";
    }

    public void RefreshAll()
    {
        // Difficulty SessionLoader tarafından set edilecek
        if (scoreText) scoreText.text = $"{_score}";
        if (goldText) goldText.text = $"{_gold}";
    }
}