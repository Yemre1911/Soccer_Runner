using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class PlayerInput : MonoBehaviour
{
    private IMover _mover;

    private void Awake()
    {
        _mover = GetComponent<IMover>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Sola Basma Anı (Tek seferlik tetik)
        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            _mover.MoveStep(-1); // Sola 1 adım at
        }
        
        // Sağa Basma Anı (Tek seferlik tetik)
        if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            _mover.MoveStep(1); // Sağa 1 adım at
        }
    }
}