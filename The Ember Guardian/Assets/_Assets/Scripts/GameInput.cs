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

    public enum Binding {
        moveLeft,
        moveRight,
        interact,
        run,
        roll,
        shoot,
        reload,
        secondary,
        selectPrimaryGun,
        selectSecondaryGun,
        ability1,
        ability2,
        callDoggo,
        torchOnOff,
        buildingFunctionLeft,
        buildingFunctionRight,
        pause,
        characterMenu,
        hoverWorkers,
    }

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

    public event EventHandler OnHoverWorkersPerformed;

    public event EventHandler OnPlayerBackPerformed;
    public event EventHandler OnPlayerPausePerformed;
    public event EventHandler OnPlayerOpenPlayerTabPerformed;

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

        bool hasSavedDefaultBindings = ES3.Load("SavedDefaultBindings", false);
        bool hasCustomBindings = ES3.Load("SavedCustomBindings", false);
        if(!hasSavedDefaultBindings) {
            ES3.Save("DefaultInputBindings", playerInputActions.SaveBindingOverridesAsJson());
        }
        if(hasCustomBindings) {
            string defaultBindings = playerInputActions.SaveBindingOverridesAsJson();
            string playerBindings = ES3.Load("PlayerInputBindings", defaultValue:defaultBindings);

            playerInputActions.LoadBindingOverridesFromJson(playerBindings);
        }


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
        playerInputActions.Player.Pause.performed += Pause_performed;
        playerInputActions.Player.OpenPlayerTab.performed += OpenPlayerTab_performed;

        playerInputActions.Player.HoverWorkers.performed += HoverWorkers_performed;
        playerInputActions.Player.LeftRightSwitch.performed += LeftRightSwitch_performed;
        playerInputActions.Player.Move.performed += Move_performed;
    }

    private void InputUser_onChange(InputUser user, InputUserChange change, InputDevice arg3) {
        if (change == InputUserChange.ControlSchemeChanged) {
            currentControlScheme = user.controlScheme.Value.name;
        }
        OnPlayerInputChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HoverWorkers_performed(InputAction.CallbackContext obj) {
        OnHoverWorkersPerformed?.Invoke(this, EventArgs.Empty);
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

    #region INPUT MANAGEMENT


    private void OpenPlayerTab_performed(InputAction.CallbackContext obj) {
        OnPlayerOpenPlayerTabPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void Pause_performed(InputAction.CallbackContext obj) {
        OnPlayerPausePerformed?.Invoke(this, EventArgs.Empty);
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

    #endregion

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

    public string GetBindingText(Binding binding) {
        switch(binding) {

            case Binding.moveLeft:
                return playerInputActions.Player.Move.bindings[1].ToDisplayString();

            case Binding.moveRight:
                return playerInputActions.Player.Move.bindings[2].ToDisplayString();

            case Binding.buildingFunctionLeft:
                return playerInputActions.Player.LeftRightSwitch.bindings[1].ToDisplayString();

            case Binding.buildingFunctionRight:
                return playerInputActions.Player.LeftRightSwitch.bindings[2].ToDisplayString();

            case Binding.interact:
                return playerInputActions.Player.Interact.bindings[0].ToDisplayString();

            case Binding.run:
                return playerInputActions.Player.Run.bindings[0].ToDisplayString();

            case Binding.roll:
                return playerInputActions.Player.Jump.bindings[0].ToDisplayString();

            case Binding.shoot:
                return playerInputActions.Player.Shoot.bindings[0].ToDisplayString();

            case Binding.reload:
                return playerInputActions.Player.Reload.bindings[0].ToDisplayString();

            case Binding.secondary:
                return playerInputActions.Player.WeaponSecondaryAbility.bindings[0].ToDisplayString();

            case Binding.selectPrimaryGun:
                return playerInputActions.Player.SelectPrimaryGun.bindings[0].ToDisplayString();

            case Binding.selectSecondaryGun:
                return playerInputActions.Player.SelectSecondaryGun.bindings[0].ToDisplayString();

            case Binding.ability1:
                return playerInputActions.Player.LeftSkill.bindings[0].ToDisplayString();

            case Binding.ability2:
                return playerInputActions.Player.RightSkill.bindings[0].ToDisplayString();

            case Binding.pause:
                return playerInputActions.Player.Pause.bindings[0].ToDisplayString();

            case Binding.characterMenu:
                return playerInputActions.Player.OpenPlayerTab.bindings[0].ToDisplayString();

            case Binding.hoverWorkers:
                return playerInputActions.Player.HoverWorkers.bindings[0].ToDisplayString();

            case Binding.callDoggo:
                return playerInputActions.Player.Back.bindings[0].ToDisplayString();

            case Binding.torchOnOff:
                return playerInputActions.Player.SwitchGunLight.bindings[0].ToDisplayString();

        }

        return playerInputActions.Player.Interact.bindings[0].ToDisplayString();
    }

    public void RebindBinding(Binding binding, Action onActionRebound) {
        playerInputActions.Player.Disable();
        Debug.Log("RebindBinding " + binding);
        InputAction inputAction;
        int bindingIndex;

        switch(binding) {
            default:
            case Binding.moveLeft:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Binding.moveRight:
                inputAction = playerInputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.interact:
                inputAction = playerInputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Binding.run:
                inputAction = playerInputActions.Player.Run;
                bindingIndex = 0;
                break;
            case Binding.roll:
                inputAction = playerInputActions.Player.Jump;
                bindingIndex = 0;
                break;
            case Binding.shoot:
                inputAction = playerInputActions.Player.Shoot;
                bindingIndex = 0;
                break;
            case Binding.reload:
                inputAction = playerInputActions.Player.Reload;
                bindingIndex = 0;
                break;
            case Binding.secondary:
                inputAction = playerInputActions.Player.WeaponSecondaryAbility;
                bindingIndex = 0;
                break;
            case Binding.ability1:
                inputAction = playerInputActions.Player.LeftSkill;
                bindingIndex = 0;
                break;
            case Binding.ability2:
                inputAction = playerInputActions.Player.RightSkill;
                bindingIndex = 0;
                break;
            case Binding.selectPrimaryGun:
                inputAction = playerInputActions.Player.SelectPrimaryGun;
                bindingIndex = 0;
                break;
            case Binding.selectSecondaryGun:
                Debug.Log("case Binding.selectSecondaryGun: " + binding);
                inputAction = playerInputActions.Player.SelectSecondaryGun;
                bindingIndex = 0;
                break;
            case Binding.hoverWorkers:
                inputAction = playerInputActions.Player.HoverWorkers;
                bindingIndex = 0;
                break;
            case Binding.callDoggo:
                inputAction = playerInputActions.Player.Back;
                bindingIndex = 0;
                break;
            case Binding.torchOnOff:
                inputAction = playerInputActions.Player.SwitchGunLight;
                bindingIndex = 0;
                break;
            case Binding.buildingFunctionLeft:
                inputAction = playerInputActions.Player.LeftRightSwitch;
                bindingIndex = 1;
                break;
            case Binding.buildingFunctionRight:
                inputAction = playerInputActions.Player.LeftRightSwitch;
                bindingIndex = 2;
                break;
            case Binding.characterMenu:
                inputAction = playerInputActions.Player.OpenPlayerTab;
                bindingIndex = 0;
                break;
            case Binding.pause:
                inputAction = playerInputActions.Player.Pause;
                bindingIndex = 0;
                break;
        }

        Debug.Log("RebindBinding " + inputAction + " bindingIndex " + bindingIndex);
        inputAction.PerformInteractiveRebinding(bindingIndex).OnComplete(callback => {
            //Debug.Log(callback.action.bindings[1].path);
            Debug.Log(callback.action.bindings[0].path);

            playerInputActions.Player.Enable();
            onActionRebound();

            ES3.Save("PlayerInputBindings", playerInputActions.SaveBindingOverridesAsJson());
            ES3.Save("SavedCustomBindings", true);

        }).Start();
    }

    public void ResetBindingsToDefault() {
        playerInputActions.Player.Disable();


        string defaultBindingsForFunction = playerInputActions.SaveBindingOverridesAsJson();
        string defaultBindings = ES3.Load("DefaultInputBindings", defaultValue: defaultBindingsForFunction);

        playerInputActions.LoadBindingOverridesFromJson(defaultBindings);

        ES3.Save("SavedCustomBindings", false);
        playerInputActions.Player.Enable();
    }
}
