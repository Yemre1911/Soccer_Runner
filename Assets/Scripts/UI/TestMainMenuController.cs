using UnityEngine;
using UnityEngine.UI;

public class TestMainMenuController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Toggles (all must belong to same ToggleGroup)")]
    [SerializeField] private Toggle easyToggle;
    [SerializeField] private Toggle mediumToggle;
    [SerializeField] private Toggle hardToggle;

    [Header("Play")]
    [SerializeField] private Button playButton;

    [Header("Test Config (common)")]
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private float levelLengthMeters = 500f;
    [SerializeField] private string environmentId = "default_env";
    [SerializeField] private string[] opponentJerseyIds = { "jersey_a", "jersey_b" };

    private void Awake()
    {
        if (playButton)
            playButton.onClick.AddListener(OnPlayClicked);

        // Varsayılan seçim
        if (easyToggle && mediumToggle && hardToggle)
        {
            if (!easyToggle.isOn && !mediumToggle.isOn && !hardToggle.isOn)
                easyToggle.isOn = true;
        }
    }

    private void OnDestroy()
    {
        if (playButton)
            playButton.onClick.RemoveListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        Difficulty diff = GetSelectedDifficulty();

        var config = new SessionConfig
        {
            levelNumber = levelNumber,
            levelLengthMeters = levelLengthMeters,
            difficulty = diff,
            environmentId = environmentId,
            opponentJerseyIds = opponentJerseyIds
        };

        Debug.Log($"[TestMainMenuController] Play -> Difficulty={diff}, Level={config.levelNumber}, Length={config.levelLengthMeters}");

        SessionBootstrap.LoadGameScene(gameSceneName, config);
    }

    private Difficulty GetSelectedDifficulty()
    {
        if (hardToggle && hardToggle.isOn) return Difficulty.Hard;
        if (mediumToggle && mediumToggle.isOn) return Difficulty.Medium;
        return Difficulty.Easy;
    }
}