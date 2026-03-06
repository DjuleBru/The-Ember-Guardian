using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DogSkinReplaceButton : ButtonUI {
    [SerializeField] private Image dogIconImage;

    private Button button;
    private Dog.DogType linkedDogType;
    private Dog.DogSkin linkedDogSkinType;

    public static event EventHandler OnDogSkinSwapped;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            SwapDogSkin();
        });
    }


    private void SwapDogSkin() {
        Dog.Instance.SetDogType(linkedDogType);
        Dog.Instance.SetDogSkin(linkedDogSkinType);
        OnDogSkinSwapped?.Invoke(this, EventArgs.Empty);
        ChangeDogPanel.Instance.OpenClosePanel();
    }

    private void UpdateDogIconImage(Dog.DogSkin dogSkin) {

        dogIconImage.sprite = DogStats.Instance.GetDogIconSprite(dogSkin);

    }

    public void SetLinkedDogSkin(Dog.DogSkin skin) {
        linkedDogSkinType = skin;
        UpdateDogIconImage(skin);
    }
    public void SetLinkedDogType(Dog.DogType type) {
        linkedDogType = type;
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
