/*using UnityEngine;
using System;

public class GameOver : MonoBehaviour
{
    public static GameOver Instance { get; private set; }

    [Header("Yönetilen Objeler")]
    [SerializeField] private BallController activeBall; // Sahnedeki topu buraya sürükle

    // Diğer scriptlerin dinleyeceği ana event
    public event Action OnGameLost;
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Oyunu kaybetme sürecini başlatır.
    /// HitResolver veya düşme algılayıcılar bunu çağırır.
    /// </summary>
    public void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        Debug.Log("Game Over State Started");
        Debug.Log("timeScale: " + Time.timeScale);


        // 1. Dünya Hızını Durdur
        if (GameManager.Instance != null)
            GameManager.Instance.SetSpeedMultiplier(0f);

        // 2. Topu Yok Et (Pooling sorunu kalmaz, kökten çözüm)
        if (activeBall != null)
        {
            Destroy(activeBall.gameObject);
        }

        // 3. Abonelere Haber Ver (UI, Input, Ses vb.)
        OnGameLost?.Invoke();
    }

}*/

using UnityEngine;
using System;

public class GameOver : MonoBehaviour
{
    public static GameOver Instance { get; private set; }

    [Header("Yönetilen Objeler")]
    [SerializeField] private BallController activeBall; // Sahnedeki topu buraya sürükle

    [Header("Gecikme")]
    [SerializeField] private float stopWorldDelay = 1.5f; // 1 saniye

    public event Action OnGameLost;
    public bool IsGameOver { get; private set; }

    private Coroutine _gameOverRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        Debug.Log("Game Over State Started");
        Debug.Log("timeScale: " + Time.timeScale);
        // 3. Abonelere Haber Ver
        OnGameLost?.Invoke();
        // Tek sefer çalışsın diye
        if (_gameOverRoutine != null) StopCoroutine(_gameOverRoutine);
        _gameOverRoutine = StartCoroutine(GameOverFlow());
    }

    private System.Collections.IEnumerator GameOverFlow()
    {
        // 1 saniye bekle (Time.timeScale 0 değilse normal akar)
        yield return new WaitForSeconds(stopWorldDelay);

        // 1. Dünya Hızını Durdur
        if (GameManager.Instance != null)
            GameManager.Instance.SetSpeedMultiplier(0f);

        // 2. Topu Yok Et
        if (activeBall != null)
            Destroy(activeBall.gameObject);


    }
}
