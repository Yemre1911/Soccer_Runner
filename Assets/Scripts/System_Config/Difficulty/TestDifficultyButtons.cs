using UnityEngine;

public class TestDifficultyButtons : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Test Config (common)")]
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private float levelLengthMeters = 500f;
    [SerializeField] private string environmentId = "default_env";
    [SerializeField] private string[] opponentJerseyIds = { "jersey_a", "jersey_b" };

    // UI Button OnClick -> bunu çağır
    public void StartEasy()   => StartRun(Difficulty.Easy);
    public void StartMedium() => StartRun(Difficulty.Medium);
    public void StartHard()   => StartRun(Difficulty.Hard);

    private void StartRun(Difficulty diff)
    {
        Debug.Log($"[TestDifficultyButtons] StartRun -> {diff}");

        var config = new SessionConfig
        {
            levelNumber = levelNumber,
            levelLengthMeters = levelLengthMeters,
            difficulty = diff,
            environmentId = environmentId,
            opponentJerseyIds = opponentJerseyIds
        };

        SessionBootstrap.LoadGameScene(gameSceneName, config);
    }
}