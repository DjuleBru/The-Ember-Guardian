using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSave : MonoBehaviour
{

    public static PlayerSave Instance;

    [SerializeField] private GunSO initialActiveGun;
    [SerializeField] private List<SkillSO> allSkillsList;
    [SerializeField] private List<SkillSO> initialSkillsUnlockedList;
    private List<SkillSO> newSkillsUnlockedList = new List<SkillSO>();
    private List<SkillSO> skillsUnlockedList = new List<SkillSO>();

    private bool playerUnlockedFlagCarry;

    private void Awake() {
        Instance = this;
        LoadUnlockedSkills();
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

    #region SKILLS
    public void HUBUnlockNewSkill(SkillSO skillSO) {
        newSkillsUnlockedList.Add(skillSO);
    }

    public void SaveNewUnlockedSkills() {
        foreach (SkillSO skillSO in newSkillsUnlockedList) {
            string key = skillSO.name + "_unlocked";
            ES3.Save(key, true);
        }
    }

    public void LoadUnlockedSkills() {
        foreach (SkillSO skillSO in initialSkillsUnlockedList) {
            skillsUnlockedList.Add(skillSO);
        }
        foreach (SkillSO skillSO in allSkillsList) {
            string key = skillSO.name + "_unlocked";
            bool unlocked = ES3.Load(key, false);

            if (unlocked) {
                skillsUnlockedList.Add(skillSO);
            }
        }
    }

    public List<SkillSO> GetAllSkillsUnlocked() {
        return skillsUnlockedList;
    }

    public List<SkillSO> GetActiveSkillsUnlocked() {
        List<SkillSO> activeSkillSOsUnlocked = new List<SkillSO>();

        foreach(SkillSO skillSO in skillsUnlockedList) {
            
            if(skillSO.itemType == MerchantItem.MerchantItemType.ActiveSkill) {
                activeSkillSOsUnlocked.Add(skillSO);
            }
        }

        return activeSkillSOsUnlocked;
    }
    public List<SkillSO> GetPassiveSkillsUnlocked() {
        List<SkillSO> passiveSkillSOsUnlocked = new List<SkillSO>();

        foreach (SkillSO skillSO in skillsUnlockedList) {

            if (skillSO.itemType == MerchantItem.MerchantItemType.PassiveSkill) {
                passiveSkillSOsUnlocked.Add(skillSO);
            }
        }

        return passiveSkillSOsUnlocked;
    }

    #endregion

    private void OnDestroy() {
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }

}
