/*using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private HitResolver hitResolver;

    private IMover _mover;
    private IJumper _jumper;
    private ISprinter _sprinter;
    private SwipeDetection _swipeDetector;

    public event Action OnAnyInputPressed;

    public bool ActionInputEnabled { get; private set; } = true;

    private void Awake()
    {
        _mover = GetComponent<IMover>();
        _jumper = GetComponent<IJumper>();
        _sprinter = GetComponent<ISprinter>();

        _swipeDetector = GetComponent<SwipeDetection>();
        if (_swipeDetector == null) _swipeDetector = gameObject.AddComponent<SwipeDetection>();

        _swipeDetector.OnSwipe += HandleSwipe;
        _swipeDetector.OnLongPress += HandleSprint;
        _swipeDetector.OnAnyTouchBegan += HandleAnyTouchBegan;
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    public void SetActionInputEnabled(bool enabled)
    {
        ActionInputEnabled = enabled;

        if (!enabled)
            HandleSprint(false);
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        bool anyPressed =
            Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame ||
            Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame;

        if (anyPressed)
            OnAnyInputPressed?.Invoke();

        if (!ActionInputEnabled)
            return;

        if (Keyboard.current.aKey.wasPressedThisFrame) _mover?.MoveStep(-1);
        if (Keyboard.current.dKey.wasPressedThisFrame) _mover?.MoveStep(1);
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame) _jumper?.Jump();
        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame) _jumper?.QuickDrop();

        if (Keyboard.current.wKey.wasPressedThisFrame) HandleSprint(true);
        if (Keyboard.current.wKey.wasReleasedThisFrame) HandleSprint(false);
    }

    private void HandleAnyTouchBegan()
    {
        OnAnyInputPressed?.Invoke();
    }

    private void HandleSwipe(Vector2 direction)
    {
        OnAnyInputPressed?.Invoke();

        if (!ActionInputEnabled)
            return;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            _mover?.MoveStep(direction.x > 0 ? 1 : -1);
        else
        {
            if (direction.y > 0) _jumper?.Jump();
            else _jumper?.QuickDrop();
        }
    }

    private void HandleSprint(bool isSprinting)
    {
        if (isSprinting)
            OnAnyInputPressed?.Invoke();

        if (!ActionInputEnabled)
            return;

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
        if (_swipeDetector != null)
        {
            _swipeDetector.OnSwipe -= HandleSwipe;
            _swipeDetector.OnLongPress -= HandleSprint;
            _swipeDetector.OnAnyTouchBegan -= HandleAnyTouchBegan;
        }

        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost -= DisableInput;
    }

    private void DisableInput()
    {
        if (_swipeDetector != null)
            _swipeDetector.enabled = false;

        HandleSprint(false);

        if (_sprinter is MonoBehaviour sprintScript)
            sprintScript.enabled = false;

        enabled = false;
        Debug.Log("Inputlar Devre Dışı!");
    }
}*/

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private HitResolver hitResolver;

    private IMover _mover;
    private IJumper _jumper;
    private ISprinter _sprinter;
    private IRainbowMove _rainbowMove;
    private SwipeDetection _swipeDetector;

    public event Action OnAnyInputPressed;

    public bool ActionInputEnabled { get; private set; } = true;

    private void Awake()
    {
        _mover = GetComponent<IMover>();
        _jumper = GetComponent<IJumper>();
        _sprinter = GetComponent<ISprinter>();
        _rainbowMove = GetComponent<IRainbowMove>();

        _swipeDetector = GetComponent<SwipeDetection>();
        if (_swipeDetector == null)
            _swipeDetector = gameObject.AddComponent<SwipeDetection>();

        _swipeDetector.OnSwipe += HandleSwipe;
        _swipeDetector.OnLongPress += HandleSprint;
        _swipeDetector.OnAnyTouchBegan += HandleAnyTouchBegan;
    }

    private void Update()
    {
        HandleKeyboardInput();
    }

    public void SetActionInputEnabled(bool enabled)
    {
        ActionInputEnabled = enabled;

        if (!enabled)
            HandleSprint(false);
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current == null) return;

        bool anyPressed =
            Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame ||
            Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame;

        if (anyPressed)
            OnAnyInputPressed?.Invoke();

        if (!ActionInputEnabled)
            return;

        if (Keyboard.current.aKey.wasPressedThisFrame)
            _mover?.MoveStep(-1);

        if (Keyboard.current.dKey.wasPressedThisFrame)
            _mover?.MoveStep(1);

        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
            _jumper?.Jump();

        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
            HandleDownAction();

        if (Keyboard.current.wKey.wasPressedThisFrame)
            HandleSprint(true);

        if (Keyboard.current.wKey.wasReleasedThisFrame)
            HandleSprint(false);
    }

    private void HandleAnyTouchBegan()
    {
        OnAnyInputPressed?.Invoke();
    }

    private void HandleSwipe(Vector2 direction)
    {
        OnAnyInputPressed?.Invoke();

        if (!ActionInputEnabled)
            return;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            _mover?.MoveStep(direction.x > 0 ? 1 : -1);
            return;
        }

        if (direction.y > 0)
        {
            _jumper?.Jump();
            return;
        }

        HandleDownAction();
    }

    private void HandleDownAction()
    {
        if (_rainbowMove != null && _rainbowMove.TryRainbow())
            return;

        _jumper?.QuickDrop();
    }

    private void HandleSprint(bool isSprinting)
    {
        if (isSprinting)
            OnAnyInputPressed?.Invoke();

        if (!ActionInputEnabled)
            return;

        if (_sprinter == null) return;

        if (isSprinting)
            _sprinter.StartSprint();
        else
            _sprinter.StopSprint();
    }

    private void OnEnable()
    {
        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost += DisableInput;
    }

    private void OnDisable()
    {
        if (_swipeDetector != null)
        {
            _swipeDetector.OnSwipe -= HandleSwipe;
            _swipeDetector.OnLongPress -= HandleSprint;
            _swipeDetector.OnAnyTouchBegan -= HandleAnyTouchBegan;
        }

        if (GameOver.Instance != null)
            GameOver.Instance.OnGameLost -= DisableInput;
    }

    private void DisableInput()
    {
        if (_swipeDetector != null)
            _swipeDetector.enabled = false;

        HandleSprint(false);

        if (_sprinter is MonoBehaviour sprintScript)
            sprintScript.enabled = false;

        enabled = false;
        Debug.Log("Inputlar Devre Dışı!");
    }
}