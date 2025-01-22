using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSave : MonoBehaviour
{

    public static PlayerSave Instance;

    [SerializeField] private GunSO initialActiveGun;

    private void Awake() {
        Instance = this;
    }

    public void SavePrimaryActiveGunSO(GunSO gunSO) {
        if (gunSO == null) return;
        Debug.Log("SavePrimaryActiveGunSO " + gunSO);
        ES3.Save("primaryActiveGunSO", gunSO);
    }

    public void SavePlayerMetaStats() {
        PlayerStats.Instance.SaveMetaBuffValues();
        PlayerShoot.Instance.SaveAllGunStats();
    } 

    public GunSO GetPrimaryActiveGun() {
        GunSO activeGun = ES3.Load("primaryActiveGunSO", initialActiveGun);

        if(activeGun == null) {
            return initialActiveGun;
        }

        return activeGun;
    }

    public void SetSecondaryActiveGunSO(GunSO gunSO) {
        ES3.Save("secondaryActiveGunSO", gunSO);
    }

    public GunSO GetSecondaryActiveGun() {
        GunSO activeGun = ES3.Load("secondaryActiveGunSO", initialActiveGun);
        if (activeGun == null) {
            return initialActiveGun;
        }

        return activeGun;
    }

}
