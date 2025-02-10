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

    public Sprite keyboardRSprite;
    public Sprite keyboardASprite;
    public Sprite keyboardFSprite;
    public Sprite keyboardCSprite;
    public Sprite keyboardVSprite;
    public Sprite keyboardQSprite;
    public Sprite keyboardDSprite;
    public Sprite keyboardESprite;
    public Sprite keyboardZSprite;
    public Sprite keyboardXSprite;
    public Sprite keyboardLSprite;
    public Sprite keyboard1Sprite;
    public Sprite keyboard2Sprite;
    public Sprite keyboardShiftSprite;
    public Sprite keyboardMouseClickSprite;
    public Sprite keyboardMouseRightClickSprite;
    public Sprite keyboardMouseSprite;
    public Sprite keyboardEscSprite;
    public Sprite keyboardSpaceSprite;
    public Sprite keyboardTabSprite;

    private void Awake() {
        Instance = this;
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
                spriteList.Add(keyboardRSprite);
            }
            if (control == Control.Skill1) {
                spriteList.Add(keyboardASprite);
            }
            if (control == Control.Skill2) {
                spriteList.Add(keyboardFSprite);
            }
            if (control == Control.SwitchDog) {
                spriteList.Add(keyboardCSprite);
            }
            if (control == Control.SwapWeapon) {
                spriteList.Add(keyboard1Sprite);
                spriteList.Add(keyboard2Sprite);
            }
            if (control == Control.Shoot) {
                spriteList.Add(keyboardMouseClickSprite);
            }
            if (control == Control.Interact) {
                spriteList.Add(keyboardESprite);
            }
            if (control == Control.Run) {
                spriteList.Add(keyboardShiftSprite);
            }
            if (control == Control.LightSwitch) {
                spriteList.Add(keyboardVSprite);
            }
            if (control == Control.Aim) {
                spriteList.Add(keyboardMouseSprite);
            }
            if (control == Control.Back) {
                spriteList.Add(keyboardEscSprite);
            }
            if (control == Control.SecondaryGunAbility) {
                spriteList.Add(keyboardMouseRightClickSprite);
            }
            if (control == Control.Roll) {
                spriteList.Add(keyboardSpaceSprite);
            }
            if (control == Control.OpenPlayerMenu) {
                spriteList.Add(keyboardTabSprite);
            }
            if (control == Control.SwitchBuildingFunctions) {
                spriteList.Add(keyboardZSprite);
                spriteList.Add(keyboardXSprite);
            }
            if (control == Control.Move) {
                spriteList.Add(keyboardQSprite);
                spriteList.Add(keyboardDSprite);
            }
        }

        return spriteList;
    }
}
