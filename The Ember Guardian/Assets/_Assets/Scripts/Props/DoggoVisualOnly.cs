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
    [SerializeField] private GameObject robodogGO;
    [SerializeField] private GameObject robodogBlueGO;

    private Dog.DogType dogType = Dog.DogType.GermanShepherd;

    private void Start() {

        if (ES3.KeyExists("DogStats")) {
            var dogData = ES3.Load<Dictionary<string, object>>("DogStats");
            dogType = GetValue(dogData, "dogType", Dog.DogType.GermanShepherd);
        }

        RefreshActiveDog(dogType);

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
        robodogGO.SetActive(false);

        if(germanShepherdLightGO != null) {
            germanShepherdLightGO.SetActive(false);
        }

        if(retreiverBrownGO != null) {
            retreiverBrownGO.SetActive(false);
        }

        if(darkCompanionRedGO != null) {
            darkCompanionRedGO.SetActive(false);
        }

        if(huskyGO != null) {
            huskyGO.SetActive(false);
        }

        if (robodogBlueGO != null) {
            robodogBlueGO.SetActive(false);
        }


        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {
            if (dogType == Dog.DogType.GermanShepherd) {
                germanShepherdGO.SetActive(true);
            }
            if (dogType == Dog.DogType.GoldenRetreiver) {
                retreiverGO.SetActive(true);
            }
            if (dogType == Dog.DogType.DarkCompanion) {
                darkCompanionGO.SetActive(true);
            }
            if (dogType == Dog.DogType.Robodog) {
                robodogGO.SetActive(true);
            }
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            bool retreiverUnlocked = DogStats.Instance.GetRetreiverUnlocked();
            bool darkCompanionUnlocked = DogStats.Instance.GetDarkCompanionUnlocked();
            bool robodogUnlocked = DogStats.Instance.GetRobodogUnlocked();

            if (dogType == Dog.DogType.GermanShepherd) {
                germanShepherdGO.SetActive(false);
                retreiverGO.SetActive(retreiverUnlocked);
                darkCompanionGO.SetActive(darkCompanionUnlocked);
                robodogGO.SetActive(robodogUnlocked);

            }

            if (dogType == Dog.DogType.GoldenRetreiver) {
                retreiverGO.SetActive(false);
                germanShepherdGO.SetActive(true);
                darkCompanionGO.SetActive(darkCompanionUnlocked);
                robodogGO.SetActive(robodogUnlocked);
            }

            if (dogType == Dog.DogType.DarkCompanion) {
                darkCompanionGO.SetActive(false);
                germanShepherdGO.SetActive(true);
                retreiverGO.SetActive(retreiverUnlocked);
                robodogGO.SetActive(robodogUnlocked);
            }

            if (dogType == Dog.DogType.Robodog) {
                robodogGO.SetActive(false);
                darkCompanionGO.SetActive(darkCompanionUnlocked);
                germanShepherdGO.SetActive(true);
                retreiverGO.SetActive(retreiverUnlocked);
            }
        }

        RandomizeAnimation(germanShepherdGO.GetComponent<Animator>());
        RandomizeAnimation(darkCompanionGO.GetComponent<Animator>());
        RandomizeAnimation(retreiverGO.GetComponent<Animator>());
        RandomizeAnimation(robodogGO.GetComponent<Animator>());

    }

    private void RefreshActiveDogSkin(Dog.DogSkin dogSkin) {
        Debug.Log("RefreshActiveDogSkin " + dogSkin);
        germanShepherdGO.SetActive(false);
        retreiverGO.SetActive(false);
        darkCompanionGO.SetActive(false);
        germanShepherdLightGO.SetActive(false);
        retreiverBrownGO.SetActive(false);
        darkCompanionRedGO.SetActive(false);
        huskyGO.SetActive(false);
        robodogGO.SetActive(false);
        robodogBlueGO.SetActive(false);

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {
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

            if (dogSkin == Dog.DogSkin.Robodog) {
                robodogGO.SetActive(true);
            }
            if (dogSkin == Dog.DogSkin.RobodogSkin1) {
                robodogBlueGO.SetActive(true);
            }
        }

        RandomizeAnimation(germanShepherdGO.GetComponent<Animator>());
        RandomizeAnimation(darkCompanionGO.GetComponent<Animator>());
        RandomizeAnimation(retreiverGO.GetComponent<Animator>());
        RandomizeAnimation(germanShepherdLightGO.GetComponent<Animator>());
        RandomizeAnimation(retreiverBrownGO.GetComponent<Animator>());
        RandomizeAnimation(darkCompanionRedGO.GetComponent<Animator>());
        RandomizeAnimation(huskyGO.GetComponent<Animator>());
        RandomizeAnimation(robodogGO.GetComponent<Animator>());
        RandomizeAnimation(robodogBlueGO.GetComponent<Animator>());

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
