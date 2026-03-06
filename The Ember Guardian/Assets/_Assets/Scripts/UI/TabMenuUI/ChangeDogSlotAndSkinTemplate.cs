using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDogSlotAndSkinTemplate : MonoBehaviour
{

    [SerializeField] protected DogReplaceButton dogReplaceButton;
    [SerializeField] protected DogSkinReplaceButton dogSkinReplaceButton;
    [SerializeField] protected Transform changeDogSkinContainerTemplate;
    [SerializeField] protected Transform changeDogSkinSlotTemplate;

    public void SetLinkedDog(Dog.DogType dogType) {
        dogReplaceButton.gameObject.SetActive(true);
        dogReplaceButton.SetLinkedDog(dogType);

        foreach(Dog.DogTypeSkins dogTypeSkin in Dog.Instance.GetDogTypeSkins()) {
            if (dogTypeSkin.dogType != dogType) continue;

            foreach(Dog.DogSkinDLCLink dogTypeDLCSkin in dogTypeSkin.dogTypeDLCSkins) {
                // Check if DLC unlocked here
                //if (dogTypeDLCSkin.linkedDLC == DLCManager.DLCType.None) continue;

                DogSkinReplaceButton skinReplaceButton = Instantiate(changeDogSkinSlotTemplate, changeDogSkinContainerTemplate).GetComponent<DogSkinReplaceButton>();
                skinReplaceButton.SetLinkedDogSkin(dogTypeDLCSkin.dogSkin);
                skinReplaceButton.SetLinkedDogType(dogType);
            }

        }

        changeDogSkinSlotTemplate.gameObject.SetActive(false);
    }

    public GameObject GetDogReplaceButton() {
        return dogReplaceButton.gameObject;
    }
}
