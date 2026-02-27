using UnityEngine;
using UnityEngine.SceneManagement;

public static class SessionBootstrap
{
    // GameScene açılınca SessionLoader buradan çekecek
    public static SessionConfig PendingConfig { get; private set; }

    public static void LoadGameScene(string gameSceneName, SessionConfig config)
    {
        PendingConfig = config;
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    // Config tüketilince temizlemek iyi olur
    public static SessionConfig ConsumeConfig()
    {
        var c = PendingConfig;
        PendingConfig = null;
        return c;
    }
}