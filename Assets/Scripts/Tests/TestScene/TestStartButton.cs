using UnityEngine;

public class TestStartButton : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Test Config")]
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private float levelLengthMeters = 500f;
    [SerializeField] private int difficulty = 1;
    [SerializeField] private string environmentId = "default_env";
    [SerializeField] private string[] opponentJerseyIds = { "jersey_a", "jersey_b" };

    public void StartRun()
    {
        Debug.Log("[TestStartButton] StartRun clicked!");

        var config = new SessionConfig
        {
            levelNumber = levelNumber,
            levelLengthMeters = levelLengthMeters,
            difficulty = difficulty,
            environmentId = environmentId,
            opponentJerseyIds = opponentJerseyIds
        };

        SessionBootstrap.LoadGameScene(gameSceneName, config);
    }
}