using System;
using UnityEngine;

[Serializable]
public class SessionConfig
{
    [Header("Level")]
    public int levelNumber = 1;
    public float levelLengthMeters = 5000f;   // süre yerine mesafe mantığı

    [Header("Difficulty")]
    [Range(0, 10)] public int difficulty = 1;

    [Header("Environment / Visual IDs (şimdilik string)")]
    public string environmentId = "default_env";
    public string[] opponentJerseyIds = { "jersey_a", "jersey_b" };

    public static SessionConfig DefaultTest()
    {
        return new SessionConfig
        {
            levelNumber = 1,
            levelLengthMeters = 5000f,
            difficulty = 1,
            environmentId = "default_env",
            opponentJerseyIds = new[] { "jersey_a", "jersey_b" }
        };
    }
}