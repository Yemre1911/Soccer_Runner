using UnityEngine;

public static class AudienceAttackRules
{
    /// <summary>
    /// waveIndex: 1-based
    /// totalWaves: >= 1
    /// </summary>
    public static bool ShouldBeActiveOnWave(Difficulty difficulty, int waveIndex, int totalWaves)
    {
        if (difficulty == Difficulty.Easy) return false;

        if (totalWaves <= 0) totalWaves = 1;
        waveIndex = Mathf.Clamp(waveIndex, 1, totalWaves);

        return difficulty switch
        {
            Difficulty.Medium => (waveIndex == totalWaves),
            Difficulty.Hard => (waveIndex >= Mathf.Max(1, totalWaves - 1)), // son 2 wave
            _ => false
        };
    }
}