using System.Collections;
using UnityEngine;

public class CurveChangeController : MonoBehaviour
{
    [Header("Shader Property Names")]
    [SerializeField] private string backwardsPropertyName = "_BackWardsStrenght";
    [SerializeField] private string sidewaysPropertyName = "_SideWayStrenght";

    [Header("Default / Restore Values")]
    [SerializeField] private float defaultBackwardsStrength = -0.002f;
    [SerializeField] private float defaultSidewaysStrength = 0.0005f;

    [Header("Blend")]
    [SerializeField] private float blendDuration = 1.2f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Optional Controllers To Disable")]
    [SerializeField] private MonoBehaviour[] controllersToDisable;

    private int _backwardsId;
    private int _sidewaysId;
    private Coroutine _routine;

    private void Awake()
    {
        _backwardsId = Shader.PropertyToID(backwardsPropertyName);
        _sidewaysId = Shader.PropertyToID(sidewaysPropertyName);
    }

    public void ApplyDefaultsImmediate()
    {
        Shader.SetGlobalFloat(_backwardsId, defaultBackwardsStrength);
        Shader.SetGlobalFloat(_sidewaysId, defaultSidewaysStrength);
    }

    public void SmoothToZero()
    {
        SetControllersEnabled(false);
        StartBlend(0f, 0f);
    }

    public void RestoreDefaults()
    {
        StartBlend(defaultBackwardsStrength, defaultSidewaysStrength);
    }

    public void RestoreDefaultsImmediate()
    {
        Shader.SetGlobalFloat(_backwardsId, defaultBackwardsStrength);
        Shader.SetGlobalFloat(_sidewaysId, defaultSidewaysStrength);
        SetControllersEnabled(true);
    }

    private void StartBlend(float targetBackwards, float targetSideways)
    {
        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(BlendRoutine(targetBackwards, targetSideways));
    }

    private IEnumerator BlendRoutine(float targetBackwards, float targetSideways)
    {
        float startBackwards = Shader.GetGlobalFloat(_backwardsId);
        float startSideways = Shader.GetGlobalFloat(_sidewaysId);

        float time = 0f;
        float duration = Mathf.Max(0.0001f, blendDuration);

        while (time < duration)
        {
            time += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            Shader.SetGlobalFloat(_backwardsId, Mathf.Lerp(startBackwards, targetBackwards, eased));
            Shader.SetGlobalFloat(_sidewaysId, Mathf.Lerp(startSideways, targetSideways, eased));

            yield return null;
        }

        Shader.SetGlobalFloat(_backwardsId, targetBackwards);
        Shader.SetGlobalFloat(_sidewaysId, targetSideways);

        // restore işlemi bittiyse diğer controller'ları tekrar aç
        bool restored =
            Mathf.Approximately(targetBackwards, defaultBackwardsStrength) &&
            Mathf.Approximately(targetSideways, defaultSidewaysStrength);

        if (restored)
            SetControllersEnabled(true);

        _routine = null;
    }

    private void SetControllersEnabled(bool enabledState)
    {
        if (controllersToDisable == null) return;

        for (int i = 0; i < controllersToDisable.Length; i++)
        {
            if (controllersToDisable[i] != null)
                controllersToDisable[i].enabled = enabledState;
        }
    }
}