using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;
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
        meleeAttack,
        editCampSelect,
        editCampDeselect,
        collectCurrencyFromContainer,
        commandWorkers,
    }

    private PlayerInputActions playerInputActions;
    private PlayerInput playerInput;
    private ES3Settings settingsSaveFileSettings;

    public event EventHandler OnPlayerInputChanged;

    public event EventHandler OnPlayerRunPerformed;
    public event EventHandler OnPlayerRunCanceled;

    public event EventHandler OnPlayerRollPerformed;
    public event EventHandler OnPlayerRollCanceled;

    public event EventHandler OnPlayerInteractPerformed;
    public event EventHandler OnPlayerInteractHeldDown;
    public event EventHandler OnPlayerInteractCanceled;

    public event EventHandler OnPlayerShootPerformed;
    public event EventHandler OnPlayerShootCanceled;

    public event EventHandler OnPlayerReloadPerformed;
    public event EventHandler OnPlayerReloadCanceled;

    public event EventHandler OnPlayerGunLightSwitch;

    public event EventHandler OnPlayerLeftRightDirPerformed;
    public event EventHandler OnPlayerNavigateUIPerformed;

    public event EventHandler OnPlayerLeftRightSwitchPerformed;
    public event EventHandler OnPlayerLeftSwitchPerformed;
    public event EventHandler OnPlayerRightSwitchPerformed;

    public event EventHandler OnPlayerLeftSkillPerformed;
    public event EventHandler OnPlayerRightSkillPerformed;

    public event EventHandler OnPlayerSwapGunCanceled;
    public event EventHandler OnPlayerPrimaryGunSelected;
    public event EventHandler OnPlayerSecondaryGunSelected;

    public event EventHandler OnWeaponSecondaryAbilityPerformed;
    public event EventHandler OnWeaponSecondaryAbilityCanceled;

    public event EventHandler OnHoverWorkersPerformed;
    public event EventHandler OnMeleeAttackPerformed;

    public event EventHandler OnPlayerBackPerformed;
    public event EventHandler OnPlayerPausePerformed;
    public event EventHandler OnPlayerOpenPlayerTabPerformed;

    public event EventHandler OnEditCampSelect;
    public event EventHandler OnEditCampSelectReleased;
    public event EventHandler OnEditCampDeselect;

    public event EventHandler OnCommandWorkerPerformed;
    public event EventHandler OnCommandWorkerHeldDown;
    public event EventHandler OnCommandWorkerHeldDownStarted;

    public event EventHandler OnCurrencyCollectedFromContainer;

    private bool interactPressed;
    private bool holdingInteract;
    private float interactHoldTimer;

    private bool commandWorkersPressed;
    private bool holdingCommandWorkersStarted;
    private bool holdingCommandWorkers;
    private float commandWorkersHoldTimer;
    private float commandWorkersHoldTime = 1.25f;
    private float commandWorkersStartHoldTime = .25f;

    private string currentControlScheme;
    private Vector2 lastMousePosition;
    private bool isUsingGamepad;
    public const float gamepadMovementDeadzone = 0.5f;
    public const float gamepadDeadzone = 0.4f;
    private bool sceneIsReady = false;


    private void Awake() {

        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Détruit les doublons
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        playerInputActions = new PlayerInputActions();
        settingsSaveFileSettings = new ES3Settings("Settings.es3");

        bool hasSavedDefaultBindings = ES3.Load("SavedDefaultBindings", false, settingsSaveFileSettings);
        bool hasCustomBindings = ES3.Load("SavedCustomBindings", false, settingsSaveFileSettings);
        if(!hasSavedDefaultBindings) {
            ES3.Save("DefaultInputBindings", playerInputActions.SaveBindingOverridesAsJson(), settingsSaveFileSettings);
        }
        if(hasCustomBindings) {
            string defaultBindings = playerInputActions.SaveBindingOverridesAsJson();
            string playerBindings = ES3.Load("PlayerInputBindings", defaultValue:defaultBindings, settingsSaveFileSettings);

            playerInputActions.LoadBindingOverridesFromJson(playerBindings);
        }


        playerInputActions.Player.Enable();
        SceneManager.sceneLoaded += OnSceneLoaded;
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
        playerInputActions.Player.SwapGun.canceled += SwapGun_canceled;
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
        playerInputActions.Player.MeleeAttack.performed += MeleeAttack_performed;

        playerInputActions.Player.CommandWorker.performed += CommandWorker_performed;
        playerInputActions.Player.CommandWorker.canceled += CommandWorker_canceled;
        playerInputActions.Player.HoverWorkers.performed += HoverWorkers_performed;

        playerInputActions.Player.LeftRightSwitch.performed += LeftRightSwitch_performed;
        playerInputActions.Player.Move.performed += Move_performed;
        playerInputActions.Player.NavigateUI.performed += NavigateUI_performed;

        playerInputActions.Player.CampCustomizationSelect.performed += Select_performed;
        playerInputActions.Player.CampCustomizationSelect.canceled += Select_canceled;
        playerInputActions.Player.CampCustomizationDeselect.performed += Deselect_performed;

        playerInputActions.Player.CollectCurrencyFromContainer.performed += CollectCurrencyFromContainer_performed;
    }


    private void Update() {
        DetectControlSchemeMouse();

        if (interactPressed) {
            interactHoldTimer += Time.deltaTime;

            if (interactHoldTimer > .2f && !holdingInteract) {
                holdingInteract = true;
                OnPlayerInteractHeldDown?.Invoke(this, EventArgs.Empty);
            }
        }

        if (commandWorkersPressed) {
            commandWorkersHoldTimer += Time.deltaTime;

            if(commandWorkersHoldTimer > commandWorkersStartHoldTime && !holdingCommandWorkersStarted) {
                holdingCommandWorkersStarted = true;
                OnCommandWorkerHeldDownStarted?.Invoke(this, EventArgs.Empty);
            }
            if (commandWorkersHoldTimer > commandWorkersHoldTime && !holdingCommandWorkers) {
                holdingCommandWorkers = true;
                OnCommandWorkerHeldDown?.Invoke(this, EventArgs.Empty);
            }
        }

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        StartCoroutine(EnableInputChangedPropagationNextFrame());
    }
    private IEnumerator EnableInputChangedPropagationNextFrame() {
        yield return null; // attend une frame (tous les Start() seront passés)
        sceneIsReady = true;
    }

    private void InputUser_onChange(InputUser user, InputUserChange change, InputDevice arg3) {
        if (!sceneIsReady) {
            Debug.Log("[GameInput] Input change ignoré, la scène n’est pas encore prête");
            return;
        }

        if (change == InputUserChange.ControlSchemeChanged) {
            currentControlScheme = user.controlScheme.Value.name;
        }

        OnPlayerInputChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Deselect_performed(InputAction.CallbackContext obj) {
        OnEditCampDeselect?.Invoke(this, EventArgs.Empty);
    }

    private void CollectCurrencyFromContainer_performed(InputAction.CallbackContext obj) {
        OnCurrencyCollectedFromContainer?.Invoke(this, EventArgs.Empty);
    }

    private void Select_performed(InputAction.CallbackContext obj) {
        OnEditCampSelect?.Invoke(this, EventArgs.Empty);
    }
    private void Select_canceled(InputAction.CallbackContext obj) {
        OnEditCampSelectReleased?.Invoke(this, EventArgs.Empty);
    }

    private void NavigateUI_performed(InputAction.CallbackContext obj) {
        OnPlayerNavigateUIPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void HoverWorkers_performed(InputAction.CallbackContext obj) {
        OnHoverWorkersPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void CommandWorker_performed(InputAction.CallbackContext obj) {
        OnCommandWorkerPerformed?.Invoke(this, EventArgs.Empty);
        commandWorkersPressed = true;
    }
    private void CommandWorker_canceled(InputAction.CallbackContext obj) {
        commandWorkersPressed = false;
        holdingCommandWorkers = false;
        holdingCommandWorkersStarted = false;
        commandWorkersHoldTimer = 0f;
    }

    private void MeleeAttack_performed(InputAction.CallbackContext obj) {
        OnMeleeAttackPerformed?.Invoke(this, EventArgs.Empty);
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

    private void SwapGun_canceled(InputAction.CallbackContext obj) {
        OnPlayerSwapGunCanceled?.Invoke(this, EventArgs.Empty);
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
        OnPlayerRollCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Jump_performed(InputAction.CallbackContext obj) {
        OnPlayerRollPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void Run_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
        OnPlayerRunCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Run_performed(InputAction.CallbackContext obj) {
        OnPlayerRunPerformed?.Invoke(this, EventArgs.Empty);
    }

    public float GetCommandWorkersHoldDownTimerNormalized() {
        return commandWorkersHoldTimer / commandWorkersHoldTime;
    }

    #endregion

    public Vector2 GetAimInput() {
        return playerInputActions.Player.Aim.ReadValue<Vector2>();
    }

    public float GetMovementFloatNormalized() {
        float moveInput = playerInputActions.Player.Move.ReadValue<float>();

        if(Mathf.Abs(moveInput) > gamepadMovementDeadzone) {
            return moveInput;
        } else {
            return 0;
        }
    }
    public Vector2 GetUINavigationVector() {
        Vector2 navigationInput = playerInputActions.Player.NavigateUI.ReadValue<Vector2>();
        return navigationInput;
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
                return HandleLanguageConversions(playerInputActions.Player.Move.bindings[1].ToDisplayString());

            case Binding.moveRight:
                return HandleLanguageConversions(playerInputActions.Player.Move.bindings[2].ToDisplayString());

            case Binding.buildingFunctionLeft:
                return HandleLanguageConversions(playerInputActions.Player.LeftRightSwitch.bindings[1].ToDisplayString());

            case Binding.buildingFunctionRight:
                return HandleLanguageConversions(playerInputActions.Player.LeftRightSwitch.bindings[2].ToDisplayString());

            case Binding.interact:
                return HandleLanguageConversions(playerInputActions.Player.Interact.bindings[0].ToDisplayString());

            case Binding.run:
                return HandleLanguageConversions(playerInputActions.Player.Run.bindings[0].ToDisplayString());

            case Binding.roll:
                return HandleLanguageConversions(playerInputActions.Player.Jump.bindings[0].ToDisplayString());

            case Binding.meleeAttack:
                return  HandleLanguageConversions(playerInputActions.Player.MeleeAttack.bindings[0].ToDisplayString());

            case Binding.shoot:
                return HandleLanguageConversions(playerInputActions.Player.Shoot.bindings[0].ToDisplayString());

            case Binding.reload:
                return HandleLanguageConversions(playerInputActions.Player.Reload.bindings[0].ToDisplayString());

            case Binding.secondary:
                return HandleLanguageConversions(playerInputActions.Player.WeaponSecondaryAbility.bindings[0].ToDisplayString());

            case Binding.selectPrimaryGun:
                return HandleLanguageConversions(playerInputActions.Player.SelectPrimaryGun.bindings[0].ToDisplayString());

            case Binding.selectSecondaryGun:
                return HandleLanguageConversions(playerInputActions.Player.SelectSecondaryGun.bindings[0].ToDisplayString());

            case Binding.ability1:
                return HandleLanguageConversions(playerInputActions.Player.LeftSkill.bindings[0].ToDisplayString());

            case Binding.ability2:
                return HandleLanguageConversions(playerInputActions.Player.RightSkill.bindings[0].ToDisplayString());

            case Binding.pause:
                return HandleLanguageConversions(playerInputActions.Player.Pause.bindings[0].ToDisplayString());

            case Binding.characterMenu:
                return HandleLanguageConversions(playerInputActions.Player.OpenPlayerTab.bindings[0].ToDisplayString());

            case Binding.hoverWorkers:
                return HandleLanguageConversions(playerInputActions.Player.HoverWorkers.bindings[0].ToDisplayString());

            case Binding.callDoggo:
                return HandleLanguageConversions(playerInputActions.Player.Back.bindings[0].ToDisplayString());

            case Binding.torchOnOff:
                return HandleLanguageConversions(playerInputActions.Player.SwitchGunLight.bindings[0].ToDisplayString());

            case Binding.editCampDeselect:
                return HandleLanguageConversions(playerInputActions.Player.CampCustomizationDeselect.bindings[0].ToDisplayString());

            case Binding.collectCurrencyFromContainer:
                return HandleLanguageConversions(playerInputActions.Player.CollectCurrencyFromContainer.bindings[0].ToDisplayString());

            case Binding.editCampSelect:
                return HandleLanguageConversions(playerInputActions.Player.CampCustomizationSelect.bindings[0].ToDisplayString());

            case Binding.commandWorkers:
                return HandleLanguageConversions(playerInputActions.Player.CommandWorker.bindings[0].ToDisplayString());

        }

        return playerInputActions.Player.Interact.bindings[0].ToDisplayString();
    }

    public void RebindBinding(Binding binding, Action onActionRebound) {
        playerInputActions.Player.Disable();
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
            case Binding.meleeAttack:
                inputAction = playerInputActions.Player.MeleeAttack;
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

        // Effectuer le rebinding
        inputAction.PerformInteractiveRebinding(bindingIndex)
            .WithControlsHavingToMatchPath("<Mouse>")

            .OnComplete(callback => {
                playerInputActions.Player.Enable();
                onActionRebound();


                ES3.Save("PlayerInputBindings", playerInputActions.SaveBindingOverridesAsJson(), settingsSaveFileSettings);
                ES3.Save("SavedCustomBindings", true, settingsSaveFileSettings);
            })
            .Start();
    }

    public void ResetBindingsToDefault() {
        playerInputActions.Player.Disable();

        string defaultBindingsForFunction = playerInputActions.SaveBindingOverridesAsJson();
        string defaultBindings = ES3.Load("DefaultInputBindings", defaultValue: defaultBindingsForFunction, settingsSaveFileSettings);

        playerInputActions.LoadBindingOverridesFromJson(defaultBindings);

        ES3.Save("SavedCustomBindings", false, settingsSaveFileSettings);
        playerInputActions.Player.Enable();
    }

    public string HandleLanguageConversions(string bindingText) {

        if (Application.systemLanguage == SystemLanguage.French) {
            if (bindingText == "A") return "Q";
            if (bindingText == "Q") return "A";
            if (bindingText == "Z") return "W";
            if (bindingText == "W") return "Z";
        }

        if (Application.systemLanguage == SystemLanguage.German) {
            if (bindingText == "Z") return "Y";
            if (bindingText == "Y") return "Z";
        }

        return bindingText;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
