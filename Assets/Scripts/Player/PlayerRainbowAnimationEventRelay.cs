using UnityEngine;

public class PlayerRainbowAnimationEventRelay : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlayerRainbowMove rainbowMove;

    public void OnRainbowBallFlick()
    {
        if (rainbowMove != null)
            rainbowMove.OnRainbowBallFlick();
        else
            Debug.LogWarning("[PlayerRainbowAnimationEventRelay] PlayerRainbowMove reference is missing.", this);
    }

    public void OnRainbowFinished()
    {
        if (rainbowMove != null)
            rainbowMove.OnRainbowFinished();
        else
            Debug.LogWarning("[PlayerRainbowAnimationEventRelay] PlayerRainbowMove reference is missing.", this);
    }
}