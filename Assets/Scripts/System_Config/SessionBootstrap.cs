/*using UnityEngine;
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
}*/

using UnityEngine;
using UnityEngine.SceneManagement;

public static class SessionBootstrap
{
    public static SessionConfig PendingConfig { get; private set; }
    public static SessionConfig LastLoadedConfig { get; private set; }

    public static void LoadGameScene(string gameSceneName, SessionConfig config)
    {
        PendingConfig = config;
        LastLoadedConfig = config;
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    public static SessionConfig ConsumeConfig()
    {
        var c = PendingConfig;
        PendingConfig = null;
        return c;
    }

    public static void ReloadLastConfig(string gameSceneName)
    {
        if (LastLoadedConfig == null)
            LastLoadedConfig = SessionConfig.DefaultTest();

        PendingConfig = LastLoadedConfig;
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }
}