using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class DoggoVisualOnly : MonoBehaviour
{
    [SerializeField] private GameObject germanShepherdGO;
    [SerializeField] private GameObject retreiverGO;
    [SerializeField] private GameObject darkCompanionGO;

    private Dog.DogType dogType = Dog.DogType.GermanShepherd;

    private void Start() {
        var dogData = ES3.Load<Dictionary<string, object>>("DogStats");

        if (ES3.KeyExists("DogStats")) {
            dogType = GetValue(dogData, "dogType", Dog.DogType.GermanShepherd);
        }

        RefreshActiveDog();

       if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;
       }
    }

    private void Dog_OnDogTypeChanged(object sender, Dog.OnDogTypeChangedEventArgs e) {
        RefreshActiveDog();
    }

    private void RefreshActiveDog() {
        germanShepherdGO.SetActive(false);
        retreiverGO.SetActive(false);
        darkCompanionGO.SetActive(false);

        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu) {
            if (dogType == Dog.DogType.GermanShepherd) {
                germanShepherdGO.SetActive(true);
            }
            if (dogType == Dog.DogType.GoldenRetreiver) {
                retreiverGO.SetActive(true);
            }
            if (dogType == Dog.DogType.DarkCompanion) {
                darkCompanionGO.SetActive(true);
            }
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            bool retreiverUnlocked = DogStats.Instance.GetRetreiverUnlocked();
            bool darkCompanionUnlocked = DogStats.Instance.GetDarkCompanionUnlocked();

            dogType = Dog.Instance.GetDogType();

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
