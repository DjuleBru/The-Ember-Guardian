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
        return ES3.Load("primaryActiveGunSO", initialActiveGun);
    }

}
