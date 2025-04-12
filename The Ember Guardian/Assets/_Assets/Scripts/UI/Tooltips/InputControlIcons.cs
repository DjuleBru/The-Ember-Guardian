using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputControlIcons : MonoBehaviour
{
    public static InputControlIcons Instance;

    public enum Control {
        Reload,
        Skill1,
        Skill2,
        SwapWeapon,
        Shoot,
        Interact,
        Move,
        Run,
        LightSwitch,
        SwitchDog,
        Aim,
        Back,
        SecondaryGunAbility,
        Roll,
        OpenPlayerMenu,
        SwitchBuildingFunctions,
        MeleeAttack,
    }


    public Sprite gamepadYSprite;
    public Sprite gamepadBSprite;
    public Sprite gamepadASprite;
    public Sprite gamepadXSprite;
    public Sprite gamepadRbSprite;
    public Sprite gamepadLbSprite;
    public Sprite gamepadRtSprite;
    public Sprite gamepadLtSprite;
    public Sprite gamepadR3Sprite;
    public Sprite gamepadL3Sprite;
    public Sprite gamepadLLeftRightSprite;
    public Sprite gamepadRJoystickSprite;
    public Sprite gamepadDownArrowSprite;
    public Sprite gamepadUpArrowSprite;
    public Sprite gamepadLeftArrowSprite;
    public Sprite gamepadRightArrowSprite;
    public Sprite gamepadSelectSprite;

    public Sprite keyboardASprite;
    public Sprite keyboardZSprite;
    public Sprite keyboardRSprite;
    public Sprite keyboardTSprite;
    public Sprite keyboardYSprite;
    public Sprite keyboardUSprite;
    public Sprite keyboardISprite;
    public Sprite keyboardOSprite;
    public Sprite keyboardPSprite;
    public Sprite keyboardQSprite;
    public Sprite keyboardFSprite;
    public Sprite keyboardSSprite;
    public Sprite keyboardGSprite;
    public Sprite keyboardHSprite;
    public Sprite keyboardJSprite;
    public Sprite keyboardKSprite;
    public Sprite keyboardLSprite;
    public Sprite keyboardMSprite;
    public Sprite keyboardWSprite;
    public Sprite keyboardXSprite;
    public Sprite keyboardCSprite;
    public Sprite keyboardVSprite;
    public Sprite keyboardBSprite;
    public Sprite keyboardNSprite;
    public Sprite keyboardDSprite;
    public Sprite keyboardESprite;
    public Sprite keyboard1Sprite;
    public Sprite keyboard2Sprite;
    public Sprite keyboard3Sprite;
    public Sprite keyboard4Sprite;
    public Sprite keyboard5Sprite;
    public Sprite keyboard6Sprite;
    public Sprite keyboard7Sprite;
    public Sprite keyboard8Sprite;
    public Sprite keyboard9Sprite;
    public Sprite keyboard0Sprite;
    public Sprite keyboardF1Sprite;
    public Sprite keyboardF2Sprite;
    public Sprite keyboardF3Sprite;
    public Sprite keyboardF4Sprite;
    public Sprite keyboardF5Sprite;
    public Sprite keyboardF6Sprite;
    public Sprite keyboardF7Sprite;
    public Sprite keyboardF8Sprite;
    public Sprite keyboardF9Sprite;
    public Sprite keyboardF10Sprite;
    public Sprite keyboardF11Sprite;
    public Sprite keyboardF12Sprite;
    public Sprite keyboardShiftSprite;
    public Sprite keyboardRightShiftSprite;
    public Sprite keyboardMouseClickSprite;
    public Sprite keyboardMouseMiddleClickSprite;
    public Sprite keyboardMouseRightClickSprite;
    public Sprite keyboardMouseSprite;
    public Sprite keyboardEscSprite;
    public Sprite keyboardSpaceSprite;
    public Sprite keyboardBackspaceSprite;
    public Sprite keyboardTabSprite;
    public Sprite keyboardAltSprite;
    public Sprite keyboardCtrlSprite;
    public Sprite keyboardCapsLockSprite;
    public Sprite keyboardEnterSprite;
    public Sprite keyboardRightArrowSprite;
    public Sprite keyboardLeftArrowSprite;
    public Sprite keyboardUpArrowSprite;
    public Sprite keyboardDownArrowSprite;
    public Sprite keyboardLeftBracketSprite;
    public Sprite keyboardRightBracketSprite;
    public Sprite keyboardBackslashSprite;
    public Sprite keyboardFrontslashSprite;
    public Sprite keyboardCommaSprite;
    public Sprite keyboardColonSprite;
    public Sprite keyboardSemiColonSprite;
    public Sprite keyboardQuestionMarkSprite;

    public Dictionary<string, Sprite> keyboardIconLookup = new Dictionary<string, Sprite>();

    private void Awake() {
        Instance = this;
        InitializeDictionaries();
    }

    private void InitializeDictionaries() {
        keyboardIconLookup = new Dictionary<string, Sprite>()
{
            { "A", keyboardASprite },
            { "B", keyboardBSprite },
            { "C", keyboardCSprite },
            { "D", keyboardDSprite },
            { "E", keyboardESprite },
            { "F", keyboardFSprite },
            { "G", keyboardGSprite },
            { "H", keyboardHSprite },
            { "I", keyboardISprite },
            { "J", keyboardJSprite },
            { "K", keyboardKSprite },
            { "L", keyboardLSprite },
            { "M", keyboardMSprite },
            { "N", keyboardNSprite },
            { "O", keyboardOSprite },
            { "P", keyboardPSprite },
            { "Q", keyboardQSprite },
            { "R", keyboardRSprite },
            { "S", keyboardSSprite },
            { "T", keyboardTSprite },
            { "U", keyboardUSprite },
            { "V", keyboardVSprite },
            { "W", keyboardWSprite },
            { "X", keyboardXSprite },
            { "Y", keyboardYSprite },
            { "Z", keyboardZSprite },
            { "1", keyboard1Sprite },
            { "2", keyboard2Sprite },
            { "3", keyboard3Sprite },
            { "4", keyboard4Sprite },
            { "5", keyboard5Sprite },
            { "6", keyboard6Sprite },
            { "7", keyboard7Sprite },
            { "8", keyboard8Sprite },
            { "9", keyboard9Sprite },
            { "0", keyboard0Sprite },
            { "F1", keyboardF1Sprite },
            { "F2", keyboardF2Sprite },
            { "F3", keyboardF3Sprite },
            { "F4", keyboardF4Sprite },
            { "F5", keyboardF5Sprite },
            { "F6", keyboardF6Sprite },
            { "F7", keyboardF7Sprite },
            { "F8", keyboardF8Sprite },
            { "F9", keyboardF9Sprite },
            { "F10", keyboardF10Sprite },
            { "F11", keyboardF11Sprite },
            { "F12", keyboardF12Sprite },
            { "Shift", keyboardShiftSprite },
            { "Space", keyboardSpaceSprite },
            { "LMB", keyboardMouseClickSprite },
            { "RMB", keyboardMouseRightClickSprite },
            { "Tab", keyboardTabSprite },
            { "Escape", keyboardEscSprite },
            { "Left Alt", keyboardAltSprite },
            { "Left Control", keyboardCtrlSprite },
            { "Caps Lock", keyboardCapsLockSprite },
            { "Right Arrow", keyboardRightArrowSprite },
            { "Up Arrow", keyboardUpArrowSprite },
            { "Left Arrow", keyboardLeftArrowSprite },
            { "Down Arrow", keyboardDownArrowSprite },
            { "Right Shift", keyboardRightShiftSprite },
            { "Enter", keyboardEnterSprite },
            { "Backspace", keyboardBackspaceSprite },
            { "[", keyboardLeftBracketSprite },
            { "]", keyboardRightBracketSprite },
            { "\\", keyboardBackslashSprite },
            { ",", keyboardCommaSprite },
            { ";", keyboardSemiColonSprite },
            { ":", keyboardColonSprite },
            { "/", keyboardFrontslashSprite },
        };
    }

    public List<Sprite> GetControlIconSprite(Control control) {
        List<Sprite> spriteList = new List<Sprite>();

        if(GameInput.Instance.IsUsingGamepad()) {
            if (control == Control.Reload) {
                spriteList.Add(gamepadYSprite);
            }
            if (control == Control.Skill1) {
                spriteList.Add(gamepadRbSprite);
            }
            if (control == Control.Skill2) {
                spriteList.Add(gamepadLbSprite);
            }
            if (control == Control.SwapWeapon) {
                spriteList.Add(gamepadR3Sprite);
            }
            if (control == Control.Shoot) {
                spriteList.Add(gamepadRtSprite);
            }
            if (control == Control.Interact) {
                spriteList.Add(gamepadXSprite);
            }
            if (control == Control.Move) {
                spriteList.Add(gamepadLLeftRightSprite);
            }
            if (control == Control.Run) {
                spriteList.Add(gamepadL3Sprite);
            }
            if (control == Control.LightSwitch) {
                spriteList.Add(gamepadDownArrowSprite);
            }
            if (control == Control.SwitchDog) {
                spriteList.Add(gamepadBSprite);
            }
            if (control == Control.Aim) {
                spriteList.Add(gamepadRJoystickSprite);
            }
            if (control == Control.Back) {
                spriteList.Add(gamepadBSprite);
            }
            if (control == Control.SecondaryGunAbility) {
                spriteList.Add(gamepadLtSprite);
            }
            if (control == Control.Roll) {
                spriteList.Add(gamepadASprite);
            }
            if (control == Control.OpenPlayerMenu) {
                spriteList.Add(gamepadSelectSprite);
            }
            if (control == Control.SwitchBuildingFunctions) {
                spriteList.Add(gamepadLeftArrowSprite);
                spriteList.Add(gamepadRightArrowSprite);
            }
        } else {

            if (control == Control.Reload) {
                string reload = GameInput.Instance.GetBindingText(GameInput.Binding.reload);

                if (keyboardIconLookup.TryGetValue(reload, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }

            if (control == Control.Skill1) {
                string ability1 = GameInput.Instance.GetBindingText(GameInput.Binding.ability1);

                if (keyboardIconLookup.TryGetValue(ability1, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.Skill2) {
                string ability2 = GameInput.Instance.GetBindingText(GameInput.Binding.ability2);

                if (keyboardIconLookup.TryGetValue(ability2, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.SwitchDog) {
                string callDog = GameInput.Instance.GetBindingText(GameInput.Binding.callDoggo);

                if (keyboardIconLookup.TryGetValue(callDog, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.SwapWeapon) {
                string selectWeapon1 = GameInput.Instance.GetBindingText(GameInput.Binding.selectPrimaryGun);
                string selectWeapon2 = GameInput.Instance.GetBindingText(GameInput.Binding.selectSecondaryGun);

                if (keyboardIconLookup.TryGetValue(selectWeapon1, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
                if (keyboardIconLookup.TryGetValue(selectWeapon2, out Sprite icon2)) {
                    spriteList.Add(icon2);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.Shoot) {
                string shoot = GameInput.Instance.GetBindingText(GameInput.Binding.shoot);

                if (keyboardIconLookup.TryGetValue(shoot, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.Interact) {
                string interact = GameInput.Instance.GetBindingText(GameInput.Binding.interact);

                if (keyboardIconLookup.TryGetValue(interact, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.Run) {
                string run = GameInput.Instance.GetBindingText(GameInput.Binding.run);

                if (keyboardIconLookup.TryGetValue(run, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.LightSwitch) {
                string lightSwitch = GameInput.Instance.GetBindingText(GameInput.Binding.torchOnOff);

                if (keyboardIconLookup.TryGetValue(lightSwitch, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.Aim) {
                spriteList.Add(keyboardMouseSprite);
            }
            if (control == Control.Back) {
                spriteList.Add(keyboardEscSprite);
            }
            if (control == Control.SecondaryGunAbility) {
                string secondaryGunAbility = GameInput.Instance.GetBindingText(GameInput.Binding.secondary);

                if (keyboardIconLookup.TryGetValue(secondaryGunAbility, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.Roll) {
                string roll = GameInput.Instance.GetBindingText(GameInput.Binding.roll);

                if (keyboardIconLookup.TryGetValue(roll, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.OpenPlayerMenu) {
                string characterMenu = GameInput.Instance.GetBindingText(GameInput.Binding.characterMenu);

                if (keyboardIconLookup.TryGetValue(characterMenu, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.SwitchBuildingFunctions) {
                string browse1 = GameInput.Instance.GetBindingText(GameInput.Binding.buildingFunctionLeft);
                string browse2 = GameInput.Instance.GetBindingText(GameInput.Binding.buildingFunctionRight);

                if (keyboardIconLookup.TryGetValue(browse1, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
                if (keyboardIconLookup.TryGetValue(browse2, out Sprite icon2)) {
                    spriteList.Add(icon2);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
            if (control == Control.Move) {
                string moveLeft = GameInput.Instance.GetBindingText(GameInput.Binding.moveLeft);
                string moveRight = GameInput.Instance.GetBindingText(GameInput.Binding.moveRight);

                if (keyboardIconLookup.TryGetValue(moveLeft, out Sprite icon1)) {
                    spriteList.Add(icon1);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
                if (keyboardIconLookup.TryGetValue(moveRight, out Sprite icon2)) {
                    spriteList.Add(icon2);
                }
                else {
                    spriteList.Add(keyboardQuestionMarkSprite);
                }
            }
        }

        return spriteList;
    }
}
