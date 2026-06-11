using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalUI_UnlockableTemplate : MonoBehaviour
{
    [SerializeField] private LevelSO.Unlockable unlockable;
    [SerializeField] private GameObject backgroundGO;
    [SerializeField] private GameObject fillGO;
    [SerializeField] private GunSO gunSO;

    public LevelSO.Unlockable GetUnlockable() {
        return unlockable;
    }

    public void LoadUnlockableUnlocked() {
        bool unlocked = false;

        switch(unlockable) {
            case LevelSO.Unlockable.NPC_Architect:
                unlocked = MetaProgressionManager.Instance.GetMerchantUnlocked(HubMerchant.HubMerchantType.StructuresMerchant);
                break;
            case LevelSO.Unlockable.NPC_Armorer:
                unlocked = MetaProgressionManager.Instance.GetMerchantUnlocked(HubMerchant.HubMerchantType.GunMerchant);
                break;
                case LevelSO.Unlockable.NPC_ArchitectTable:
                unlocked = MetaProgressionManager.Instance.GetMerchantUnlocked(HubMerchant.HubMerchantType.ArchitectTable);
                break;
                case LevelSO.Unlockable.NPC_Mushroom:
                unlocked = MetaProgressionManager.Instance.GetMerchantUnlocked(HubMerchant.HubMerchantType.MushroomMerchant);
                break;
                case LevelSO.Unlockable.NPC_Tamer:
                unlocked = MetaProgressionManager.Instance.GetMerchantUnlocked(HubMerchant.HubMerchantType.DogTamer);
                break;
                case LevelSO.Unlockable.NPC_Trainer:
                unlocked = MetaProgressionManager.Instance.GetMerchantUnlocked(HubMerchant.HubMerchantType.HeroMerchant);
                break;
                case LevelSO.Unlockable.NPC_Watcher:
                unlocked = MetaProgressionManager.Instance.GetMerchantUnlocked(HubMerchant.HubMerchantType.WorkerMerchant);
                break;
                case LevelSO.Unlockable.Weapon_AA:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Weapon_AR:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Weapon_GL:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Weapon_LMG:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Weapon_Minigun:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Weapon_Pistol:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Weapon_Revolver:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Weapon_RL:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Weapon_Shotgun:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Weapon_SMG:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Weapon_Sniper:
                unlocked = MetaProgressionManager.Instance.GetGunUnlocked(gunSO);
                break;
                case LevelSO.Unlockable.Dog_DarkCompanion:
                unlocked = DogStats.Instance.GetDogUnlocked(Dog.DogType.DarkCompanion);
                break;
                case LevelSO.Unlockable.Dog_Golden:
                unlocked = DogStats.Instance.GetDogUnlocked(Dog.DogType.GoldenRetreiver);
                break;
                case LevelSO.Unlockable.Robodog:
                unlocked = DogStats.Instance.GetDogUnlocked(Dog.DogType.Robodog);
                break;
            case LevelSO.Unlockable.Worker_Engineer:
                unlocked = MetaProgressionManager.Instance.GetMerchantItemUnlocked(StructureSO.StructureType.engineerShrine.ToString() + "1");
                break;
                case LevelSO.Unlockable.Worker_Guard:
                unlocked = MetaProgressionManager.Instance.GetMerchantItemUnlocked(StructureSO.StructureType.guardShrine.ToString() + "1");
                break;
                case LevelSO.Unlockable.Worker_Miner:
                unlocked = MetaProgressionManager.Instance.GetMerchantItemUnlocked(StructureSO.StructureType.minerShrine.ToString() + "1");
                break;

                case LevelSO.Unlockable.Traps:
                unlocked = VideoTipManager.Instance.GetTrapTipShown();
                break;
                case LevelSO.Unlockable.CarryFlag:
                unlocked = VideoTipManager.Instance.GetHuntingFlagTipShown();
                break;
                case LevelSO.Unlockable.Mines:
                unlocked = VideoTipManager.Instance.GetMinesTipShown();
                break;
                case LevelSO.Unlockable.Scavengables:
                unlocked = VideoTipManager.Instance.GetScavengablesTipShown();
                break;
                case LevelSO.Unlockable.ScavengableObstacles:
                unlocked = VideoTipManager.Instance.GetScavengableObstaclesTipShown();
                break;
                case LevelSO.Unlockable.ThroneArtifact:
                unlocked = VideoTipManager.Instance.GetThroneArtifactTipShown();
                break;
                case LevelSO.Unlockable.Trials:
                unlocked = VideoTipManager.Instance.GetTrialTipsShown();
                break;

        }

        backgroundGO.SetActive(!unlocked);
        fillGO.SetActive(unlocked);
    }

}
