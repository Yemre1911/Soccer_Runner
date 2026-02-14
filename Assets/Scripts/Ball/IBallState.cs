using UnityEngine;

public interface IBallState
{
    // Duruma girdiğimizde bir kez çalışır (Örn: Ses çalma, efekt patlatma)
    void OnEnter(BallController ball);

    // Her karede çalışır (Hareket mantığı buradadır)
    void OnUpdate(BallController ball, float deltaTime);

    // Durumdan çıkarken çalışır
    void OnExit(BallController ball);
}