using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameInput;

public class KeyRebindingUI : MonoBehaviour
{
    public static KeyRebindingUI Instance;

    [SerializeField] protected GameObject waitingForRebindGameObject;

    [SerializeField] protected Button resetBindingsButton;
    [SerializeField] protected Button moveLeftButton;
    [SerializeField] protected Button moveRightButton;
    [SerializeField] protected Button interactButton;
    [SerializeField] protected Button runButton;
    [SerializeField] protected Button rollButton;
    [SerializeField] protected Button crouchButton;
    [SerializeField] protected Button meleeAttackButton;

    [SerializeField] protected Button shootButton;
    [SerializeField] protected Button reloadButton;
    [SerializeField] protected Button secondaryButton;
    [SerializeField] protected Button ability1Button;
    [SerializeField] protected Button ability2Button;
    [SerializeField] protected Button selectPrimaryGunButton;
    [SerializeField] protected Button selectSecondaryGunButton;

    [SerializeField] protected Button callDoggoButton;
    [SerializeField] protected Button torchButton;
    [SerializeField] protected Button hoverWorkersButton;
    [SerializeField] protected Button buildingFunctionRightButton;
    [SerializeField] protected Button buildingFunctionLeftButton;

    [SerializeField] protected Button pauseButton;
    [SerializeField] protected Button characterMenuButton;

    [SerializeField] protected TextMeshProUGUI moveLeftText;
    [SerializeField] protected TextMeshProUGUI moveRightText;
    [SerializeField] protected TextMeshProUGUI interactText;
    [SerializeField] protected TextMeshProUGUI runText;
    [SerializeField] protected TextMeshProUGUI rollText;
    [SerializeField] protected TextMeshProUGUI crouchText;
    [SerializeField] protected TextMeshProUGUI meleeAttackText;

    [SerializeField] protected TextMeshProUGUI shootText;
    [SerializeField] protected TextMeshProUGUI reloadText;
    [SerializeField] protected TextMeshProUGUI secondaryText;
    [SerializeField] protected TextMeshProUGUI ability1Text;
    [SerializeField] protected TextMeshProUGUI ability2Text;
    [SerializeField] protected TextMeshProUGUI selectPrimaryGunText;
    [SerializeField] protected TextMeshProUGUI selectSecondaryGunText;

    [SerializeField] protected TextMeshProUGUI hoverWorkersText;
    [SerializeField] protected TextMeshProUGUI callDoggoText;
    [SerializeField] protected TextMeshProUGUI torchText;
    [SerializeField] protected TextMeshProUGUI buildingFunctionLeftText;
    [SerializeField] protected TextMeshProUGUI buildingFunctionRightText;

    [SerializeField] protected TextMeshProUGUI pauseText;
    [SerializeField] protected TextMeshProUGUI characterMenuText;

    protected virtual void Awake() {
        SetInstance();
        waitingForRebindGameObject.gameObject.SetActive(false);

        resetBindingsButton.onClick.AddListener(() => {
            ResetBindingsToDefault();
            UpdateVisual();
        });

        InitializeRebindButtons(false);
    }

    protected virtual void InitializeRebindButtons(bool gamepad) {
        if(!gamepad) {
            moveLeftButton.onClick.AddListener(() => {
                RebindBinding(GameInput.Binding.moveLeft, gamepad);
            });
            moveRightButton.onClick.AddListener(() => {
                RebindBinding(GameInput.Binding.moveRight, gamepad);
            });
        }
       
        interactButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.interact, gamepad);
        });
        runButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.run, gamepad);
        });
        rollButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.roll, gamepad);
        });
        crouchButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.crouch, gamepad);
        });
        meleeAttackButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.meleeAttack, gamepad);
        });
        shootButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.shoot, gamepad);
        });
        reloadButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.reload, gamepad);
        });
        secondaryButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.secondary, gamepad);
        });
        ability1Button.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.ability1, gamepad);
        });
        ability2Button.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.ability2, gamepad);
        });

        if(!gamepad) {
            selectPrimaryGunButton.onClick.AddListener(() => {
                RebindBinding(GameInput.Binding.selectPrimaryGun, gamepad);
            });
            selectSecondaryGunButton.onClick.AddListener(() => {
                RebindBinding(GameInput.Binding.selectSecondaryGun, gamepad);
            });

        }

        //hoverWorkersButton.onClick.AddListener(() => {
        //    RebindBinding(GameInput.Binding.hoverWorkers, gamepad);
        //});

        callDoggoButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.callDoggo, gamepad);
        });
        torchButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.torchOnOff, gamepad);
        });
        buildingFunctionLeftButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.buildingFunctionLeft, gamepad);
        });
        buildingFunctionRightButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.buildingFunctionRight, gamepad);
        });
        pauseButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.pause, gamepad);
        });
        characterMenuButton.onClick.AddListener(() => {
            RebindBinding(GameInput.Binding.characterMenu, gamepad);
        });
    }

    protected virtual void SetInstance() {
        Instance = this;
    }

    public virtual void UpdateVisual() {
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.moveLeft);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.moveRight);
        interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.interact);
        runText.text = GameInput.Instance.GetBindingText(GameInput.Binding.run);
        rollText.text = GameInput.Instance.GetBindingText(GameInput.Binding.roll);
        crouchText.text = GameInput.Instance.GetBindingText(GameInput.Binding.crouch);
        meleeAttackText.text = GameInput.Instance.GetBindingText(GameInput.Binding.meleeAttack);

        shootText.text = GameInput.Instance.GetBindingText(GameInput.Binding.shoot);
        reloadText.text = GameInput.Instance.GetBindingText(GameInput.Binding.reload);
        secondaryText.text = GameInput.Instance.GetBindingText(GameInput.Binding.secondary);
        ability1Text.text = GameInput.Instance.GetBindingText(GameInput.Binding.ability1);
        ability2Text.text = GameInput.Instance.GetBindingText(GameInput.Binding.ability2);
        selectPrimaryGunText.text = GameInput.Instance.GetBindingText(GameInput.Binding.selectPrimaryGun);
        selectSecondaryGunText.text = GameInput.Instance.GetBindingText(GameInput.Binding.selectSecondaryGun);

        hoverWorkersText.text = GameInput.Instance.GetBindingText(GameInput.Binding.hoverWorkers);
        callDoggoText.text = GameInput.Instance.GetBindingText(GameInput.Binding.callDoggo);
        torchText.text = GameInput.Instance.GetBindingText(GameInput.Binding.torchOnOff);
        buildingFunctionLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.buildingFunctionLeft);
        buildingFunctionRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.buildingFunctionRight);

        pauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.pause);
        characterMenuText.text = GameInput.Instance.GetBindingText(GameInput.Binding.characterMenu);
    }

    protected virtual void RebindBinding(GameInput.Binding binding, bool gamepad) {
        ShowWaitingToRebind();

        if(PauseMenuUI.Instance != null) {
            PauseMenuUI.Instance.SetRebindingKey(true);
        }

        GameInput.Instance.RebindBinding(binding, () => {
            HideWaitingToRebind();
            UpdateVisual();

            if (PauseMenuUI.Instance != null) {
                PauseMenuUI.Instance.SetRebindingKeyAfterFrames(false);
            }
        }, gamepad);
        
    }

    public void ResetBindingsToDefault() {
        GameInput.Instance.ResetBindingsToDefault();
    }

    public void ShowWaitingToRebind() {
        waitingForRebindGameObject.SetActive(true);
    }

    public void HideWaitingToRebind() {
        waitingForRebindGameObject.SetActive(false);
    }
}
