/*using UnityEngine;

public class PlayerEffectContext : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private MagnetBoostTarget magnetTarget;
    [SerializeField] private JumpBoostTarget jumpTarget;

    public MagnetBoostTarget GetMagnetTarget()
    {
        if (!magnetTarget)
            magnetTarget = GetComponentInChildren<MagnetBoostTarget>(true);

        return magnetTarget;
    }

    public JumpBoostTarget GetJumpTarget()
    {
        if (!jumpTarget)
            jumpTarget = GetComponentInChildren<JumpBoostTarget>(true);

        return jumpTarget;
    }
}*/

using UnityEngine;

public class PlayerEffectContext : MonoBehaviour
{
    [Header("Gameplay Targets")]
    [SerializeField] private MagnetBoostTarget magnetTarget;
    [SerializeField] private JumpBoostTarget jumpTarget;
    [SerializeField] private ShieldBoostTarget shieldTarget;
    [SerializeField] private ScoreMultiplierTarget scoreMultiplierTarget;

    [Header("Visual Targets")]
    [SerializeField] private MagnetVisualTarget magnetVisualTarget;
    [SerializeField] private JumpVisualTarget jumpVisualTarget;

    public MagnetBoostTarget GetMagnetTarget()
    {
        if (!magnetTarget)
            magnetTarget = GetComponentInChildren<MagnetBoostTarget>(true);
        return magnetTarget;
    }

    public JumpBoostTarget GetJumpTarget()
    {
        if (!jumpTarget)
            jumpTarget = GetComponentInChildren<JumpBoostTarget>(true);
        return jumpTarget;
    }

    public ShieldBoostTarget GetShieldTarget()
    {
        if (!shieldTarget)
            shieldTarget = GetComponentInChildren<ShieldBoostTarget>(true);
        return shieldTarget;
    }

    public ScoreMultiplierTarget GetScoreMultiplierTarget()
    {
        if (!scoreMultiplierTarget)
            scoreMultiplierTarget = GetComponentInChildren<ScoreMultiplierTarget>(true);
        return scoreMultiplierTarget;
    }

    public MagnetVisualTarget GetMagnetVisualTarget()
    {
        if (!magnetVisualTarget)
            magnetVisualTarget = GetComponentInChildren<MagnetVisualTarget>(true);
        return magnetVisualTarget;
    }

    public JumpVisualTarget GetJumpVisualTarget()
    {
        if (!jumpVisualTarget)
            jumpVisualTarget = GetComponentInChildren<JumpVisualTarget>(true);
        return jumpVisualTarget;
    }
}