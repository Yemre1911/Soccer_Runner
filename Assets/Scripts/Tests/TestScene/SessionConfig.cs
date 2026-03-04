using System;
using UnityEngine;

[Serializable]
public class SessionConfig
{
    public int levelNumber = 1;
    public float levelLengthMeters = 5000f;

    public Difficulty difficulty = Difficulty.Easy;

    public string environmentId = "default_env";
    public string[] opponentJerseyIds = { "jersey_a", "jersey_b" };

    public static SessionConfig DefaultTest()
    {
        return new SessionConfig
        {
            levelNumber = 1,
            levelLengthMeters = 5000f,
            difficulty = Difficulty.Easy,
            environmentId = "default_env",
            opponentJerseyIds = new[] { "jersey_a", "jersey_b" }
        };
    }
}