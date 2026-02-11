using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSave : MonoBehaviour
{

    public static PlayerSave Instance;

    [SerializeField] private GunSO initialActiveGun;
    [SerializeField] private List<GunSO> allGunsList;
    [SerializeField] private List<SkillSO> allSkillsList;
    [SerializeField] private List<SkillSO> initialSkillsUnlockedList;
    private List<SkillSO> newSkillsUnlockedList = new List<SkillSO>();
    private List<SkillSO> skillsUnlockedList = new List<SkillSO>();
    private List<SkillSO> skillsUnlockedList_HordeMode = new List<SkillSO>();

    private bool playerUnlockedFlagCarry;

    private void Awake() {
        Instance = this;
        LoadUnlockedSkills();
    }

    private void Start() {
        playerUnlockedFlagCarry = MetaProgressionManager.Instance.GetPlayerUnlockedFlagCarry();
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level && LevelManager.Instance.IsHordeMode()) {
            LoadUnlockedSkills_HordeMode();
        }
    }

    public void SavePrimaryActiveGunSO(GunSO gunSO) {
        if (gunSO == null) return;

        ES3.Save("primaryActiveGunSO", gunSO.gunType);
    }

    public void SaveSecondaryActiveGunSO(GunSO gunSO) {
        if (gunSO == null) return;

        ES3.Save("secondaryActiveGunSO", gunSO.gunType);
    }

    public void SavePlayerMetaStats() {
        PlayerStats.Instance.SaveMetaBuffValues();
        PlayerShoot.Instance.SaveAllGunStats(true);
    }

    public GunSO.GunType GetPrimaryActiveGunType() {
        if (!ES3.KeyExists("primaryActiveGunSO"))
            return initialActiveGun.gunType;

        try {
            // Essaye de charger l’enum (nouveau format)
            return ES3.Load<GunSO.GunType>("primaryActiveGunSO");
        }
        catch (System.InvalidOperationException) {
            // Fallback : ancien format = ScriptableObject
            GunSO oldGunSO = ES3.Load<GunSO>("primaryActiveGunSO");
            GunSO.GunType gunTypeFromOldSO = oldGunSO.gunType;

            // Convertit la save au nouveau format
            SavePrimaryActiveGunSO(oldGunSO);

            return gunTypeFromOldSO;
        }
    }

    public GunSO.GunType GetSecondaryActiveGunType() {
        try {
            // Essaye de lire en tant que GunType (nouveau format)
            return ES3.Load<GunSO.GunType>("secondaryActiveGunSO");
        }
        catch (System.InvalidOperationException) {
            // Fallback : l'ancien fichier contenait un ScriptableObject
            GunSO oldGunSO = ES3.Load<GunSO>("secondaryActiveGunSO");
            GunSO.GunType gunTypeFromOldSO = oldGunSO.gunType;

            // Réécris la sauvegarde en format propre pour la suite
            SaveSecondaryActiveGunSO(oldGunSO);

            return gunTypeFromOldSO;
        }
    }

    public bool GetSecondaryGunIsEquipped() {
        if(ES3.KeyExists("secondaryActiveGunSO")) {
            return true;
        } else {
            return false;
        }
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

    public List<GunSO> GetUnlockedGunSOList() {
        List<GunSO> unlockedGunSOList = new List<GunSO>();

        foreach (GunSO gunSO in allGunsList) {
            if(MetaProgressionManager.Instance.GetGunUnlocked(gunSO)) {
                unlockedGunSOList.Add(gunSO);
            }
        }

        return unlockedGunSOList;
    }

    public List<GunSO> GetHordeModeUnlockedGunSOList() {
        List<GunSO> unlockedGunSOList = new List<GunSO>();

        foreach (GunSO gunSO in allGunsList) {

            if (HordeModeProgressionManager.Instance.GetWeaponUnlocked(gunSO.gunType)) {
                unlockedGunSOList.Add(gunSO);
            }

        }

        return unlockedGunSOList;
    }

    public List<GunSO> GetAllGunSOs() {
        return allGunsList;
    }

    #region SKILLS
    public void HUBUnlockNewSkill(SkillSO skillSO) {
        newSkillsUnlockedList.Add(skillSO);
    }

    public void SaveNewUnlockedSkills() {
        if (newSkillsUnlockedList.Count == 0) return; // rapide exit si vide

        foreach (SkillSO skillSO in newSkillsUnlockedList) {
            Debug.Log("SaveNewUnlockedSkill " + skillSO);
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
    public void LoadUnlockedSkills_HordeMode() {
        foreach (SkillSO skillSO in initialSkillsUnlockedList) {
            skillsUnlockedList_HordeMode.Add(skillSO);
        }

        foreach (SkillSO skillSO in allSkillsList) {
            if (HordeModeProgressionManager.Instance.GetSkillUnlocked(skillSO) && !skillsUnlockedList_HordeMode.Contains(skillSO)) {
                skillsUnlockedList_HordeMode.Add(skillSO);
            }
        }
    }

    public List<SkillSO> GetAllSkillsUnlocked() {
        if(DebugManager.Instance.GetDebugMode_AllSkillsUnlocked()) {
            return allSkillsList;
        } else {
            return skillsUnlockedList;
        }
    }

    public List<SkillSO> GetAllSkillsUnlocked_HordeMode() {
        if (DebugManager.Instance.GetDebugMode_AllSkillsUnlocked()) {
            return allSkillsList;
        }
        else {
            return skillsUnlockedList_HordeMode;
        }
    }

    public List<SkillSO> GetAllActiveSkills() {
        List<SkillSO> allActiveSkills = new List<SkillSO>();

        foreach (SkillSO skillSO in allSkillsList) {

            if (skillSO.itemType == MerchantItem.MerchantItemType.ActiveSkill) {
                allActiveSkills.Add(skillSO);
            }
        }

        return allActiveSkills;
    }
    public List<SkillSO> GetAllPassiveSkills() {
        List<SkillSO> allPassiveSkills = new List<SkillSO>();

        foreach (SkillSO skillSO in allSkillsList) {

            if (skillSO.itemType == MerchantItem.MerchantItemType.PassiveSkill) {
                allPassiveSkills.Add(skillSO);
            }
        }

        return allPassiveSkills;
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
