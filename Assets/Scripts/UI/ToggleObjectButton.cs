using UnityEngine;

public class ToggleObjectButton : MonoBehaviour
{
    [SerializeField] private GameObject target;

    public void Toggle()
    {
        if (!target)
        {
            Debug.LogWarning("[ToggleObjectButton] Target is null.");
            return;
        }

        target.SetActive(!target.activeSelf);
    }
}