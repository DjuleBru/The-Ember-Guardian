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

    public void SetPrimaryActiveGunSO(GunSO gunSO) {
        ES3.Save("primaryActiveGunSO", gunSO);
    }

    public GunSO GetPrimaryActiveGun() {
        GunSO activeGun = ES3.Load("primaryActiveGunSO", initialActiveGun);
        if(activeGun == null) {
            return initialActiveGun;
        }

        Debug.Log(activeGun.ToString());
        return activeGun;
    }

}
