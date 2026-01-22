using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeModeCustomizationManager : MonoBehaviour
{
    public static HordeModeCustomizationManager Instance;

    private GunSO.GunType selectedGunType;
    private Dog.DogType selectedDogType;
    private LevelSO.LevelEnvironment selectedEnvironmentType;
    [SerializeField] private LevelSO.LevelEnvironment debugEnvironmentType;
    [SerializeField] private bool useDebugEnv;

    private void Awake() {
        Instance = this;

        selectedDogType = ES3.Load("hordeModeDogType", Dog.DogType.GermanShepherd);
        selectedGunType = ES3.Load("hordeModeGunType", GunSO.GunType.Rifle);
        selectedEnvironmentType = ES3.Load("hordeModeEnvironment", LevelSO.LevelEnvironment.TheVerdantGraveyard);

        if (useDebugEnv) {
            selectedEnvironmentType = debugEnvironmentType;
        }
    }

    public void SetSelectedDog(Dog.DogType dogType) {
        //Debug.Log("SetSelectedDog " + dogType);
        ES3.Save("hordeModeDogType", dogType);
        selectedDogType = dogType;
    }

    public void SetSelectedWeapon(GunSO.GunType gunType) {
        //Debug.Log("SetSelectedWeapon " + gunType);
        ES3.Save("hordeModeGunType", gunType);
        selectedGunType = gunType;
    }

    public void SetSelectedEnvironment(LevelSO.LevelEnvironment environment) {
        //Debug.Log("SetSelectedEnvironment " + environment);
        ES3.Save("hordeModeEnvironment", environment);
        selectedEnvironmentType = environment;
    }

    public LevelSO.LevelEnvironment GetSelectedEnvironment() {
        return selectedEnvironmentType;
    }

    public Dog.DogType GetSelectedDogType() {
        //Debug.Log("GetSelectedDogType " + selectedDogType);
        return selectedDogType;
    }

    public GunSO.GunType GetSelectedWeaponType() {
        return selectedGunType;
    }

    public void SaveHordeModeParameters() {
        HordeModeCustomizationManager.Instance.SetSelectedWeapon(selectedGunType);
        HordeModeCustomizationManager.Instance.SetSelectedDog(selectedDogType);
        HordeModeCustomizationManager.Instance.SetSelectedEnvironment(selectedEnvironmentType);

        CampEditManager.Instance.SaveCampLayout();
    }
}
