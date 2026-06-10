using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeDogSlotAndSkinTemplate : MonoBehaviour
{

    [SerializeField] protected DogReplaceButton dogReplaceButton;
    [SerializeField] protected DogSkinReplaceButton dogSkinReplaceButton;
    [SerializeField] protected Transform changeDogSkinContainerTemplate;
    [SerializeField] protected Transform changeDogSkinSlotTemplate;

    private List<Selectable> skinButtons = new List<Selectable>();
    public void SetLinkedDog(Dog.DogType dogType) {
        dogReplaceButton.gameObject.SetActive(true);
        dogReplaceButton.SetLinkedDog(dogType);

        foreach(Dog.DogTypeSkins dogTypeSkin in DogStats.Instance.GetDogTypeSkins()) {
            if (dogTypeSkin.dogType != dogType) continue;

            foreach(Dog.DogSkinDLCLink dogTypeDLCSkin in dogTypeSkin.dogTypeDLCSkins) {

                // Check if DLC unlocked here

                // Remove default skins
                if (dogTypeDLCSkin.dogSkin == Dog.DogSkin.DarkCompanionSkin || dogTypeDLCSkin.dogSkin == Dog.DogSkin.GermanShepherdSkin || dogTypeDLCSkin.dogSkin == Dog.DogSkin.GoldenRetreiverSkin || dogTypeDLCSkin.dogSkin == Dog.DogSkin.Robodog) continue;

                DogSkinReplaceButton skinReplaceButton = Instantiate(changeDogSkinSlotTemplate, changeDogSkinContainerTemplate).GetComponent<DogSkinReplaceButton>();
                skinReplaceButton.SetLinkedDogSkin(dogTypeDLCSkin.dogSkin);
                skinReplaceButton.SetLinkedDogType(dogType);

                skinButtons.Add(skinReplaceButton.GetComponent<Selectable>());
            }

        }

        changeDogSkinSlotTemplate.gameObject.SetActive(false);

        SetupNavigation();
    }

    public GameObject GetDogReplaceButton() {
        return dogReplaceButton.gameObject;
    }

    private void SetupNavigation() {

        Selectable dogSelectable = dogReplaceButton.GetComponent<Selectable>();

        if (skinButtons.Count == 0) return;

        // Navigation du DogSlot
        Navigation dogNav = dogSelectable.navigation;
        dogNav.mode = Navigation.Mode.Explicit;
        dogNav.selectOnRight = skinButtons[0];
        dogSelectable.navigation = dogNav;

        for (int i = 0; i < skinButtons.Count; i++) {

            Selectable skin = skinButtons[i];

            Navigation nav = skin.navigation;
            // gauche
            if (i == 0)
                nav.selectOnLeft = dogSelectable;
            else
                nav.selectOnLeft = skinButtons[i - 1];

            // droite
            if (i < skinButtons.Count - 1)
                nav.selectOnRight = skinButtons[i + 1];

            skin.navigation = nav;
        }
    }

}
