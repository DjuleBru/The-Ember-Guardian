using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DogSkinReplaceButton : ButtonUI {
    [SerializeField] private Image dogIconImage;

    [SerializeField] private Material lockedSkinMaterial;
    [SerializeField] private Material emptyMaterial;
    private Button button;
    private Dog.DogType linkedDogType;
    private Dog.DogSkin linkedDogSkinType;

    private bool skinUnlocked;
    public static event EventHandler OnDogSkinSwapped;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            SwapDogSkin();
        });
    }

    protected override void Start() {
        base.Start();
        DogStats.Instance.OnNewDogSkinUnlocked += DogStats_OnNewDogSkinUnlocked;
    }

    private void SwapDogSkin() {
        if (!skinUnlocked) return;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {
            HordeModeUI.Instance.SetSelectedDogSkin(linkedDogSkinType);
            HordeModeUI.Instance.SetSelectedDog(linkedDogType);
            return;
        }

        Dog.Instance.SetDogType(linkedDogType);
        Dog.Instance.SetDogSkin(linkedDogSkinType, true);
        OnDogSkinSwapped?.Invoke(this, EventArgs.Empty);
        ChangeDogPanel.Instance.OpenClosePanel();
    }

    private void DogStats_OnNewDogSkinUnlocked(object sender, EventArgs e) {
        RefreshSkinUnlocked();
    }

    private void UpdateDogIconImage(Dog.DogSkin dogSkin) {

        dogIconImage.sprite = DogStats.Instance.GetDogIconSprite(dogSkin);

    }

    public void SetLinkedDogSkin(Dog.DogSkin skin) {
        linkedDogSkinType = skin;

        RefreshSkinUnlocked();
        UpdateDogIconImage(skin);
    }

    private void RefreshSkinUnlocked() {
        skinUnlocked = DogStats.Instance.GetDogSkinUnlocked(linkedDogSkinType);

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {
            skinUnlocked = true;
        }

        if (!skinUnlocked) {
            dogIconImage.material = lockedSkinMaterial;
        }
        else {
            dogIconImage.material = emptyMaterial;
        }
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

    protected override void OnDestroy() {
        base.OnDestroy();

        DogStats.Instance.OnNewDogSkinUnlocked -= DogStats_OnNewDogSkinUnlocked;
    }
}
