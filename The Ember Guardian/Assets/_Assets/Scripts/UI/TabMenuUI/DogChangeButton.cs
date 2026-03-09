using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DogChangeButton : ButtonUI {

    private Dog.DogType linkedDogType;
    private Button button;

    [SerializeField] private Image dogIconImage;

    private void Awake() {
        button = GetComponent<Button>();
    }

    protected override void Start() {
        Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;
        Dog.Instance.OnDogSkinChanged += Dog_OnDogSkinChanged;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            //button.enabled = false;
            button.onClick.AddListener(() => {
                DogButtonPressLevel();
            });

        }
        else {

            button.onClick.AddListener(() => {
                DogButtonPressHub();
            });
        }
        UpdateDogIconImage(Dog.Instance.GetDogSkin());
    }

    private void Dog_OnDogSkinChanged(object sender, Dog.OnDogTypeChangedEventArgs e) {
        UpdateDogIconImage(Dog.Instance.GetDogSkin());

        if (!e.selectedFromMenu) return;
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    private void Dog_OnDogTypeChanged(object sender, Dog.OnDogTypeChangedEventArgs e) {
        UpdateDogIconImage(Dog.Instance.GetDogSkin());

        if (!e.selectedFromMenu) return;
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    private void DogButtonPressHub() {
        //OnAnyWeaponChangeButtonPressed?.Invoke(this, EventArgs.Empty);

        if (!DogStats.Instance.GetRetreiverUnlocked() && !DogStats.Instance.GetDarkCompanionUnlocked()) return;

        //ChangeWeaponPanel.Instance.SetPrimaryWeaponSwap(isPrimaryWeaponButton);

        //if (ChangeWeaponPanel.Instance.GetJustPressedByOtherWeaponButton(this)) {
        //    ChangeWeaponPanel.Instance.SetLastWeaponChangeButton(this);
        //    return;
        //};

        //ChangeWeaponPanel.Instance.SetLastWeaponChangeButton(this);
        ChangeDogPanel.Instance.OpenClosePanel();
        ;
    }

    private void DogButtonPressLevel() {
        //OnAnyWeaponChangeButtonPressed?.Invoke(this, EventArgs.Empty);
    }


    private void UpdateDogIconImage(Dog.DogSkin dogType) {
        dogIconImage.sprite = DogStats.Instance.GetDogIconSprite(dogType);
        dogIconImage.color = Color.white;
    }

    //public void SetLinkedDog(Dog.DogType dogType) {
    //    linkedDogType = dogType;

    //    UpdateDogIconImage(dogType);
    //}

    public Dog.DogType GetLinkedDog() {
        return linkedDogType;
    }
}
