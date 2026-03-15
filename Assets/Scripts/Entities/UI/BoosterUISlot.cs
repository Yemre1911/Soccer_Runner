using UnityEngine;
using UnityEngine.UI;

public class BoosterUISlot : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image fillImage;

    private string _boundItemId;

    public bool IsOccupied => !string.IsNullOrEmpty(_boundItemId);
    public string BoundItemId => _boundItemId;

    private void Awake()
    {
        Hide();
    }

    public void Bind(ActiveBoosterEntry entry)
    {
        if (entry == null || entry.itemDefinition == null) return;

        _boundItemId = string.IsNullOrWhiteSpace(entry.itemDefinition.itemId)
            ? entry.itemDefinition.name
            : entry.itemDefinition.itemId;

        if (root) root.SetActive(true);

        if (iconImage)
            iconImage.sprite = entry.itemDefinition.icon;

        SetFill(entry.NormalizedRemaining);
    }

    public void SetFill(float normalized)
    {
        if (fillImage)
            fillImage.fillAmount = Mathf.Clamp01(normalized);
    }

    public void Hide()
    {
        _boundItemId = null;

        if (root) root.SetActive(false);

        if (fillImage)
            fillImage.fillAmount = 0f;

        if (iconImage)
            iconImage.sprite = null;
    }
}