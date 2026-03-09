using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DogReplaceButton : ButtonUI
{
    [SerializeField] private Image dogIconImage;

    private Button button;
    private Dog.DogType linkedDogType;

    public static event EventHandler OnDogSwapped;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            SwapDog();
        });
    }


    private void SwapDog() {
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {

            HordeModeUI.Instance.SetSelectedDogSkin(DogStats.Instance.GetDefaultDogSkin(linkedDogType));
            HordeModeUI.Instance.SetSelectedDog(linkedDogType);

        } else {

            Dog.Instance.SetDogType(linkedDogType, true);
            Dog.Instance.SetDogSkin(DogStats.Instance.GetDefaultDogSkin(linkedDogType), true);
            OnDogSwapped?.Invoke(this, EventArgs.Empty);
            ChangeDogPanel.Instance.OpenClosePanel();

        }

    }

    private void UpdateDogIconImage(Dog.DogType dogType) {

        dogIconImage.sprite = DogStats.Instance.GetDogIconSprite(dogType);

    }

    public void SetLinkedDog(Dog.DogType type) {
        linkedDogType = type;
        UpdateDogIconImage(type);
    }

    protected override void ButtonUI_OnAnyButtonSelected(object sender, EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;
        ButtonUI buttonUI = sender as ButtonUI;

        if (this == buttonUI) {
            buttonSelected = true;
        }

        if (this != buttonUI && buttonSelected) {
            buttonSelected = false;
        }
    }
}
