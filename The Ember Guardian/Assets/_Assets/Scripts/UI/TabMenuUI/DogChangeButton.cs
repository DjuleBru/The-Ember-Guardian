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
        UpdateDogIconImage(Dog.Instance.GetDogType());
    }

    private void Dog_OnDogTypeChanged(object sender, System.EventArgs e) {
        UpdateDogIconImage(Dog.Instance.GetDogType());
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    private void DogButtonPressHub() {
        //OnAnyWeaponChangeButtonPressed?.Invoke(this, EventArgs.Empty);

        //if (PlayerShoot.Instance.GetUnlockedGunSOList().Count <= 1) return;

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


    private void UpdateDogIconImage(Dog.DogType dogType) {
        dogIconImage.sprite = DogStats.Instance.GetDogIconSprite(dogType);
        dogIconImage.color = Color.white;
    }

    public void SetLinkedDog(Dog.DogType dogType) {
        linkedDogType = dogType;

        UpdateDogIconImage(dogType);
    }

    public Dog.DogType GetLinkedDog() {
        return linkedDogType;
    }
}
