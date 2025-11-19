using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDogPanel_HordeMode : ChangeDogPanel
{

    protected override void Start() {
        UpdateDogSlots(Dog.DogType.GermanShepherd);

        gameObject.SetActive(false);
    }

    protected override void UpdateDogSlots(Dog.DogType activeDogType) {
        changeDogSlotTemplate.gameObject.SetActive(true);
        emptyDogSlotTemplate.gameObject.SetActive(true);
        changeDogButtons = new List<GameObject>();

        foreach (Transform child in changeDogSlotContainer) {
            if (child == changeDogSlotTemplate || child == emptyDogSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Dog.DogType type in Enum.GetValues(typeof(Dog.DogType))) {

            if (DogStats.Instance.GetDogUnlocked(type)) {
                DogReplaceButton dogReplaceButton = Instantiate(changeDogSlotTemplate, changeDogSlotContainer).GetComponent<DogReplaceButton>();

                dogReplaceButton.SetLinkedDog(type);
                changeDogButtons.Add(dogReplaceButton.gameObject);
            }
            else {
                Instantiate(emptyDogSlotTemplate, changeDogSlotContainer);
            }

        }

        changeDogSlotTemplate.gameObject.SetActive(false);
        emptyDogSlotTemplate.gameObject.SetActive(false);

        SetupNavigation();
    }
}
