using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementsProgressManager : MonoBehaviour
{
    [SerializeField] private CreatureSO finalBossCreatureSO;
    private bool mushroomMerchantUnlocked;
    private bool architectTableUnlocked;
    private bool fireFuelDepletionUnlocked;
    private bool fireOrbConversionRate;
    private bool fireFuelCapacity;

    private int hunterAmount;
    private int minerAmount;
    private int guardAmount;
    private int engineerAmount;

    private bool specialWave;
    private bool nightStarted;
    private bool playerShot;
    private bool dropppingMoreThanTreshold;
    private int successfulSurgeReloadsIndex;
    private int currentWorkersAssignedJobs;

    private bool pettingDog;
    private float pettingDogTimer;
    private float pettingDogTimerTreshold = 5f;

    private int resourcesDugByDogTreshold = 50;
    private int killCreaturesTreshold = 1000;
    private int killCreaturesInFireTreshold = 100;
    private int successfulSurgeReloadTreshold = 3;
    private int assignedJobsTreshold = 15;
    private int orbDroppedByWorkerTreshold = 20;


    private void Awake() {
        mushroomMerchantUnlocked = ES3.Load("mushroomMerchantUnlocked", false);
        architectTableUnlocked = ES3.Load("architectTableUnlocked", false);
        fireOrbConversionRate = ES3.Load("fireOrbConversionRate", false);
        fireFuelDepletionUnlocked = ES3.Load("fireFuelDepletionUnlocked", false);
        fireFuelCapacity = ES3.Load("fireFuelCapacity", false);
    }

    private void Start() {
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level || SceneLoader.Instance.GetSceneType() ==  SceneLoader.SceneType.Tutorial) {

            if(EndLevelArea.Instance != null) {
                EndLevelArea.Instance.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;
            }

            LevelUI_Locations.Instance.OnLocationTextShown += LevelUI_Locations_OnLocationTextShown;
            LevelManager.Instance.OnLevelSuccess += LevelManager_OnLevelSuccess;
            Fire.Instance.OnPrimordialFireLit += Fire_OnPrimordialFireLit;
            Creature.OnAnyMobDied += Creature_OnAnyMobDied;
            Creature.OnAnyCreatureKilledByDog += Creature_OnAnyCreatureKilledByDog;
            DogDigAbility.OnAnyResourceDug += DogDigAbility_OnAnyResourceDug;
            DayNightManager.Instance.OnNightStart += DayNightManager_OnNightStart;
            DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
            PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShot;
            Gun.OnAnySurgeReloadSuccess += Gun_OnAnySurgeReloadSuccess;
            PlayerShoot.Instance.OnSpinningBulletFail += PlayerShoot_OnSpinningBulletFail;
            WorkerAI.OnAnyWorkerAssignedJob += WorkerAI_OnAnyWorkerAssignedJob;
            Worker.OnAnyWorkerDied += Worker_OnAnyWorkerDied;
            Worker.OnAnyOrbDroppedByWorker += Worker_OnAnyOrbDroppedByWorker;
            Worker.OnAnyWorkerDroppedAllCurrencies += Worker_OnAnyWorkerDroppedAllCurrencies;
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            HubMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_OnAnyHubMerchantItemBought;
            CampEditManager.Instance.OnAnyChangeMade += CampEditManager_OnAnyChangeMade;
        }

        if(PetDog.Instance != null) {
            PetDog.Instance.OnPlayerStartedPettingDog += PetDog_OnPlayerStartedPettingDog;
            PetDog.Instance.OnPlayerStoppedPettingDog += PetDog_OnPlayerStoppedPettingDog;
        }
    }

    #region UPDATE RELATED
    private void Update() {
        if(pettingDog) {
            pettingDogTimer += Time.deltaTime;
            if(pettingDogTimer >= pettingDogTimerTreshold) {
                pettingDog = false;
                TryUnlockSuccess("PET_DOG");
            }
        }
    }

    private void PetDog_OnPlayerStoppedPettingDog(object sender, System.EventArgs e) {
        pettingDog = false;
    }

    private void PetDog_OnPlayerStartedPettingDog(object sender, System.EventArgs e) {
        pettingDogTimer = 0;
        pettingDog = true;
    }
    #endregion

    #region HUB

    private void CampEditManager_OnAnyChangeMade(object sender, System.EventArgs e) {
        TryUnlockSuccess("CUSTOMIZE_CAMP");
    }

    private void HubMerchantItem_OnAnyHubMerchantItemBought(object sender, System.EventArgs e) {
        if (sender is HUBMerchantItem_DogTamerItem) {
            HUBMerchantItem_DogTamerItem tamerItem = sender as HUBMerchantItem_DogTamerItem;

            if (tamerItem.GetDogTamerItemCategory() == HUBMerchantItem_DogTamerItem.DogTamerItemCategory.NewAbility) {
                TryUnlockSuccess("FIRST_DOG_ABILITY");
            }
        }

        if (sender is HubMerchantItem_ArchitectMerchantItem) {
            HubMerchantItem_ArchitectMerchantItem architectItem = sender as HubMerchantItem_ArchitectMerchantItem;

            if (architectItem.GetArchitectItemType() == HubMerchantItem_ArchitectMerchantItem.ArchitectItemType.FireFuelDepletion) {
                fireFuelDepletionUnlocked = true;
                ES3.Save("fireFuelDepletionUnlocked", true);

                if(fireOrbConversionRate && fireFuelCapacity) {
                    TryUnlockSuccess("ALL_FIRE_UPGRADES");
                }
            }

            if (architectItem.GetArchitectItemType() == HubMerchantItem_ArchitectMerchantItem.ArchitectItemType.FireOrbConversionRate) {
                fireOrbConversionRate = true;
                ES3.Save("fireOrbConversionRate", true);

                if (fireFuelDepletionUnlocked && fireFuelCapacity) {
                    TryUnlockSuccess("ALL_FIRE_UPGRADES");
                }
            }

            if (architectItem.GetArchitectItemType() == HubMerchantItem_ArchitectMerchantItem.ArchitectItemType.MaxFuelCapacity) {
                fireFuelCapacity = true;
                ES3.Save("fireFuelCapacity", true);

                if (fireOrbConversionRate && fireFuelDepletionUnlocked) {
                    TryUnlockSuccess("ALL_FIRE_UPGRADES");
                }
            }
        }


        if (sender is HubMerchantItem_GemMerchantItem) {
            HubMerchantItem_GemMerchantItem gemItem = sender as HubMerchantItem_GemMerchantItem;

            if (gemItem.GetStructureType() == StructureSO.StructureType.merchant_skills) {
                TryUnlockSuccess("ORB_ALCHEMIST");
            }
            if (gemItem.GetStructureType() == StructureSO.StructureType.merchant_traps) {
                TryUnlockSuccess("TRAPS_MERCHANT");
            }
        }
    }

    #endregion

    private void Worker_OnAnyWorkerDroppedAllCurrencies(object sender, System.EventArgs e) {
        if (dropppingMoreThanTreshold) {
            dropppingMoreThanTreshold = false;
            TryUnlockSuccess("EMBERLING_DROP");
        }
    }

    private void Worker_OnAnyOrbDroppedByWorker(object sender, System.EventArgs e) {
        Worker worker = sender as Worker;
        if ((worker.GetCurrencyAmount(PlayerCurrencies.CurrencyType.bigBlueOrb) + worker.GetCurrencyAmount(PlayerCurrencies.CurrencyType.smallBlueOrb)) > orbDroppedByWorkerTreshold - 1) {
            dropppingMoreThanTreshold = true;
        }
    }

    private void Worker_OnAnyWorkerDied(object sender, System.EventArgs e) {
        Worker worker = sender as Worker;
        if(worker.GetComponent<WorkerAI>().GetJob() != WorkerAI.JobTypes.wild && worker.GetComponent<WorkerAI>().GetJob() != WorkerAI.JobTypes.jobless) {
            currentWorkersAssignedJobs--;
            if(currentWorkersAssignedJobs < 0) {
                currentWorkersAssignedJobs = 0;
            }
        }

        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.hunter) {
            hunterAmount--;
            if (hunterAmount < 0) {
                hunterAmount = 0;
            }
        }

        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.miner) {
            minerAmount--;
            if (minerAmount < 0) {
                minerAmount = 0;
            }
        }

        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.engineer) {
            engineerAmount--;
            if (engineerAmount < 0) {
                engineerAmount = 0;
            }
        }

        if (worker.GetComponent<WorkerAI>().GetJob() == WorkerAI.JobTypes.guard) {
            guardAmount--;
            if (guardAmount < 0) {
                guardAmount = 0;
            }
        }
    }

    private void WorkerAI_OnAnyWorkerAssignedJob(object sender, System.EventArgs e) {
        WorkerAI workerAI = sender as WorkerAI;

        currentWorkersAssignedJobs++;

        if(currentWorkersAssignedJobs >= assignedJobsTreshold) {
            TryUnlockSuccess("EMBERLING_AMOUNT");
        }

        if(workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            hunterAmount++;
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.miner) {
            minerAmount++;
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.guard) {
            guardAmount++;
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.engineer) {
            engineerAmount++;
        }


        if(hunterAmount > 0 && minerAmount > 0 && guardAmount > 0 && engineerAmount > 0) {
            TryUnlockSuccess("EMBERLING_CLASSES");
        }
    }

    private void PlayerShoot_OnSpinningBulletFail(object sender, System.EventArgs e) {
        successfulSurgeReloadsIndex = 0;
    }

    private void Gun_OnAnySurgeReloadSuccess(object sender, System.EventArgs e) {
        successfulSurgeReloadsIndex++;

        if(successfulSurgeReloadsIndex >= successfulSurgeReloadTreshold) {
            TryUnlockSuccess("SURGE_RELOAD");
        }
    }

    private void PlayerShoot_OnPlayerShot(object sender, System.EventArgs e) {
        playerShot = true;
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (specialWave) {
            TryUnlockSuccess("SPECIAL_WAVE");
        }

        if(!playerShot) {
            TryUnlockSuccess("NO_SHOT_FIRED");
        }
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        specialWave = CreaturesSpawnManager.Instance.GetSpecialWaveType() != CreaturesSpawnManager.SpecialWaveType.none;
        playerShot = false;
    }

    private void DogDigAbility_OnAnyResourceDug(object sender, System.EventArgs e) {
        AchievementsManager.Instance.AddToSteamStat("RESOURCES_DUG_BY_DOG", 1);
        if(AchievementsManager.Instance.GetSteamStat("RESOURCES_DUG_BY_DOG") >= resourcesDugByDogTreshold) {
            TryUnlockSuccess("DOG_RESOURCES");
        }
    }

    private void Creature_OnAnyCreatureKilledByDog(object sender, System.EventArgs e) {
        TryUnlockSuccess("FIRST_KILL_DOG");
    }

    private void Creature_OnAnyMobDied(object sender, System.EventArgs e) {
        Creature creature = sender as Creature;

        if(creature.GetCreatureSO() == finalBossCreatureSO) {
            TryUnlockSuccess("FINAL_BOSS");
        }

        AchievementsManager.Instance.AddToSteamStat("KILL_ENEMIES", 1);

        if(AchievementsManager.Instance.GetSteamStat("KILL_ENEMIES") >= killCreaturesTreshold) {
            TryUnlockSuccess("KILL_ENEMIES");
        }

        if(creature.GetInFireLightAmount() > 0) {
            AchievementsManager.Instance.AddToSteamStat("KILL_ENEMIES_IN_FIRE", 1);

            if (AchievementsManager.Instance.GetSteamStat("KILL_ENEMIES_IN_FIRE") >= killCreaturesInFireTreshold) {
                TryUnlockSuccess("KILL_ENEMIES_IN_FIRE");
            }

        }
    }

    private void LevelManager_OnLevelSuccess(object sender, System.EventArgs e) {
        if (LevelManager.Instance.GetLevelSO().merchantsUnlockedInLevel[0] == HubMerchant.HubMerchantType.MushroomMerchant) {
            mushroomMerchantUnlocked = true;
            ES3.Save("mushroomMerchantUnlocked", true);

            if(architectTableUnlocked) {
                TryUnlockSuccess("ALL_NPC_UNLOCKED");
            }
        }

        if (LevelManager.Instance.GetLevelSO().merchantsUnlockedInLevel[0] == HubMerchant.HubMerchantType.ArchitectTable) {
            architectTableUnlocked = true;
            ES3.Save("architectTableUnlocked", architectTableUnlocked);

            if (mushroomMerchantUnlocked) {
                TryUnlockSuccess("ALL_NPC_UNLOCKED");
            }
        }
    }

    private void LevelUI_Locations_OnLocationTextShown(object sender, System.EventArgs e) {
        LevelSO.LevelEnvironment environment = LevelManager.Instance.GetLevelSO().environmentType;

        if (environment == LevelSO.LevelEnvironment.CorruptedCity) {
            TryUnlockSuccess("CORRUPTED_CITY");
        }
        if (environment == LevelSO.LevelEnvironment.TheLumenHollow) {
            TryUnlockSuccess("LUMEN_HOLLOW");
        }
        if (environment == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            TryUnlockSuccess("VICTORIAN_CITY");
        }

    }

    private void Fire_OnPrimordialFireLit(object sender, System.EventArgs e) {
        TryUnlockSuccess("FIRST_PRIMORDIAL_FIRE_LIT");
    }

    private void EndLevelArea_OnEndLevelFireLit(object sender, System.EventArgs e) {
        TryUnlockSuccess("FIRST_NEST_DESTROYED");
    }

    private void TryUnlockSuccess(string id) {
       if(!AchievementsManager.Instance.IsThisAchievementUnlocked(id)) {
            AchievementsManager.Instance.UnlockAchievement(id);
       }
    }

    private void OnDestroy() {
        
    }
}
