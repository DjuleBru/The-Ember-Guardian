using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSave : MonoBehaviour
{

    public static PlayerSave Instance;

    [SerializeField] private GunSO initialActiveGun;

    private bool playerUnlockedFlagCarry;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        playerUnlockedFlagCarry = MetaProgressionManager.Instance.GetPlayerUnlockedFlagCarry();
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }

    public void SavePrimaryActiveGunSO(GunSO gunSO) {
        if (gunSO == null) return;
        ES3.Save("primaryActiveGunSO", gunSO);
    }

    public void SaveSecondaryActiveGunSO(GunSO gunSO) {
        if (gunSO == null) return;
        ES3.Save("secondaryActiveGunSO", gunSO);
    }

    public void SavePlayerMetaStats() {
        PlayerStats.Instance.SaveMetaBuffValues();
        PlayerShoot.Instance.SaveAllGunStats();
    } 

    public GunSO GetPrimaryActiveGun() {
        GunSO activeGun = ES3.Load("primaryActiveGunSO", initialActiveGun);
        if(activeGun == null || activeGun.name == "") {
            return initialActiveGun;
        }

        return activeGun;
    }


    public GunSO GetSecondaryActiveGun() {
        GunSO activeGun = ES3.Load("secondaryActiveGunSO", initialActiveGun);
        if (activeGun == null) {
            return initialActiveGun;
        }

        return activeGun;
    }


    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, EventArgs e) {
        if (GetPlayerUnlockedFlagCarry()) return;

        HubMerchant hubMerchant = (HubMerchant)sender;
        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.WorkerMerchant) {
            playerUnlockedFlagCarry = true;
            Debug.Log("Unlock glaf");
            MetaProgressionManager.Instance.SetPlayerUnlockedFlagCarry(true);
        }
    }

    public void SetPlayerUnlockedFlagCarry() {
        playerUnlockedFlagCarry = true;
        MetaProgressionManager.Instance.SetPlayerUnlockedFlagCarry(true);
    }

    public bool GetPlayerUnlockedFlagCarry() {
        return playerUnlockedFlagCarry;
    }
    private void OnDestroy() {
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }

}
