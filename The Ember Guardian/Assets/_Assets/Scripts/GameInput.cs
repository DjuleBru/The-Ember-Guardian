using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance;

    private PlayerInputActions playerInputActions;
    private PlayerInput playerInput;

    public event EventHandler OnPlayerInputChanged;

    public event EventHandler OnPlayerRunPerformed;
    public event EventHandler OnPlayerRunCanceled;

    public event EventHandler OnPlayerJumpPerformed;
    public event EventHandler OnPlayerJumpCanceled;

    public event EventHandler OnPlayerInteractPerformed;
    public event EventHandler OnPlayerInteractHeldDown;
    public event EventHandler OnPlayerInteractCanceled;

    public event EventHandler OnPlayerShootPerformed;
    public event EventHandler OnPlayerShootCanceled;

    public event EventHandler OnPlayerReloadPerformed;
    public event EventHandler OnPlayerReloadCanceled;

    public event EventHandler OnPlayerGunLightSwitch;

    public event EventHandler OnPlayerLeftRightDirPerformed;

    public event EventHandler OnPlayerLeftRightSwitchPerformed;
    public event EventHandler OnPlayerLeftSwitchPerformed;
    public event EventHandler OnPlayerRightSwitchPerformed;

    public event EventHandler OnPlayerLeftSkillPerformed;
    public event EventHandler OnPlayerRightSkillPerformed;

    public event EventHandler OnPlayerSwapGunPerformed;
    public event EventHandler OnPlayerPrimaryGunSelected;
    public event EventHandler OnPlayerSecondaryGunSelected;

    public event EventHandler OnWeaponSecondaryAbilityPerformed;
    public event EventHandler OnWeaponSecondaryAbilityCanceled;

    public event EventHandler OnPlayerBackPerformed;

    private bool interactPressed;
    private bool holdingInteract;
    private float interactHoldTimer;

    private string currentControlScheme;
    private Vector2 lastMousePosition;
    private bool isUsingGamepad;
    public const float gamepadDeadzone = 0.2f;

    private void Awake() {

        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Détruit les doublons
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }

    private void Start() {
        playerInput = GetComponent<PlayerInput>();
        InputUser.onChange += InputUser_onChange;

        playerInputActions.Player.Run.performed += Run_performed;
        playerInputActions.Player.Run.canceled += Run_canceled;
        playerInputActions.Player.Jump.performed += Jump_performed;
        playerInputActions.Player.Jump.canceled += Jump_canceled;
        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.Interact.canceled += Interact_canceled;
        playerInputActions.Player.Shoot.performed += Shoot_performed;
        playerInputActions.Player.Shoot.canceled += Shoot_canceled;
        playerInputActions.Player.Reload.performed += Reload_performed;
        playerInputActions.Player.Reload.canceled += Reload_canceled;
        playerInputActions.Player.SwapGun.performed += SwapGun_performed;
        playerInputActions.Player.SelectPrimaryGun.performed += SelectPrimaryGun_performed;
        playerInputActions.Player.SelectSecondaryGun.performed += SelectSecondaryGun_performed;
        playerInputActions.Player.SwitchGunLight.performed += SwitchGunLight_performed;
        playerInputActions.Player.RightSkill.performed += RightSkill_performed;
        playerInputActions.Player.LeftSkill.performed += LeftSkill_performed;
        playerInputActions.Player.Back.performed += Back_performed;
        playerInputActions.Player.WeaponSecondaryAbility.performed += WeaponSecondaryAbility_performed;
        playerInputActions.Player.WeaponSecondaryAbility.canceled += WeaponSecondaryAbility_canceled;

        playerInputActions.Player.LeftRightSwitch.performed += LeftRightSwitch_performed;
        playerInputActions.Player.Move.performed += Move_performed;
    }

    private void InputUser_onChange(InputUser user, InputUserChange change, InputDevice arg3) {
        if (change == InputUserChange.ControlSchemeChanged) {
            currentControlScheme = user.controlScheme.Value.name;
        }
        OnPlayerInputChanged?.Invoke(this, EventArgs.Empty);
    }

    private void WeaponSecondaryAbility_canceled(InputAction.CallbackContext obj) {
        OnWeaponSecondaryAbilityCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void WeaponSecondaryAbility_performed(InputAction.CallbackContext obj) {
        OnWeaponSecondaryAbilityPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void Back_performed(InputAction.CallbackContext obj) {
        OnPlayerBackPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void Update() {
        DetectControlSchemeMouse();

        if (interactPressed) {
            interactHoldTimer += Time.deltaTime;

            if (interactHoldTimer > .35f && !holdingInteract) {
                holdingInteract = true;
                OnPlayerInteractHeldDown?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    private void DetectControlSchemeMouse() {
        Vector2 mousePosition = Input.mousePosition;
        Vector2 gamepadLookInput = playerInputActions.Player.Aim.ReadValue<Vector2>();

        // Détecte l'utilisation de la souris
        if ((mousePosition - lastMousePosition).sqrMagnitude > 0.01f) {
            lastMousePosition = mousePosition;
            if (isUsingGamepad) {
                currentControlScheme = "Keyboard";
                isUsingGamepad = false;
                OnPlayerInputChanged?.Invoke(this, EventArgs.Empty);
            };
        }

        // Détecte l'utilisation du joystick droit
        if (gamepadLookInput.magnitude > 0.1f) {
            if(!isUsingGamepad) {
                currentControlScheme = "Gamepad";
                isUsingGamepad = true;
                OnPlayerInputChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    public Vector2 GetGamepadLookInput() {
        Vector2 input = playerInputActions.Player.Aim.ReadValue<Vector2>();
        return input.magnitude > gamepadDeadzone ? input : Vector2.zero;
    }

    public bool IsUsingGamepad() {
        return currentControlScheme == "Gamepad";
    }

    private void Move_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerLeftRightDirPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void LeftSkill_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerLeftSkillPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void RightSkill_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerRightSkillPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void SwitchGunLight_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerGunLightSwitch?.Invoke(this, EventArgs.Empty);
    }
    private void Reload_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerReloadCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void SelectSecondaryGun_performed(InputAction.CallbackContext obj) {
        OnPlayerSecondaryGunSelected?.Invoke(this, EventArgs.Empty);
    }

    private void SelectPrimaryGun_performed(InputAction.CallbackContext obj) {
        OnPlayerPrimaryGunSelected?.Invoke(this, EventArgs.Empty);
    }

    private void SwapGun_performed(InputAction.CallbackContext obj) {
        OnPlayerSwapGunPerformed?.Invoke(this, EventArgs.Empty);
    }
    private void Reload_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerReloadPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void Shoot_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerShootCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Shoot_performed(InputAction.CallbackContext obj) {
        OnPlayerShootPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void LeftRightSwitch_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        float leftRightDir = GetLeftRightDir();

        if(leftRightDir > 0) {
            OnPlayerRightSwitchPerformed?.Invoke(this, EventArgs.Empty);
        } else {
            OnPlayerLeftSwitchPerformed?.Invoke(this, EventArgs.Empty);
        }
        OnPlayerLeftRightSwitchPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        interactPressed = false;
        OnPlayerInteractCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(InputAction.CallbackContext obj) {
        interactPressed = true;
        holdingInteract = false;
        interactHoldTimer = 0;
        OnPlayerInteractPerformed?.Invoke(this, EventArgs.Empty);
    }
    private void Jump_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerJumpCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Jump_performed(InputAction.CallbackContext obj) {
        OnPlayerJumpPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void Run_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerRunCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Run_performed(InputAction.CallbackContext obj) {
        OnPlayerRunPerformed?.Invoke(this, EventArgs.Empty);
    }
    public Vector2 GetAimInput() {
        return playerInputActions.Player.Aim.ReadValue<Vector2>();
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
