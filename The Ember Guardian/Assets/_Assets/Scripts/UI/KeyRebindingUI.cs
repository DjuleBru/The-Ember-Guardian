using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KeyRebindingUI : MonoBehaviour
{
    public static KeyRebindingUI Instance;

    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button interactButton;
    [SerializeField] private Button runButton;
    [SerializeField] private Button rollButton;

    [SerializeField] private Button shootButton;
    [SerializeField] private Button reloadButton;
    [SerializeField] private Button secondaryButton;
    [SerializeField] private Button ability1Button;
    [SerializeField] private Button ability2Button;
    [SerializeField] private Button selectPrimaryGunButton;
    [SerializeField] private Button selectSecondaryGunButton;

    [SerializeField] private Button callDoggoButton;
    [SerializeField] private Button torchButton;
    [SerializeField] private Button hoverWorkersButton;
    [SerializeField] private Button buildingFunctionRightButton;
    [SerializeField] private Button buildingFunctionLeftButton;

    [SerializeField] private Button pauseButton;
    [SerializeField] private Button characterMenuButton;

    [SerializeField] private TextMeshProUGUI moveLeftText;
    [SerializeField] private TextMeshProUGUI moveRightText;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private TextMeshProUGUI runText;
    [SerializeField] private TextMeshProUGUI rollText;

    [SerializeField] private TextMeshProUGUI shootText;
    [SerializeField] private TextMeshProUGUI reloadText;
    [SerializeField] private TextMeshProUGUI secondaryText;
    [SerializeField] private TextMeshProUGUI ability1Text;
    [SerializeField] private TextMeshProUGUI ability2Text;
    [SerializeField] private TextMeshProUGUI selectPrimaryGunText;
    [SerializeField] private TextMeshProUGUI selectSecondaryGunText;

    [SerializeField] private TextMeshProUGUI hoverWorkersText;
    [SerializeField] private TextMeshProUGUI callDoggoText;
    [SerializeField] private TextMeshProUGUI torchText;
    [SerializeField] private TextMeshProUGUI buildingFunctionLeftText;
    [SerializeField] private TextMeshProUGUI buildingFunctionRightText;

    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private TextMeshProUGUI characterMenuText;

    private void Awake() {
        Instance = this;
    }

    public void UpdateVisual() {
        moveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.moveLeft);
        moveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.moveRight);
        interactText.text = GameInput.Instance.GetBindingText(GameInput.Binding.interact);
        runText.text = GameInput.Instance.GetBindingText(GameInput.Binding.run);
        rollText.text = GameInput.Instance.GetBindingText(GameInput.Binding.roll);

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
}
