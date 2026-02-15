using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class PlayerInput : MonoBehaviour
{
    private IMover _mover;
    private IJumper _jumper;

    private void Awake()
    {
        _mover = GetComponent<IMover>();
        _jumper = GetComponent<IJumper>();

        // KONTROL 1: Scriptleri bulabildi mi?
        if (_mover == null) Debug.LogError("HATA: PlayerLateralMovement scripti bu objede yok!");
        else Debug.Log("BAŞARILI: Sağa/Sola gitme scripti bulundu.");

        if (_jumper == null) Debug.LogError("HATA: PlayerVerticalMovement scripti bu objede yok!");
        else Debug.Log("BAŞARILI: Zıplama scripti bulundu.");
    }
    private void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("TUŞ: Space tuşuna basıldı!"); // Tuşu algılıyor mu?
            
            if (_jumper != null)
            {
                _jumper.Jump();
                Debug.Log("EYLEM: Zıplama komutu gönderildi.");
            }
        }
        HandleLateralInput();
        HandleVerticalInput();
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
}


    
