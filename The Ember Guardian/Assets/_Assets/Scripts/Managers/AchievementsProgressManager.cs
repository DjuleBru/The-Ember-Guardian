using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementsProgressManager : MonoBehaviour
{
    [SerializeField] private CreatureSO finalBossCreatureSO;
    private bool mushroomMerchantUnlocked_ACHIEVEMENT;
    private bool architectTableUnlocked_ACHIEVEMENT;
    private bool fireFuelDepletionUnlocked_ACHIEVEMENT;
    private bool fireOrbConversionRate_ACHIEVEMENT;
    private bool fireFuelCapacity_ACHIEVEMENT;

    private int boneReaperDeathAmount;
    private int hunterAmount;
    private int minerAmount;
    private int guardAmount;
    private int engineerAmount;

    private bool specialWave;
    private bool nightStarted;
    private bool loadedLevel;
    private bool playerShot;
    private bool playerMoving;
    private bool dropppingMoreThanTreshold;
    private int successfulSurgeReloadsIndex;
    private int currentWorkersAssignedJobs;
    private int creaturesKilledWithoutMoving;

    private bool bulletBouncedOff;
    private float bulletBouncedOffTimer;
    private bool pettingDog;
    private bool dogInCampZoneArea;
    private bool playerExploredWholeDay;
    private float pettingDogTimer;
    private float pettingDogTimerTreshold = 5f;

    private int resourcesDugByDogTreshold = 50;
    private int resourcesFetchedByDogTreshold = 50;
    private int creaturesKilledByDogTreshold = 50;
    private int killCreaturesTreshold = 5000;
    private int killCreaturesInFireTreshold = 500;
    private int killCreaturesWithoutMovingTreshold = 30;
    private int successfulSurgeReloadTreshold = 3;
    private int assignedJobsTreshold = 20;
    private int orbDroppedByWorkerTreshold = 25;
    private int nightsToSurviveTreshold = 100;

    private int secondaryFiresLitTreshold = 4;

    private int huntingFlagsTooFar;
    private bool nightJustStarted;
    private float nightJustStartedTimer;
    private float nightJustStartedTime = 10f;

    List<StructureSO.StructureType> allStructureTypeList = new List<StructureSO.StructureType> {
        StructureSO.StructureType.ammoCrafter,
        StructureSO.StructureType.barricade,
        StructureSO.StructureType.hunterShrine,
        StructureSO.StructureType.minerShrine,
        StructureSO.StructureType.guardShrine,
        StructureSO.StructureType.tower,
        StructureSO.StructureType.orbProcessor,
        StructureSO.StructureType.merchant_skills,
        StructureSO.StructureType.merchant_traps,
        StructureSO.StructureType.observationTower,
        StructureSO.StructureType.sniperTower,
        StructureSO.StructureType.machineGunTower,
        StructureSO.StructureType.mortarTower,
        StructureSO.StructureType.secondaryFire,
        StructureSO.StructureType.fastTravelTeleporter,
        StructureSO.StructureType.orbExtractor,
        StructureSO.StructureType.engineerShrine,
        StructureSO.StructureType.currencyStorage_BigOrb,
        StructureSO.StructureType.currencyStorage_SmallOrb,
        StructureSO.StructureType.currencyStorage_Ammo,
        StructureSO.StructureType.currencyStorage_SpecialAmmo,
    };


    private void Awake() {
        mushroomMerchantUnlocked_ACHIEVEMENT = ES3.Load("mushroomMerchantUnlocked_ACHIEVEMENT", false);
        architectTableUnlocked_ACHIEVEMENT = ES3.Load("architectTableUnlocked_ACHIEVEMENT", false);
        fireOrbConversionRate_ACHIEVEMENT = ES3.Load("fireOrbConversionRate_ACHIEVEMENT", false);
        fireFuelDepletionUnlocked_ACHIEVEMENT = ES3.Load("fireFuelDepletionUnlocked_ACHIEVEMENT", false);
        fireFuelCapacity_ACHIEVEMENT = ES3.Load("fireFuelCapacity_ACHIEVEMENT", false);
        boneReaperDeathAmount = ES3.Load("boneReaperDeathAmount", 0);
    }

    private void Start() {
        if (VersioningManager.Instance.GetIsDemo()) return;
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level || SceneLoader.Instance.GetSceneType() ==  SceneLoader.SceneType.Tutorial) {

            if(EndLevelArea.Instance != null) {
                EndLevelArea.Instance.OnEndLevelFireLit += EndLevelArea_OnEndLevelFireLit;
            }

            LevelUI_Locations.Instance.OnLocationTextShown += LevelUI_Locations_OnLocationTextShown;
            LevelManager.Instance.OnLevelSuccess += LevelManager_OnLevelSuccess;
            Fire.Instance.OnPrimordialFireLit += Fire_OnPrimordialFireLit;
            Creature.OnAnyCreatureDied += Creature_OnAnyCreatureDied;
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
            Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;
            DogAI_Retreiver.OnAnyOrbDroppedByDog += DogAI_Retreiver_OnAnyOrbDroppedByDog;
            StructureLocation.OnAnyStructureBuilt += StructureLocation_OnAnyStructureBuilt;
            Fire.OnAnySecondaryFireReset += Fire_OnAnySecondaryFireReset;
            Fire.Instance.OnFireFuelled += Fire_OnFireFuelled;
            Fire.OnFireExtinguishedByPlayerRespawning += Fire_OnFireExtinguishedByPlayerRespawning;
            HuntingFlag_PlayerDefined.OnHuntingFlagBackToSafetyCarriedByPlayer += HuntingFlag_PlayerDefined_OnHuntingFlagBackToSafetyCarriedByPlayer;
            HuntingFlag_PlayerDefined.OnHuntingFlagTooFarCarriedByPlayer += HuntingFlag_PlayerDefined_OnHuntingFlagTooFarCarriedByPlayer;
            Player.Instance.OnPlayerEnteredCamp += Player_OnPlayerEnteredCamp;
            Player.Instance.OnPlayerExitedCamp += Player_OnPlayerExitedCamp;
            ParticleCollision.OnAnyParticleBouncedOff += ParticleCollision_OnAnyParticleBouncedOff;
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

        if(bulletBouncedOff) {
            bulletBouncedOffTimer += Time.deltaTime;
            if(bulletBouncedOffTimer > PlayerShoot.Instance.GetHeldGunSO().shootCooldownTime) {
                bulletBouncedOff = false;
            }
        }

        if(nightJustStarted) {
            nightJustStartedTimer += Time.deltaTime;
            if(nightJustStartedTimer > nightJustStartedTime) {
                nightJustStarted = false;
            }
        }


        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            if(dogInCampZoneArea) {
                if (!CampZoneManager.Instance.IsWithinCampZoneLimits(Dog.Instance.transform.position)) {
                    dogInCampZoneArea = false;
                }
            }
            if (Mathf.Abs(PlayerMovement.Instance.GetMoveSpeed()) < 1) {
                playerMoving = false;
            }
            else {
                playerMoving = true;
                creaturesKilledWithoutMoving = 0;
            }

        }
    }

    private void ParticleCollision_OnAnyParticleBouncedOff(object sender, System.EventArgs e) {
        ParticleCollision pc = (ParticleCollision)sender;
        if (!pc.GetIsPlayerBullet()) return;

        bulletBouncedOff = true;
        bulletBouncedOffTimer = 0;
    }

    private void Player_OnPlayerExitedCamp(object sender, System.EventArgs e) {
        if (DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dawn) {
            playerExploredWholeDay = true;
        } else {
            playerExploredWholeDay = false;
        }
    }

    private void Player_OnPlayerEnteredCamp(object sender, System.EventArgs e) {

        if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Dusk || DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night) {
            if(dogInCampZoneArea && playerExploredWholeDay) {
                TryUnlockSuccess("DOG_WAIT_CAMP");
            }
        } else {
            playerExploredWholeDay = false;
        }
    }

    private void HuntingFlag_PlayerDefined_OnHuntingFlagTooFarCarriedByPlayer(object sender, System.EventArgs e) {
        huntingFlagsTooFar++;
    }

    private void HuntingFlag_PlayerDefined_OnHuntingFlagBackToSafetyCarriedByPlayer(object sender, System.EventArgs e) {
        huntingFlagsTooFar--;
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
                fireFuelDepletionUnlocked_ACHIEVEMENT = true;
                ES3.Save("fireFuelDepletionUnlocked_ACHIEVEMENT", true);

                if(fireOrbConversionRate_ACHIEVEMENT && fireFuelCapacity_ACHIEVEMENT) {
                    TryUnlockSuccess("ALL_FIRE_UPGRADES");
                }
            }

            if (architectItem.GetArchitectItemType() == HubMerchantItem_ArchitectMerchantItem.ArchitectItemType.FireOrbConversionRate) {
                fireOrbConversionRate_ACHIEVEMENT = true;
                ES3.Save("fireOrbConversionRate_ACHIEVEMENT", true);

                if (fireFuelDepletionUnlocked_ACHIEVEMENT && fireFuelCapacity_ACHIEVEMENT) {
                    TryUnlockSuccess("ALL_FIRE_UPGRADES");
                }
            }

            if (architectItem.GetArchitectItemType() == HubMerchantItem_ArchitectMerchantItem.ArchitectItemType.MaxFuelCapacity) {
                fireFuelCapacity_ACHIEVEMENT = true;
                ES3.Save("fireFuelCapacity_ACHIEVEMENT", true);

                if (fireOrbConversionRate_ACHIEVEMENT && fireFuelDepletionUnlocked_ACHIEVEMENT) {
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

  
    private void Fire_OnFireFuelled(object sender, System.EventArgs e) {
        float remainingFuelBeforeFuelled = Fire.Instance.GetCurrentFuelLevel() - StructureStats.Instance.GetOrbFuelValue();

        if (remainingFuelBeforeFuelled < StructureStats.Instance.GetInitialMaxFuelTreshold() / 100f && remainingFuelBeforeFuelled > 0) {
            TryUnlockSuccess("FUEL_FIRE_ALMOST_EMPTY");
        }
    }


    private void Fire_OnFireExtinguishedByPlayerRespawning(object sender, System.EventArgs e) {
        TryUnlockSuccess("LOOSE_RESPAWNING");
    }
    private void Fire_OnAnySecondaryFireReset(object sender, System.EventArgs e) {
        AchievementsManager.Instance.AddToSteamStat("SECONDARY_FIRES_LIT_v3", 1);
        if (AchievementsManager.Instance.GetSteamStat("SECONDARY_FIRES_LIT_v3") >= secondaryFiresLitTreshold) {
            TryUnlockSuccess("SECONDARY_FIRES");
        }
    }

    private void StructureLocation_OnAnyStructureBuilt(object sender, StructureLocation.OnAnyStructureBuiltEventArgs e) {
        StructureLocation location = sender as StructureLocation;

        if(allStructureTypeList.Contains(location.GetStructureSOToBuild().structureType)) {
            allStructureTypeList.Remove(location.GetStructureSOToBuild().structureType);

            if(allStructureTypeList.Count == 0) {
                TryUnlockSuccess("BUILD_ALL_STRUCTURES");
            }

            if(!allStructureTypeList.Contains(StructureSO.StructureType.sniperTower) && !allStructureTypeList.Contains(StructureSO.StructureType.mortarTower) && !allStructureTypeList.Contains(StructureSO.StructureType.machineGunTower)) {
                TryUnlockSuccess("SPECIAL_TOWERS");
            }
        }
    }

    private void Dog_OnDogTypeChanged(object sender, Dog.OnDogTypeChangedEventArgs e) {
        if (Dog.Instance.GetDogType() == Dog.DogType.GoldenRetreiver) {
            StartCoroutine(TryUnlockSuccessAfterDelay("GOLDEN_RETREIVER", 2f));
        }
        if (Dog.Instance.GetDogType() == Dog.DogType.DarkCompanion) {
            StartCoroutine(TryUnlockSuccessAfterDelay("DARK_COMPANION", 2f));
        }
    }
    private void Worker_OnAnyWorkerDroppedAllCurrencies(object sender, System.EventArgs e) {
        if (dropppingMoreThanTreshold) {
            dropppingMoreThanTreshold = false;
            TryUnlockSuccess("EMBERLING_DROP");
        }
    }

    private void Worker_OnAnyOrbDroppedByWorker(object sender, System.EventArgs e) {
        Worker worker = sender as Worker;
        int orbsToDrop =worker.GetCurrencyAmount(PlayerCurrencies.CurrencyType.bigBlueOrb);

        if (orbsToDrop > orbDroppedByWorkerTreshold - 1) {
            dropppingMoreThanTreshold = true;
        }
    }

    private void Worker_OnAnyWorkerDied(object sender, System.EventArgs e) {
        Worker worker = sender as Worker;

        if(huntingFlagsTooFar > 0) {
            if(DayNightManager.Instance.GetDayNightCycleState() == DayNightManager.State.Night && nightJustStarted) {
                if(!CampZoneManager.Instance.IsWithinCampZoneLimits(worker.transform.position)) {
                    TryUnlockSuccess("EMBERLING_LATE");
                }
            }
        }

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
        if(DayNightManager.Instance.GetCurrentDay() == 0) {
            boneReaperDeathAmount = 0;
        } else {
            AchievementsManager.Instance.AddToSteamStat("NIGHTS_SURVIVED_v3", 1);
            if (AchievementsManager.Instance.GetSteamStat("NIGHTS_SURVIVED_v3") >= nightsToSurviveTreshold) {
                TryUnlockSuccess("TOTAL_NIGHTS_SURVIVED");
            }
        }

        if (CampZoneManager.Instance.IsWithinCampZoneLimits(Dog.Instance.transform.position)) {
            dogInCampZoneArea = true;
        }

        if (specialWave) {
            TryUnlockSuccess("SPECIAL_WAVE");
        }

        if(!playerShot) {
            if (DayNightManager.Instance.GetCurrentDay() == 0) return;
            if (SavingManager_Level.Instance.GetLoadingSavedLevel() && !loadedLevel) {
                loadedLevel = true;
                return;
            } 
            TryUnlockSuccess("NO_SHOT_FIRED");
        }
    }

    private void DayNightManager_OnNightStart(object sender, System.EventArgs e) {
        specialWave = CreaturesSpawnManager.Instance.GetSpecialWaveType() != CreaturesSpawnManager.SpecialWaveType.none;
        playerShot = false;
        nightJustStarted = true;
        nightJustStartedTimer = 0;
    }

    private void DogDigAbility_OnAnyResourceDug(object sender, System.EventArgs e) {
        AchievementsManager.Instance.AddToSteamStat("RESOURCES_DUG_BY_DOG_v3", 1);
        if(AchievementsManager.Instance.GetSteamStat("RESOURCES_DUG_BY_DOG_v3") >= resourcesDugByDogTreshold) {
            TryUnlockSuccess("DOG_RESOURCES");
        }
    }

    private void DogAI_Retreiver_OnAnyOrbDroppedByDog(object sender, System.EventArgs e) {
        AchievementsManager.Instance.AddToSteamStat("RESOURCES_DROPPED_BY_DOG", 1);
        if (AchievementsManager.Instance.GetSteamStat("RESOURCES_DROPPED_BY_DOG") >= resourcesFetchedByDogTreshold) {
            TryUnlockSuccess("DOG_FETCH");
        }
    }

    private void Creature_OnAnyCreatureKilledByDog(object sender, System.EventArgs e) {
        TryUnlockSuccess("FIRST_KILL_DOG");

        AchievementsManager.Instance.AddToSteamStat("CREATURES_KILLED_BY_DOG", 1);
        if (AchievementsManager.Instance.GetSteamStat("CREATURES_KILLED_BY_DOG") >= creaturesKilledByDogTreshold) {
            TryUnlockSuccess("DOG_KILL_CREATURES");
        }
    }

    private void Creature_OnAnyCreatureDied(object sender, Creature.OnCreatureDiedEventArgs e) {
        if (e.damageSource != Player.Instance.transform) return;

        Creature creature = sender as Creature;

        if (!playerMoving) {
            creaturesKilledWithoutMoving++;
            if (creaturesKilledWithoutMoving >= killCreaturesWithoutMovingTreshold) {
                TryUnlockSuccess("KILL_CREATURES_WITHOUT_MOVING");
            }
        }

        if (bulletBouncedOff) {
            TryUnlockSuccess("RICOCHET");
        }

        if (creature.GetCreatureSO() == finalBossCreatureSO) {
            boneReaperDeathAmount++;
            ES3.Save("boneReaperDeathAmount", boneReaperDeathAmount);
            if (boneReaperDeathAmount == 2) {
                StartCoroutine(TryUnlockSuccessAfterDelay("FINAL_BOSS", 4f));
            }
        }

        AchievementsManager.Instance.AddToSteamStat("KILLED_CREATURES", 1);

        if (AchievementsManager.Instance.GetSteamStat("KILLED_CREATURES") >= killCreaturesTreshold) {
            TryUnlockSuccess("KILL_ENEMIES");
        }

        if (creature.GetInFireLightAmount() > 0) {
            AchievementsManager.Instance.AddToSteamStat("KILLED_CREATURES_INSIDE_FIRE_v3", 1);

            if (AchievementsManager.Instance.GetSteamStat("KILLED_CREATURES_INSIDE_FIRE_v3") >= killCreaturesInFireTreshold) {
                TryUnlockSuccess("KILL_ENEMIES_IN_FIRE");
            }

        }

    }

    private void LevelManager_OnLevelSuccess(object sender, System.EventArgs e) {
        if (LevelManager.Instance.GetLevelSO().merchantsUnlockedInLevel[0] == HubMerchant.HubMerchantType.MushroomMerchant) {
            mushroomMerchantUnlocked_ACHIEVEMENT = true;
            ES3.Save("mushroomMerchantUnlocked_ACHIEVEMENT", true);

            if(architectTableUnlocked_ACHIEVEMENT) {
                TryUnlockSuccess("ALL_NPC_UNLOCKED");
            }
        }

        if (LevelManager.Instance.GetLevelSO().merchantsUnlockedInLevel[0] == HubMerchant.HubMerchantType.ArchitectTable) {
            architectTableUnlocked_ACHIEVEMENT = true;
            ES3.Save("architectTableUnlocked_ACHIEVEMENT", architectTableUnlocked_ACHIEVEMENT);

            if (mushroomMerchantUnlocked_ACHIEVEMENT) {
                TryUnlockSuccess("ALL_NPC_UNLOCKED");
            }
        }
    }

    private void LevelUI_Locations_OnLocationTextShown(object sender, System.EventArgs e) {
        LevelSO.LevelEnvironment environment = LevelManager.Instance.GetLevelSO().environmentType;

        if (environment == LevelSO.LevelEnvironment.TheVerdantGraveyard) {
            StartCoroutine(TryUnlockSuccessAfterDelay("VERDANT_GRAVEYARD", 3f));
        }

        if (environment == LevelSO.LevelEnvironment.CorruptedCity) {
            StartCoroutine(TryUnlockSuccessAfterDelay("CORRUPTED_CITY",3f));
        }
        if (environment == LevelSO.LevelEnvironment.TheLumenHollow) {
            StartCoroutine(TryUnlockSuccessAfterDelay("LUMEN_HOLLOW", 3f));
        }
        if (environment == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            StartCoroutine(TryUnlockSuccessAfterDelay("FRACTURED_DISCTRICT", 3f));
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
    private IEnumerator TryUnlockSuccessAfterDelay(string id, float delay) {
        yield return new WaitForSeconds(delay);
        if (!AchievementsManager.Instance.IsThisAchievementUnlocked(id)) {
            AchievementsManager.Instance.UnlockAchievement(id);
        }
    }
    private void OnDestroy() {
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level || SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {

            if (EndLevelArea.Instance != null) {
                EndLevelArea.Instance.OnEndLevelFireLit -= EndLevelArea_OnEndLevelFireLit;
            }

            LevelUI_Locations.Instance.OnLocationTextShown -= LevelUI_Locations_OnLocationTextShown;
            LevelManager.Instance.OnLevelSuccess -= LevelManager_OnLevelSuccess;
            Fire.Instance.OnPrimordialFireLit -= Fire_OnPrimordialFireLit;
            Creature.OnAnyCreatureDied -= Creature_OnAnyCreatureDied;
            Creature.OnAnyCreatureKilledByDog -= Creature_OnAnyCreatureKilledByDog;
            DogDigAbility.OnAnyResourceDug -= DogDigAbility_OnAnyResourceDug;
            DayNightManager.Instance.OnNightStart -= DayNightManager_OnNightStart;
            DayNightManager.Instance.OnDawnStart -= DayNightManager_OnDawnStart;
            PlayerShoot.Instance.OnPlayerShot -= PlayerShoot_OnPlayerShot;
            Gun.OnAnySurgeReloadSuccess -= Gun_OnAnySurgeReloadSuccess;
            PlayerShoot.Instance.OnSpinningBulletFail -= PlayerShoot_OnSpinningBulletFail;
            WorkerAI.OnAnyWorkerAssignedJob -= WorkerAI_OnAnyWorkerAssignedJob;
            Worker.OnAnyWorkerDied -= Worker_OnAnyWorkerDied;
            Worker.OnAnyOrbDroppedByWorker -= Worker_OnAnyOrbDroppedByWorker;
            Worker.OnAnyWorkerDroppedAllCurrencies -= Worker_OnAnyWorkerDroppedAllCurrencies;
            Dog.Instance.OnDogTypeChanged -= Dog_OnDogTypeChanged;
            DogAI_Retreiver.OnAnyOrbDroppedByDog -= DogAI_Retreiver_OnAnyOrbDroppedByDog;
            StructureLocation.OnAnyStructureBuilt -= StructureLocation_OnAnyStructureBuilt;
            Fire.OnAnySecondaryFireReset -= Fire_OnAnySecondaryFireReset;
            Fire.Instance.OnFireFuelled -= Fire_OnFireFuelled;
            Fire.OnFireExtinguishedByPlayerRespawning -= Fire_OnFireExtinguishedByPlayerRespawning;
            HuntingFlag_PlayerDefined.OnHuntingFlagBackToSafetyCarriedByPlayer -= HuntingFlag_PlayerDefined_OnHuntingFlagBackToSafetyCarriedByPlayer;
            HuntingFlag_PlayerDefined.OnHuntingFlagTooFarCarriedByPlayer -= HuntingFlag_PlayerDefined_OnHuntingFlagTooFarCarriedByPlayer;
            Player.Instance.OnPlayerEnteredCamp -= Player_OnPlayerEnteredCamp;
            Player.Instance.OnPlayerExitedCamp -= Player_OnPlayerExitedCamp;
            ParticleCollision.OnAnyParticleBouncedOff -= ParticleCollision_OnAnyParticleBouncedOff;
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            HubMerchantItem.OnAnyHubMerchantItemBought -= HubMerchantItem_OnAnyHubMerchantItemBought;
            CampEditManager.Instance.OnAnyChangeMade -= CampEditManager_OnAnyChangeMade;
        }

        if (PetDog.Instance != null) {
            PetDog.Instance.OnPlayerStartedPettingDog -= PetDog_OnPlayerStartedPettingDog;
            PetDog.Instance.OnPlayerStoppedPettingDog -= PetDog_OnPlayerStoppedPettingDog;
        }
    }
}
