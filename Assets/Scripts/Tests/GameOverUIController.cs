using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUIController : MonoBehaviour
{
    [Header("UI Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Scenes")]
    [SerializeField] private string homeSceneName = "TestHomeScene";

    private bool _subscribed;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;

        if (GameOver.Instance != null)
        {
            GameOver.Instance.OnGameLost += HandleGameLost;
            _subscribed = true;

            // Eğer script enable olduğunda zaten gameover ise paneli aç
            if (GameOver.Instance.IsGameOver)
                HandleGameLost();
        }
    }

    private void Unsubscribe()
    {
        if (!_subscribed) return;

        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost -= HandleGameLost;

        _subscribed = false;
    }

    private void HandleGameLost()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }

    // --- UI Buttons ---

    public void OnHomeClicked()
    {
        SceneManager.LoadScene(homeSceneName, LoadSceneMode.Single);
    }

    public void OnContinueClicked()
    {
        Debug.Log("[GameOverUIController] Continue clicked (TODO).");
        // Şimdilik boş
    }
}