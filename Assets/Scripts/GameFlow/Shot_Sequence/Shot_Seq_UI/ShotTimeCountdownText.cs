using UnityEngine;
using TMPro;

public class ShotTimeCountdownText : MonoBehaviour
{
    [SerializeField] private ShotTimeController shotTimeController;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private string prefix = "Time: ";
    [SerializeField] private bool showDecimals = false;

    private void Awake()
    {
        if (!shotTimeController)
            shotTimeController = FindFirstObjectByType<ShotTimeController>();

        if (!timeText)
            timeText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (shotTimeController == null)
            return;

        shotTimeController.OnCountdownTick += HandleCountdownTick;
        shotTimeController.OnCountdownValueChanged += HandleCountdownValueChanged;
        shotTimeController.OnFlowCompleted += HandleFlowCompleted;
        shotTimeController.OnSlowmoStarted += HandleSlowmoStarted;
    }

    private void OnDisable()
    {
        if (shotTimeController == null)
            return;

        shotTimeController.OnCountdownTick -= HandleCountdownTick;
        shotTimeController.OnCountdownValueChanged -= HandleCountdownValueChanged;
        shotTimeController.OnFlowCompleted -= HandleFlowCompleted;
        shotTimeController.OnSlowmoStarted -= HandleSlowmoStarted;
    }

    private void HandleSlowmoStarted()
    {
        SetText(prefix + "...");
    }

    private void HandleCountdownTick(int seconds)
    {
        if (!showDecimals)
            SetText(prefix + seconds.ToString());
    }

    private void HandleCountdownValueChanged(float remaining)
    {
        if (!showDecimals)
            return;

        SetText(prefix + remaining.ToString("F1"));
    }

    private void HandleFlowCompleted()
    {
        SetText(prefix + "0");
    }

    private void SetText(string value)
    {
        if (timeText != null)
            timeText.text = value;
    }
}