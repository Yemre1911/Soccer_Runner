/*using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class PlayerInput : MonoBehaviour
{
    private IMover _mover;
    private IJumper _jumper;
    private ISprinter _sprinter;
    private void Awake()
    {
        _mover = GetComponent<IMover>();
        _jumper = GetComponent<IJumper>();
        _sprinter = GetComponent<ISprinter>(); // Sprint scriptini al

        // KONTROL 1: Scriptleri bulabildi mi?
        if (_mover == null) Debug.LogError("HATA: PlayerLateralMovement scripti bu objede yok!");
        else Debug.Log("BAŞARILI: Sağa/Sola gitme scripti bulundu.");

        if (_jumper == null) Debug.LogError("HATA: PlayerVerticalMovement scripti bu objede yok!");
        else Debug.Log("BAŞARILI: Zıplama scripti bulundu.");

        if (_sprinter == null) Debug.LogError("ISprinter bulunamadı!");
    }
    private void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (_jumper != null)
                _jumper.Jump();
        }
        HandleLateralInput();
        HandleVerticalInput();
        HandleSprintInput();
    }

    private void HandleLateralInput()
    {
        // Sola Basma
        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            _mover.MoveStep(-1);
        }

        // Sağa Basma
        if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            _mover.MoveStep(1);
        }
    }

    private void HandleVerticalInput()
    {
        // Zıplama (Space veya Yukarı Ok)
        if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            _jumper.Jump();
        }

        // Hızlı İniş (S veya Aşağı Ok)
        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            _jumper.QuickDrop();
        }
    }

    private void HandleSprintInput()
    {
        if (_sprinter == null) return;

        // Gelecek için not: Mobilde burası "IsTouchingScreen" gibi bir bool olacak.
        bool isSprintKeyPressed = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed;

        if (isSprintKeyPressed)
        {
            _sprinter.StartSprint();
        }
        else
        {
            _sprinter.StopSprint();
        }
    }
}
*/

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
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
}