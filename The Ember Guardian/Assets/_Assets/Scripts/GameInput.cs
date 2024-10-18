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

    public event EventHandler OnPlayerJumpStarted;
    public event EventHandler OnPlayerJumpCanceled;

    private void Awake() {
        Instance = this;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    private void Start() {
        playerInputActions.Player.Run.started += Run_started;
        playerInputActions.Player.Run.canceled += Run_canceled;
        playerInputActions.Player.Jump.started += Jump_started;
        playerInputActions.Player.Jump.canceled += Jump_canceled;
    }

    private void Jump_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerJumpCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Jump_started(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerJumpStarted?.Invoke(this, EventArgs.Empty);
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
    public float GetJumpDirNormalized() {
        float jumpDir = playerInputActions.Player.JumpDir.ReadValue<float>();
        return jumpDir;
    }
}
