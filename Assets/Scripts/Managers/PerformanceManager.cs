using UnityEngine;
public class PerformanceManager : MonoBehaviour
{
    private void Awake()
    {
        // 1. Önce V-Sync'i kapatıyoruz ki kontrol bize geçsin
        QualitySettings.vSyncCount = 0; 
        
        // 2. Hedef FPS'i belirliyoruz
        Application.targetFrameRate = 60; 
    }
}