using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{

    [SerializeField] private HitResolver hitResolver;
    private IMover _mover;
    private IJumper _jumper;
    private ISprinter _sprinter;
    private SwipeDetection _swipeDetector;

    private void Awake()
    {
        _mover = GetComponent<IMover>();
        _jumper = GetComponent<IJumper>();
        _sprinter = GetComponent<ISprinter>();

        // SwipeDetection objede yoksa ekle
        _swipeDetector = GetComponent<SwipeDetection>();
        if (_swipeDetector == null) _swipeDetector = gameObject.AddComponent<SwipeDetection>();

        // Mobil Eventleri Bağla
        _swipeDetector.OnSwipe += HandleSwipe;
        _swipeDetector.OnLongPress += HandleSprint;
    }

    private void Update()
    {
        // Klavye her zaman çalışmaya devam eder (Test için)
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.aKey.wasPressedThisFrame) _mover?.MoveStep(-1);
        if (Keyboard.current.dKey.wasPressedThisFrame) _mover?.MoveStep(1);
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) _jumper?.Jump();
        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame) _jumper?.QuickDrop();

        // Klavye sprint (W tuşu)
        if (Keyboard.current.wKey.wasPressedThisFrame) HandleSprint(true);
        if (Keyboard.current.wKey.wasReleasedThisFrame) HandleSprint(false);
    }

    private void HandleSwipe(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            _mover?.MoveStep(direction.x > 0 ? 1 : -1);
        else
            if (direction.y > 0) _jumper?.Jump(); else _jumper?.QuickDrop();
    }

    private void HandleSprint(bool isSprinting)
    {
        if (_sprinter == null) return;
        if (isSprinting) _sprinter.StartSprint();
        else _sprinter.StopSprint();
    }


    private void OnEnable()
    {
        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost += DisableInput;
    }

    private void OnDisable()
    {
        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost -= DisableInput;
    }


    private void DisableInput()
    {
        _swipeDetector.enabled = false;
        this.enabled = false; // Update döngüsünü durdurur, tuşlar çalışmaz.
        HandleSprint(false);
        if (_sprinter is MonoBehaviour sprintScript)
        {
            sprintScript.enabled = false;
        }
        Debug.Log("Inputlar Devre Dışı!");
    }
}