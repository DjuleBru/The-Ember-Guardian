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

    public override void UpdateDogSlots(Dog.DogType activeDogType) {
        changeDogSlotTemplate.gameObject.SetActive(true);
        emptyDogSlotTemplate.gameObject.SetActive(true);
        changeDogButtons = new List<GameObject>();

        foreach (Transform child in changeDogSlotContainer) {
            if (child == changeDogSlotTemplate || child == emptyDogSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Dog.DogType type in Enum.GetValues(typeof(Dog.DogType))) {
            bool unlocked = false;
            if (type == Dog.DogType.GermanShepherd) {
                unlocked = true;
            }
            if (type == Dog.DogType.GoldenRetreiver) {
                if(HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.GoldenRetreiver)) {
                    unlocked = true;
                }
            }
            if (type == Dog.DogType.DarkCompanion) {
                if (HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.DarkCompanion)) {
                    unlocked = true;
                }
            }

            if (unlocked) {
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
