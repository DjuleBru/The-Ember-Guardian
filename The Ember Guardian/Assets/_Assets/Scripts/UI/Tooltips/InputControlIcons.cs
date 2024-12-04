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
    public Sprite gamepadRLeftRightSprite;

    public Sprite keyboardRSprite;
    public Sprite keyboardASprite;
    public Sprite keyboardFSprite;
    public Sprite keyboardQSprite;
    public Sprite keyboardDSprite;
    public Sprite keyboardESprite;
    public Sprite keyboardLSprite;
    public Sprite keyboard1Sprite;
    public Sprite keyboard2Sprite;
    public Sprite keyboardShiftSprite;
    public Sprite keyboardMouseClickSprite;

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
                spriteList.Add(gamepadRtSprite);
            }
            if (control == Control.Skill2) {
                spriteList.Add(gamepadLtSprite);
            }
            if (control == Control.SwapWeapon) {
                spriteList.Add(gamepadR3Sprite);
            }
            if (control == Control.Shoot) {
                spriteList.Add(gamepadLtSprite);
            }
            if (control == Control.Interact) {
                spriteList.Add(gamepadASprite);
            }
            if (control == Control.Move) {
                spriteList.Add(gamepadRLeftRightSprite);
            }
            if (control == Control.Run) {
                spriteList.Add(gamepadLbSprite);
            }
            if (control == Control.LightSwitch) {
                spriteList.Add(gamepadL3Sprite);
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
            if (control == Control.SwapWeapon) {
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
                spriteList.Add(keyboardLSprite);
            }
            if (control == Control.Move) {
                spriteList.Add(keyboardQSprite);
                spriteList.Add(keyboardDSprite);
            }
        }

        return spriteList;
    }
}
