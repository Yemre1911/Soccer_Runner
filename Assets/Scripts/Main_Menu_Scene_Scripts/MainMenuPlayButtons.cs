using UnityEngine;

public class MainMenuPlayButtons : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Normal Run")]
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private float levelLengthMeters = 500f;
    [SerializeField] private string environmentId = "default_env";
    [SerializeField] private string[] opponentJerseyIds = { "jersey_a", "jersey_b" };

    public void StartEasy()   => StartNormal(Difficulty.Easy);
    public void StartMedium() => StartNormal(Difficulty.Medium);
    public void StartHard()   => StartNormal(Difficulty.Hard);

    public void StartShotTraining()
    {
        var config = SessionConfig.CreateShotTraining();
        SessionBootstrap.LoadGameScene(gameSceneName, config);
    }

    private void StartNormal(Difficulty diff)
    {
        var config = new SessionConfig
        {
            levelNumber = levelNumber,
            levelLengthMeters = levelLengthMeters,
            difficulty = diff,
            environmentId = environmentId,
            opponentJerseyIds = opponentJerseyIds,
            mode = SessionMode.NormalRun
        };

        SessionBootstrap.LoadGameScene(gameSceneName, config);
    }
}