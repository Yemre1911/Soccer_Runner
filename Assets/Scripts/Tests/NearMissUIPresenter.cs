using UnityEngine;

public class NearMissUIPresenter : MonoBehaviour
{
    [SerializeField] private PlayerNearMissReceiver receiver;
    [SerializeField] private GameObject singlePopup;
    [SerializeField] private GameObject doublePopup;

    private void Awake()
    {
        if (receiver == null)
        {
#if UNITY_2022_1_OR_NEWER
            receiver = FindFirstObjectByType<PlayerNearMissReceiver>();
#else
            receiver = FindObjectOfType<PlayerNearMissReceiver>();
#endif
        }
    }

    private void OnEnable()
    {
        if (receiver != null) receiver.OnNearMiss += HandleNearMiss;
    }

    private void OnDisable()
    {
        if (receiver != null) receiver.OnNearMiss -= HandleNearMiss;
    }

    private void HandleNearMiss(NearMissEvent e)
    {
        if (e.Kind == NearMissKind.Double) Play(doublePopup);
        else Play(singlePopup);
    }

    private void Play(GameObject go)
    {
        if (go == null) return;

        go.SetActive(false);
        go.SetActive(true);

        var anim = go.GetComponent<Animator>();
        if (anim != null) anim.Play(0, 0, 0f);
    }
}