using UnityEngine;

public class PlayerVFXController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BoosterController boosterController;

    [Header("One-shot VFX")]
    [SerializeField] private ParticleSystem coinVFX;

    [Header("Loop VFX")]
    [SerializeField] private ParticleSystem magnetVFX;
    [SerializeField] private ParticleSystem shieldVFX;

    [Header("Booster Ids")]
    [SerializeField] private string magnetItemId = "magnet_booster";
    [SerializeField] private string shieldItemId = "booster_shield";

    private void Awake()
    {
        if (!boosterController)
            boosterController = GetComponentInParent<BoosterController>();

        StopAndHide(coinVFX);
        StopAndHide(magnetVFX);
        StopAndHide(shieldVFX);
    }

    private void OnEnable()
    {
        if (boosterController == null) return;

        boosterController.OnBoosterActivated += HandleBoosterChanged;
        boosterController.OnBoosterUpdated += HandleBoosterChanged;
        boosterController.OnBoosterRemoved += HandleBoosterChanged;
    }

    private void OnDisable()
    {
        if (boosterController == null) return;

        boosterController.OnBoosterActivated -= HandleBoosterChanged;
        boosterController.OnBoosterUpdated -= HandleBoosterChanged;
        boosterController.OnBoosterRemoved -= HandleBoosterChanged;
    }

    private void HandleBoosterChanged(ActiveBoosterEntry _)
    {
        RefreshLoopEffects();
    }

    private void RefreshLoopEffects()
    {
        bool magnetActive = IsBoosterActive(magnetItemId);
        bool shieldActive = IsBoosterActive(shieldItemId);

        SetLoopState(magnetVFX, magnetActive);
        SetLoopState(shieldVFX, shieldActive);
    }

    public void PlayCoinVFX()
    {
        if (!coinVFX) return;

        coinVFX.gameObject.SetActive(true);
        coinVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        coinVFX.Play(true);
    }

    private bool IsBoosterActive(string itemId)
    {
        if (boosterController == null || string.IsNullOrWhiteSpace(itemId))
            return false;

        var active = boosterController.ActiveBoosters;
        for (int i = 0; i < active.Count; i++)
        {
            ActiveBoosterEntry entry = active[i];
            if (entry == null || entry.itemDefinition == null) continue;
            if (entry.ItemId == itemId) return true;
        }

        return false;
    }

    private static void SetLoopState(ParticleSystem ps, bool active)
    {
        if (!ps) return;

        if (active)
        {
            if (!ps.gameObject.activeSelf)
                ps.gameObject.SetActive(true);

            if (!ps.isPlaying)
                ps.Play(true);
        }
        else
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.gameObject.SetActive(false);
        }
    }

    private static void StopAndHide(ParticleSystem ps)
    {
        if (!ps) return;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
    }
}