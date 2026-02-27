using UnityEngine;

public class SessionDebugHUD : MonoBehaviour
{
    [Header("Auto Find")]
    [SerializeField] private SessionDirector director;
    [SerializeField] private OpponentSpawner spawner;

    [Header("UI")]
    [SerializeField] private bool show = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    [Header("Scale")]
    [Tooltip("Açıkken ekran çözünürlüğüne göre otomatik büyütür.")]
    [SerializeField] private bool autoScale = true;

    [Tooltip("Manuel büyütme. autoScale kapalıysa direkt bunu kullanır.")]
    [SerializeField] private float uiScale = 2.0f;

    [Header("Layout")]
    [SerializeField] private Vector2 padding = new Vector2(12f, 12f);
    [SerializeField] private float width = 560f;

    private GUIStyle _style;
    private GUIStyle _titleStyle;
    private bool _stylesReady;

    private void Awake()
    {
        if (!director) director = FindFirstObjectByType<SessionDirector>();
        if (!spawner) spawner = FindFirstObjectByType<OpponentSpawner>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            show = !show;
    }

    private void EnsureStyles()
    {
        if (_stylesReady) return;

        _style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            richText = true,
            wordWrap = true
        };

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            richText = true,
            fontStyle = FontStyle.Bold
        };

        _stylesReady = true;
    }

    private float GetScale()
    {
        if (!autoScale) return Mathf.Max(0.5f, uiScale);

        // 1080p referans: daha büyük ekranlarda otomatik büyüt
        float refH = 1080f;
        float s = Screen.height / refH;

        // çok uçmasın
        s = Mathf.Clamp(s, 1.0f, 2.6f);

        // üstüne küçük bir boost (istersen kaldır)
        s *= 1.35f;

        return s;
    }

    private void OnGUI()
    {
        if (!show) return;

        EnsureStyles();

        float scale = GetScale();
        Matrix4x4 oldMatrix = GUI.matrix;

        // Her şeyi ölçekle
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        float x = padding.x;
        float y = padding.y;

        GUILayout.BeginArea(new Rect(x, y, width, 9999f), GUI.skin.box);

        GUILayout.Label("<b>SESSION DEBUG HUD</b>", _titleStyle);
        GUILayout.Space(8);

        // GameManager
        if (GameManager.Instance != null)
        {
            var gm = GameManager.Instance;
            GUILayout.Label($"<b>RunDistance:</b> {gm.RunDistance:0.0} m", _style);
            GUILayout.Label($"<b>BaseSpeed:</b> {gm.BaseSpeed:0.00}", _style);
            GUILayout.Label($"<b>SpeedMultiplier:</b> {gm.SpeedMultiplier:0.00}", _style);
            GUILayout.Label($"<b>CurrentSpeed (cap):</b> {gm.CurrentSpeed:0.00}", _style);
        }
        else
        {
            GUILayout.Label("<b>GameManager:</b> NOT FOUND", _style);
        }

        GUILayout.Space(10);

        // Director
        if (director != null)
        {
            GUILayout.Label($"<b>Director Phase:</b> {director.CurrentPhase}", _style);
        }
        else
        {
            GUILayout.Label("<b>SessionDirector:</b> NOT FOUND", _style);
        }

        GUILayout.Space(10);

        // Spawner
        GUILayout.Label($"<b>OpponentSpawner:</b> {(spawner != null ? "FOUND" : "NOT FOUND")}", _style);

        GUILayout.Space(12);
        GUILayout.Label($"Toggle: <b>{toggleKey}</b> | Scale: <b>{scale:0.00}</b>", _style);

        GUILayout.EndArea();

        GUI.matrix = oldMatrix;
    }
}