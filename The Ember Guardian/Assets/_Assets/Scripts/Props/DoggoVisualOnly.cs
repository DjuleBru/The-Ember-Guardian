using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoggoVisualOnly : MonoBehaviour
{
    [SerializeField] private GameObject germanShepherdGO;
    [SerializeField] private GameObject germanShepherdLightGO;
    [SerializeField] private GameObject retreiverGO;
    [SerializeField] private GameObject retreiverBrownGO;
    [SerializeField] private GameObject darkCompanionGO;
    [SerializeField] private GameObject darkCompanionRedGO;
    [SerializeField] private GameObject huskyGO;

    private Dog.DogType dogType = Dog.DogType.GermanShepherd;

    private void Start() {

        Dog.DogSkin dogSkin = Dog.DogSkin.GermanShepherdSkin;

        if (ES3.KeyExists("DogStats")) {
            var dogData = ES3.Load<Dictionary<string, object>>("DogStats");

            dogType = GetValue(dogData, "dogType", Dog.DogType.GermanShepherd);

            string skinKey = dogType.ToString() + "_skin";
            dogSkin = ES3.Load(skinKey, Dog.DogSkin.GermanShepherdSkin);
        }

        // Security if people change save file manually
        if (!DLCManager.Instance.HasDLC(DogStats.Instance.GetDogSkinLinkedDLC(dogSkin))) {
            dogSkin = DogStats.Instance.GetDefaultDogSkin(dogType);
        };

        RefreshActiveDogSkin(dogSkin);

       if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;
       }
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {
            HordeModeUI.Instance.OnDogSelected += HordeModeUI_OnDogSelected;
            HordeModeUI.Instance.OnHordeModePanelOpened += HordeModeUI_OnDogSelected;
        }
    }

    private void HordeModeUI_OnDogSelected(object sender, System.EventArgs e) {
        RefreshActiveDogSkin(HordeModeCustomizationManager.Instance.GetSelectedDogSkin());
    }

    private void Dog_OnDogTypeChanged(object sender, Dog.OnDogTypeChangedEventArgs e) {
        RefreshActiveDog(Dog.Instance.GetDogType());
    }

    private void RefreshActiveDog(Dog.DogType dogType) {
        germanShepherdGO.SetActive(false);
        retreiverGO.SetActive(false);
        darkCompanionGO.SetActive(false);

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {

            germanShepherdLightGO.SetActive(false);
            retreiverBrownGO.SetActive(false);
            darkCompanionRedGO.SetActive(false);
            huskyGO.SetActive(false);

            if (dogType == Dog.DogType.GermanShepherd) {
                germanShepherdGO.SetActive(true);
            }
            if (dogType == Dog.DogType.GoldenRetreiver) {
                retreiverGO.SetActive(true);
            }
            if (dogType == Dog.DogType.DarkCompanion) {
                darkCompanionGO.SetActive(true);
            }
            RandomizeAnimation(germanShepherdLightGO.GetComponent<Animator>());
            RandomizeAnimation(retreiverBrownGO.GetComponent<Animator>());
            RandomizeAnimation(darkCompanionRedGO.GetComponent<Animator>());
            RandomizeAnimation(huskyGO.GetComponent<Animator>());
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            bool retreiverUnlocked = DogStats.Instance.GetRetreiverUnlocked();
            bool darkCompanionUnlocked = DogStats.Instance.GetDarkCompanionUnlocked();

            if (dogType == Dog.DogType.GermanShepherd) {
                germanShepherdGO.SetActive(false);
                retreiverGO.SetActive(retreiverUnlocked);
                darkCompanionGO.SetActive(darkCompanionUnlocked);

            }

            if (dogType == Dog.DogType.GoldenRetreiver) {
                retreiverGO.SetActive(false);
                germanShepherdGO.SetActive(true);
                darkCompanionGO.SetActive(darkCompanionUnlocked);
            }

            if (dogType == Dog.DogType.DarkCompanion) {
                darkCompanionGO.SetActive(false);
                germanShepherdGO.SetActive(true);
                retreiverGO.SetActive(retreiverUnlocked);
            }
        }

        RandomizeAnimation(germanShepherdGO.GetComponent<Animator>());
        RandomizeAnimation(darkCompanionGO.GetComponent<Animator>());
        RandomizeAnimation(retreiverGO.GetComponent<Animator>());

    }

    private void RefreshActiveDogSkin(Dog.DogSkin dogSkin) {
        germanShepherdGO.SetActive(false);
        retreiverGO.SetActive(false);
        darkCompanionGO.SetActive(false);

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {

            germanShepherdLightGO.SetActive(false);
            retreiverBrownGO.SetActive(false);
            darkCompanionRedGO.SetActive(false);
            huskyGO.SetActive(false);

            if (dogSkin == Dog.DogSkin.GermanShepherdSkin) {
                germanShepherdGO.SetActive(true);
            }
            if (dogSkin == Dog.DogSkin.GoldenRetreiverSkin) {
                retreiverGO.SetActive(true);
            }
            if (dogSkin == Dog.DogSkin.DarkCompanionSkin) {
                darkCompanionGO.SetActive(true);
            }

            if (dogSkin == Dog.DogSkin.GermanShepherdLight) {
                germanShepherdLightGO.SetActive(true);
            }
            if (dogSkin == Dog.DogSkin.GoldenBrownSkin) {
                retreiverBrownGO.SetActive(true);
            }
            if (dogSkin == Dog.DogSkin.DarkCompanionRed) {
                darkCompanionRedGO.SetActive(true);
            }
            if (dogSkin == Dog.DogSkin.Husky) {
                huskyGO.SetActive(true);
            }

            RandomizeAnimation(germanShepherdLightGO.GetComponent<Animator>());
            RandomizeAnimation(retreiverBrownGO.GetComponent<Animator>());
            RandomizeAnimation(darkCompanionRedGO.GetComponent<Animator>());
            RandomizeAnimation(huskyGO.GetComponent<Animator>());
        }

        RandomizeAnimation(germanShepherdGO.GetComponent<Animator>());
        RandomizeAnimation(darkCompanionGO.GetComponent<Animator>());
        RandomizeAnimation(retreiverGO.GetComponent<Animator>());

    }

    private void RandomizeAnimation(Animator animator) {
        float randomValue = UnityEngine.Random.value;
        if (randomValue < .33f) {
            animator.SetTrigger("Sit");
        }
        if (randomValue > .66f) {
            animator.SetTrigger("Sleep");
        }
    }
    private T GetValue<T>(Dictionary<string, object> dict, string key, T defaultValue) {
        if (dict.ContainsKey(key) && dict[key] is T value)
            return value;
        return defaultValue;
    }

}
