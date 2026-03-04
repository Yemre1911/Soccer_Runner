using UnityEngine;

[CreateAssetMenu(menuName = "SoccerRunner/Session Tuning Provider", fileName = "SessionTuningProvider")]
public class SessionTuningProviderSO : ScriptableObject, ISessionTuningProvider
{
    [Header("Difficulty -> Tuning")]
    public SessionTuning easy;
    public SessionTuning medium;
    public SessionTuning hard;

    public SessionTuning GetTuning(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Hard => hard ? hard : medium ? medium : easy,
            Difficulty.Medium => medium ? medium : easy,
            _ => easy
        };
    }
}