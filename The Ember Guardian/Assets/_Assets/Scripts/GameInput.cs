using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance;
    private PlayerInputActions playerInputActions;

    public event EventHandler OnPlayerRunStarted;
    public event EventHandler OnPlayerRunCanceled;

    private void Awake() {
        Instance = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    private void Start() {
        playerInputActions.Player.Run.started += Run_started;
        playerInputActions.Player.Run.canceled += Run_canceled;
    }

    private void Run_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerRunCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Run_started(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerRunStarted?.Invoke(this, EventArgs.Empty);
    }

    public float GetMovementFloatNormalized() {
        float moveInput = playerInputActions.Player.Move.ReadValue<float>();
        return moveInput;
    }

}
