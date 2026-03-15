/*using UnityEngine;
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
}*/

using UnityEngine;
using UnityEngine.UI;

public class BoosterUISlot : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image fillImage;

    [Header("Fill Color Gradient")]
    [SerializeField] private Gradient fillGradient;

    private string _boundItemId;

    public bool IsOccupied => !string.IsNullOrEmpty(_boundItemId);
    public string BoundItemId => _boundItemId;

    private void Reset()
    {
        SetupDefaultGradient();
    }

    private void Awake()
    {
        if (fillGradient == null || fillGradient.colorKeys == null || fillGradient.colorKeys.Length == 0)
            SetupDefaultGradient();

        Hide();
    }

    public void Bind(ActiveBoosterEntry entry)
    {
        if (entry == null || entry.itemDefinition == null) return;

        _boundItemId = string.IsNullOrWhiteSpace(entry.itemDefinition.itemId)
            ? entry.itemDefinition.name
            : entry.itemDefinition.itemId;

        if (root)
            root.SetActive(true);

        if (iconImage)
            iconImage.sprite = entry.itemDefinition.icon;

        SetFill(entry.NormalizedRemaining);
        ForceRefreshVisuals();
    }

    public void SetFill(float normalized)
    {
        float t = Mathf.Clamp01(normalized);

        if (fillImage)
        {
            fillImage.fillAmount = t;
            fillImage.color = fillGradient.Evaluate(t);
        }
    }

    public void Hide()
    {
        _boundItemId = null;

        if (root)
            root.SetActive(false);

        if (fillImage)
        {
            fillImage.fillAmount = 0f;
            fillImage.color = fillGradient.Evaluate(0f);
        }

        if (iconImage)
            iconImage.sprite = null;
    }

    public void ForceRefreshVisuals()
    {
        Canvas.ForceUpdateCanvases();

        RectTransform rt = transform as RectTransform;
        if (rt != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        if (root != null)
        {
            RectTransform rootRt = root.transform as RectTransform;
            if (rootRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rootRt);
        }

        Transform p = transform.parent;
        if (p != null)
        {
            RectTransform parentRt = p as RectTransform;
            if (parentRt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRt);
        }

        Canvas.ForceUpdateCanvases();
    }

    private void SetupDefaultGradient()
    {
        fillGradient = new Gradient();
        fillGradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.red, 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.33f), // turuncu
                new GradientColorKey(Color.yellow, 0.66f),
                new GradientColorKey(Color.green, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        );
    }
}