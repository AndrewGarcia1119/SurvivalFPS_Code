using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public bool IsLocalPlayerPaused { get; private set; } = false;
    private PlayerInput playerInput;

    public event Action<bool> onLocalPlayerPause;
    void OnEnable()
    {
        if (playerInput == null)
        {
            playerInput = new PlayerInput();
        }
        playerInput.Player.Pause.Enable();
        playerInput.Player.Pause.performed += ToggleLocalPlayerPausedEventWrapper;
    }



    void OnDisable()
    {
        playerInput.Player.Pause.Disable();
        playerInput.Player.Pause.performed -= ToggleLocalPlayerPausedEventWrapper;
    }

    private void ToggleLocalPlayerPausedEventWrapper(InputAction.CallbackContext context)
    {
        ToggleLocalPlayerPaused();
    }

    public void ToggleLocalPlayerPaused()
    {
        IsLocalPlayerPaused = !IsLocalPlayerPaused;
        onLocalPlayerPause?.Invoke(IsLocalPlayerPaused);
        Cursor.lockState = IsLocalPlayerPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }
}
