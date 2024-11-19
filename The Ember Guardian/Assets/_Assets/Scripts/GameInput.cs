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

    public event EventHandler OnPlayerInteractStarted;
    public event EventHandler OnPlayerInteractHeldDown;
    public event EventHandler OnPlayerInteractCanceled;

    public event EventHandler OnPlayerShootStarted;
    public event EventHandler OnPlayerShootCanceled;

    public event EventHandler OnPlayerReloadPerformed;
    public event EventHandler OnPlayerReloadCanceled;

    public event EventHandler OnPlayerGunLightSwitch;

    public event EventHandler OnPlayerLeftRightSwitchPerformed;

    private bool interactPressed;
    private bool holdingInteract;
    private float interactHoldTimer;

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
        playerInputActions.Player.Interact.started += Interact_started;
        playerInputActions.Player.Interact.canceled += Interact_canceled;
        playerInputActions.Player.Shoot.started += Shoot_started;
        playerInputActions.Player.Shoot.canceled += Shoot_canceled;
        playerInputActions.Player.Reload.performed += Reload_performed;
        playerInputActions.Player.Reload.canceled += Reload_canceled;
        playerInputActions.Player.SwitchGunLight.performed += SwitchGunLight_performed;

        playerInputActions.Player.LeftRightSwitch.performed += LeftRightSwitch_performed;
    }


    private void Update() {
        if(interactPressed) {
            interactHoldTimer += Time.deltaTime;

            if(interactHoldTimer > .15f && !holdingInteract) {
                holdingInteract = true;
                OnPlayerInteractHeldDown?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void SwitchGunLight_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerGunLightSwitch?.Invoke(this, EventArgs.Empty);
    }
    private void Reload_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerReloadCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Reload_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerReloadPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void Shoot_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerShootCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Shoot_started(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerShootStarted?.Invoke(this, EventArgs.Empty);
    }

    private void LeftRightSwitch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerLeftRightSwitchPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        interactPressed = false;
        OnPlayerInteractCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_started(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        interactPressed = true;
        holdingInteract = false;
        interactHoldTimer = 0;
        OnPlayerInteractStarted?.Invoke(this, EventArgs.Empty);
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
    public float GetLeftRightDir() {
        float leftRightDir = playerInputActions.Player.LeftRightSwitch.ReadValue<float>();
        return leftRightDir;
    }

    public bool GetWasHoldingInteract() {
        return holdingInteract;
    }
}
