using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyRebindingUI_Controller : KeyRebindingUI
{
    public static KeyRebindingUI_Controller Instance_Controller;

    [SerializeField] protected Button switchWeaponsButton;

    [SerializeField] protected Image moveLeftImg;
    [SerializeField] protected Image moveRightImg;
    [SerializeField] protected Image interactImg;
    [SerializeField] protected Image runImg;
    [SerializeField] protected Image rollImg;
    [SerializeField] protected Image meleeAttackImg;

    [SerializeField] protected Image shootImg;
    [SerializeField] protected Image reloadImg;
    [SerializeField] protected Image secondaryImg;
    [SerializeField] protected Image ability1Img;
    [SerializeField] protected Image ability2Img;
    [SerializeField] protected Image swapWeaponImg;

    [SerializeField] protected Image hoverWorkersImg;
    [SerializeField] protected Image callDoggoImg;
    [SerializeField] protected Image torchImg;
    [SerializeField] protected Image buildingFunctionLeftImg;
    [SerializeField] protected Image buildingFunctionRightImg;

    [SerializeField] protected Image pauseImg;
    [SerializeField] protected Image characterMenuImg;

    protected override void Awake() {
        SetInstance();
        waitingForRebindGameObject.gameObject.SetActive(false);

        resetBindingsButton.onClick.AddListener(() => {
            ResetBindingsToDefault();
            UpdateVisual();
        });

        InitializeRebindButtons(true);
    }
    protected override void SetInstance() {
        Instance_Controller = this;
    }

    protected override void InitializeRebindButtons(bool gamepad) {
        base.InitializeRebindButtons(gamepad);
        switchWeaponsButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.swapGun, gamepad);
        });
    }

    public override void UpdateVisual() {
        interactImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.interact, true);
        runImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.run, true);
        rollImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.roll, true);
        meleeAttackImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.meleeAttack, true);

        shootImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.shoot, true);
        reloadImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.reload, true);
        secondaryImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.secondary, true);
        ability1Img.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.ability1, true);
        ability2Img.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.ability2, true);
        swapWeaponImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.swapGun, true);

        callDoggoImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.callDoggo, true);
        torchImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.torchOnOff, true);
        buildingFunctionLeftImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.buildingFunctionLeft, true);
        buildingFunctionRightImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.buildingFunctionRight, true);

        pauseImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.pause, true);
        characterMenuImg.sprite = InputControlIcons.Instance.GetSingleControlIconSprite(GameInput.Binding.characterMenu, true);
    }

}
