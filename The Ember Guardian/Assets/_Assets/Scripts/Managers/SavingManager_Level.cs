using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SavingManager_Level : MonoBehaviour
{
    public static SavingManager_Level Instance;
    private bool loadingSavedLevel;
    private bool firstDawnAfterGameStart = true;
    private bool isLoading;
    private bool gameSavedOnce;
    private DateTime lastSaveTime;

    public event EventHandler OnLoadGameEnded;
    public event EventHandler OnSaveGameStarted;
    public event EventHandler OnSaveGameEnded;

    private void Awake() {
        Instance = this;
        if (VersioningManager.Instance.GetIsDemo()) return;
        if (LevelManager.Instance.IsHordeMode()) return;

        if (ES3.FileExists("LevelSave_temp.es3")) {
            ES3.DeleteFile("LevelSave_temp.es3");
        }

        if (ES3.FileExists("LevelSave.es3")) {

            if (!ES3.KeyExists("LevelState", "LevelSave.es3")) return;
            LevelSaveData saveData = ES3.Load<LevelSaveData>("LevelState", "LevelSave.es3");

            if(saveData.sceneName != SceneManager.GetActiveScene().name) {
                ES3.DeleteFile("LevelSave.es3");
                return;
            }

            loadingSavedLevel = true;
            Debug.Log($"[Save] Save file found for scene" + "LevelSave" + " loading...");
        }
        else {
            Debug.Log($"[Save] No save file for scene {"LevelSave"}, starting fresh.");
        }
    }

    private void Start() {
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) return;
        if (VersioningManager.Instance.GetIsDemo()) return;

        if (loadingSavedLevel) {
            LoadGame();
        }

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
    }

    public void DeleteLevelSave() {
        if (ES3.FileExists("LevelSave.es3")) {
            ES3.DeleteFile("LevelSave.es3");
        }
    }


    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (firstDawnAfterGameStart) {
            firstDawnAfterGameStart = false;
            return;
        };

        if (VersioningManager.Instance.GetIsDemo()) return;
        SaveGame();
        CreateLevelSaveCopy();
    }

    #region SAVE

    private void SaveGame() {
        OnSaveGameStarted?.Invoke(this, EventArgs.Empty);
        StartCoroutine(SaveAfterDelay(1f));
    }

    private IEnumerator SaveAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        string mainPath = "LevelSave.es3";
        string tempPath = "LevelSave_temp.es3";
        string backupPath = "LevelSave_backup.es3";

        if (ES3.FileExists(tempPath))
            ES3.DeleteFile(tempPath);

        SaveLevelState(tempPath);
        yield return new WaitForEndOfFrame();

        SaveCollectibles(tempPath);
        yield return new WaitForEndOfFrame();

        SaveRecruitedWorkers(tempPath);
        yield return new WaitForEndOfFrame();

        SaveSpawners(tempPath);
        yield return new WaitForEndOfFrame();

        SavePlayer(tempPath);
        yield return new WaitForEndOfFrame();

        SaveStructures(tempPath);
        yield return new WaitForEndOfFrame();

        SaveTrapUpgrades(tempPath);
        yield return new WaitForEndOfFrame();

        SaveTrapTypes(tempPath);
        yield return new WaitForEndOfFrame();

        SaveObstacles(tempPath);
        SaveChests(tempPath);

        yield return new WaitForEndOfFrame();
        SaveScavengables(tempPath);

        yield return new WaitForEndOfFrame();
        SaveTrialAreas(tempPath);

        if (ES3.FileExists(mainPath)) {
            ES3.CopyFile(mainPath, backupPath); // sauvegarde de secours
            ES3.DeleteFile(mainPath);
        }

        ES3.RenameFile(tempPath, mainPath);

        yield return new WaitForSeconds(3f);

        OnSaveGameEnded?.Invoke(this, EventArgs.Empty);
        gameSavedOnce = true;
        lastSaveTime = DateTime.Now;
    }

    private void SaveLevelState(string path) {
        LevelSaveData levelSaveData = new LevelSaveData();

        levelSaveData.sceneName = SceneManager.GetActiveScene().name;
        levelSaveData.currentDay = DayNightManager.Instance.GetCurrentDay();
        levelSaveData.currentObjectiveType = LevelUI_ObjectiveUI.Instance.GetCurrentObjectiveType();
        levelSaveData.currentSubObjectiveTypeList = LevelUI_ObjectiveUI.Instance.GetCurrentSubObjectivesList();

        levelSaveData.emberExtracted = LevelObjectives.Instance.GetEmberExtracted();
        levelSaveData.initialFireLit = LevelObjectives.Instance.GetInitialFireLit();
        levelSaveData.darklingNestFound = LevelObjectives.Instance.GetDarklingNestFound();
        levelSaveData.darklingNestCleared = LevelObjectives.Instance.GetDarklingNestCleared();
        levelSaveData.darklingNest_RightFound = LevelObjectives.Instance.GetRightDarklingNestFound();
        levelSaveData.darklingNest_RightDestroyed = LevelObjectives.Instance.GetRightDarklingNestDestroyed();
        levelSaveData.darklingNest_LeftFound = LevelObjectives.Instance.GetLeftDarklingNestFound();
        levelSaveData.darklingNest_LeftDestroyed = LevelObjectives.Instance.GetLeftDarklingNestDestroyed();
        levelSaveData.returnToHubObjectiveShown = LevelObjectives.Instance.GetReturnToHubObjectiveShown();
        levelSaveData.conditionalLockedStructureLocationBuilt = LevelManager.Instance.GetConditionalLockedStructureLocationBuilt();
        levelSaveData.conditionalLockedStructureLocationUnlocked = LevelManager.Instance.GetConditionalLockedStructureLocationUnlocked();
        levelSaveData.hubMerchantHasTalkLinesToShow = LevelManager.Instance.GetLevelHubMerchantHasTalkLinesToShow();
        levelSaveData.hubMerchantsHaveTalkLinesToShow_LevelObjectives = LevelObjectives.Instance.GetLevelMerchantsHaveTalkLinesToShow();
        levelSaveData.levelSucceeded = LevelManager.Instance.GetLevelSucceeded();

        levelSaveData.currentSpecialWaveAmount = CreaturesSpawnManager.Instance.GetCurrentSpecialWaveAmount();
        levelSaveData.NPCInteractionsIndex = LevelObjectives.Instance.GetNPCInteractionsIndex();
        levelSaveData.levelManager_levelHubMerchantInteractionIndex = LevelManager.Instance.GetLevelHubMerchantInteractionIndex();
        levelSaveData.nightsSurvived = LevelObjectives.Instance.GetNightsSurvived();
        levelSaveData.obstaclesRemoved = LevelObjectives.Instance.GetObstaclesRemoved();
        levelSaveData.watcherArtifactFillUpAmount = LevelObjectives.Instance.GetWatcherArtifactFillAmount();

        if(levelSaveData.currentObjectiveType == LevelUI_ObjectiveUI.ObjectiveType.SurviveNights && levelSaveData.nightsSurvived == LevelManager.Instance.GetLevelSO().nightsToSurviveAmount) {
            levelSaveData.currentObjectiveType = LevelUI_ObjectiveUI.ObjectiveType.ReturnToHub;
        }

        ES3.Save("LevelState", levelSaveData, path);
    }

    private void SaveCollectibles(string path) {
        List<CollectibleSaveData> collectibleData = new List<CollectibleSaveData>();

        foreach (Collectible collectible in LevelManager.Instance.GetAllCollectibles()) {
            CollectibleSaveData data = new CollectibleSaveData();

            // Position
            data.posX = collectible.transform.position.x;
            data.posY = collectible.transform.position.y;
            data.rotation = collectible.transform.rotation;

            data.currencyType = collectible.GetCurrencyType();
            collectibleData.Add(data);
        }

        // Sauvegarde via EasySave
        ES3.Save("Collectibles", collectibleData, path);
    }

    private void SaveRecruitedWorkers(string path) {
        List<WorkerSaveData> workersData = new List<WorkerSaveData>();

        foreach (Worker worker in WorkerManager.Instance.GetRecruitedWorkers()) {
            WorkerSaveData data = new WorkerSaveData();

            // Position
            data.posX = worker.transform.position.x;
            data.posY = worker.transform.position.y;

            // Job
            data.jobType = worker.GetComponent<WorkerAI>().GetJob().ToString();

            // Camp side
            data.campSide = worker.GetCampSideAddigned().ToString();

            // Vie
            data.health = worker.GetHealth(); // hérité de Mob

            // Currencies
            data.currencies = new Dictionary<string, int>();
            foreach (PlayerCurrencies.CurrencyType type in System.Enum.GetValues(typeof(PlayerCurrencies.CurrencyType))) {
                int amount = worker.GetCurrencyAmount(type);
                if (amount > 0) {
                    data.currencies[type.ToString()] = amount;
                }
            }

            workersData.Add(data);

            // Sauvegarde via EasySave
            ES3.Save("Workers", workersData, path);
        }
    }

    private void SaveSpawners(string path) {
        List<SpawnerSaveData> spawnersData = new List<SpawnerSaveData>();

        foreach (MobSpawner spawner in SpawnersManager.Instance.GetAllSpawners()) {
            SpawnerSaveData data = new SpawnerSaveData();

            data.spawnerID = spawner.GetSpawnerID();
            data.currentMobsAlive = spawner.GetMobCount(); // tu ajoutes une méthode pour retourner mobSpawnedList.Count
            data.mobsCanSpawnAtDawn = spawner.GetMobsCanSpawnAtDawn();
            data.ambushSpawned = spawner.GetAmbushSpawned();

            if (spawner is CreatureSpawnerContinuous continuousSpawner) {
                data.dead = continuousSpawner.GetDead();
            }

            spawnersData.Add(data);
        }

        ES3.Save("Spawners", spawnersData, path);
    }

    private void SavePlayer(string path) {
        PlayerSaveData data = new PlayerSaveData();

        // Position
        Vector2 pos = Player.Instance.transform.position;
        data.posX = pos.x;
        data.posY = pos.y;

        // HP
        data.health = Player.Instance.GetHP();

        // Skills
        SkillItem skillLeft = PlayerSkills.Instance.GetActiveSkillLeft();
        if (skillLeft != null) {
            data.activeSkills.Add(new SkillData {
                skillType = skillLeft.skillType,
                level = skillLeft.currentLevel
            });
        }

        SkillItem skillRight = PlayerSkills.Instance.GetActiveSkillRight();
        if (skillRight != null) {
            data.activeSkills.Add(new SkillData {
                skillType = skillRight.skillType,
                level = skillRight.currentLevel
            });
        }

        foreach (var skill in PlayerSkills.Instance.GetPassiveSkillList()) {
            data.passiveSkills.Add(new SkillData {
                skillType = skill.skillType,
                level = skill.currentLevel
            });
        }

        // Inventory
        var currencyData = new Dictionary<string, List<Vector3>>();
        var currencyRotationData = new Dictionary<string, List<Quaternion>>();

        foreach (PlayerCurrencies.CurrencyType currencyType in Enum.GetValues(typeof(PlayerCurrencies.CurrencyType))) {
            // Récupérer toutes les positions du type de gemme
            List<Vector3> positions = UICurrencyManager.PlayerInventoryUI.GetCurrencyPositions(currencyType);
            List<Quaternion> rotations = UICurrencyManager.PlayerInventoryUI.GetCurrencyRotations(currencyType);

            // Ajouter au dictionnaire
            currencyData[currencyType.ToString()] = positions;
            currencyRotationData[currencyType.ToString()] = rotations;
        }

        data.currencyData = currencyData;
        data.currencyRotationData = currencyRotationData;

        // Primary gun
        GunSO primary = PlayerShoot.Instance.GetPrimaryGunSO();
        if (primary != null) {
            data.primaryGun = new GunData {
                gunType = primary.gunType,
                ammoClip = PlayerShoot.Instance.GetGun(primary).GetCurrentAmmoClip(),
                currentBullet = PlayerShoot.Instance.GetGun(primary).GetCurrentBullet()
            };
        }

        // Secondary gun
        if (PlayerShoot.Instance.GetSecondaryGunSO() != null) {
            data.hasSecondary = true;
            GunSO secondary = PlayerShoot.Instance.GetSecondaryGunSO();
            data.secondaryGun = new GunData {
                gunType = secondary.gunType,
                ammoClip = PlayerShoot.Instance.GetGun(secondary).GetCurrentAmmoClip(),
                currentBullet = PlayerShoot.Instance.GetGun(secondary).GetCurrentBullet()
            };
        }

        if(PlayerShoot.Instance.GetGunSOInStock() != null) {
            data.hasFoundGun = true;
            List<GunSO> foundGundList = PlayerShoot.Instance.GetGunSOInStock();

            foreach(GunSO gunSO in foundGundList) {
                data.foundGuns.Add(new GunData {
                    gunType = gunSO.gunType,
                    ammoClip = PlayerShoot.Instance.GetGun(gunSO).GetCurrentAmmoClip(),
                    currentBullet = PlayerShoot.Instance.GetGun(gunSO).GetCurrentBullet()
                });
            }

        }

        ES3.Save("Player", data, path);
    }

    private void SaveStructures(string path) {
        List<StructureSaveData> structuresData = new List<StructureSaveData>();
        List<TrapSaveData> trapsData = new List<TrapSaveData>();
        List<FireSaveData> fireData = new List<FireSaveData>();
        List<CurrencyCrafterSaveData> crafterData = new List<CurrencyCrafterSaveData>();
        List<SpecialTowerSaveData> specialTowerData = new List<SpecialTowerSaveData>();
        List<BarricadeSaveData> barricadeData = new List<BarricadeSaveData>();
        List<CurrencyStorageSaveData> currencyStorageData = new List<CurrencyStorageSaveData>();

        foreach (StructureLocation location in StructuresManager.Instance.GetStructureLocationsList()) {
            if (location == null) continue;
            StructureSaveData data = new StructureSaveData();

            data.structureType = location.GetStructureSOToBuild().structureType;
            data.posX = location.transform.position.x;
            data.posY = location.transform.position.y;
            data.structureBuilt = false;
            data.structureUnlocked = location.GetStructureLocationUnlocked();

            structuresData.Add(data);
        }

        foreach (Structure structure in StructuresManager.Instance.GetBuiltStructureList()) {
            if (structure == null) continue;

            StructureSaveData data = new StructureSaveData();
            StructureSO.StructureType structureType = structure.GetStructureSO().structureType;
            StructureSO.StructureCategory structureCategory= structure.GetStructureSO().structureCategory;

            data.structureType = structure.GetStructureSO().structureType;
            data.posX = structure.transform.position.x;
            data.posY = structure.transform.position.y;
            data.structureBuilt = true;
            data.structureLevel = structure.GetStructureLevel();
            data.isWorldStructure = structure.GetIsWorldStructure();
            data.worldStructureScaleX = structure.GetWorldScaleX();

            structuresData.Add(data);

            if (structureCategory == StructureSO.StructureCategory.storage) {
                CurrencyStorage storage = structure as CurrencyStorage;

                CurrencyStorageSaveData data_storage = new CurrencyStorageSaveData();
                data_storage.currencyAmountStored = storage.GetCurrencyAmountStored();

                if(storage is CurrencyStorage_Objective) {
                    CurrencyStorage_Objective storage_obj = storage as CurrencyStorage_Objective;
                    data_storage.maxCurrencyStorageIndex = storage_obj.GetMaxCurrencyStorageIndex();
                }

                currencyStorageData.Add(data_storage);
            }

            if (structureCategory == StructureSO.StructureCategory.trap) {
                Structure_Trap trapStructure = structure as Structure_Trap;

                TrapSaveData data_trap = new TrapSaveData();
                data_trap.currentRearmIndex = trapStructure.GetCurrentRearmIndex();
                data_trap.currentUseIndex = trapStructure.GetCurrentUseIndex();

                trapsData.Add(data_trap);
            }

            if (structureType == StructureSO.StructureType.barricade) {
                Barricade barricade = structure as Barricade;

                BarricadeSaveData data_barricade = new BarricadeSaveData();
                data_barricade.barricadeHealth = barricade.GetHealth();

                barricadeData.Add(data_barricade);
            }


            if (structureType == StructureSO.StructureType.machineGunTower || structureType == StructureSO.StructureType.sniperTower || structureType == StructureSO.StructureType.mortarTower) {
                SpecialTower specialTower = structure as SpecialTower;

                SpecialTowerSaveData data_specialTower = new SpecialTowerSaveData();
                data_specialTower.currentAmmoClips = specialTower.GetCurrentAmmoClip();

                specialTowerData.Add(data_specialTower);
            }

            if (structureType == StructureSO.StructureType.fire || structureType == StructureSO.StructureType.secondaryFire) {
                Fire fireStructure = structure as Fire;

                FireSaveData data_fire = new FireSaveData();
                data_fire.currentFuelLevel = fireStructure.GetCurrentFuelLevel();
                data_fire.isMainFire = fireStructure.GetIsMainFire();
                data_fire.isSecondaryFire = fireStructure.GetIsSecondaryFire();
                data_fire.isEndLevelFire = fireStructure.GetIsEndLevelAreaFire();

                fireData.Add(data_fire);
            }

            if (structureType == StructureSO.StructureType.ammoCrafter || structureType == StructureSO.StructureType.orbProcessor) {
                CurrencyCrafter currencyCrafterStructure = structure as CurrencyCrafter;

                CurrencyCrafterSaveData data_currencyCrafter = new CurrencyCrafterSaveData();
                data_currencyCrafter.currentBatches = currencyCrafterStructure.GetCurrentBatch();
                data_currencyCrafter.currencyCraftTimer = currencyCrafterStructure.GetCurrencyCraftTimer();  
                data_currencyCrafter.craftingCurrency = currencyCrafterStructure.GetCraftingCurrency();  
                data_currencyCrafter.craftedCurrency = currencyCrafterStructure.GetCraftedCurrency();  
                data_currencyCrafter.currencyTypeCrafted = currencyCrafterStructure.GetCurrencyTypeBeingCrafted();  

                crafterData.Add(data_currencyCrafter);
            }
        }


        ES3.Save("Structures", structuresData, path);
        ES3.Save("Structures_Traps", trapsData, path);
        ES3.Save("Structures_Fire", fireData, path);
        ES3.Save("Structures_CurrencyCrafters", crafterData, path);
        ES3.Save("Structures_SpecialTowers", specialTowerData, path);
        ES3.Save("Structures_Barricades", barricadeData, path);
        ES3.Save("Structures_CurrencyStorages", currencyStorageData, path);
    }

    private void SaveTrapUpgrades(string path) {

        ES3.Save("TrapUpgrades", TrapManager.Instance.GetTrapUpgradesLevels(), path);
        
    }
    private void SaveTrapTypes(string path) {
        ES3.Save("TrapTypes", TrapManager.Instance.GetTrapTypesBoughtByPlayer(), path);
    }

    private void SaveObstacles(string path) {
        var obstacleDataList = new List<ObstacleSaveData>();
        var scavengableObstacleDataList = new List<ScavengableObstacleSaveData>();

        foreach (Obstacle obstacle in LevelManager.Instance.GetAllObstacles()) {

            if(obstacle is ScavengableObstacle) {
                ScavengableObstacle scave = (ScavengableObstacle)obstacle;
                scavengableObstacleDataList.Add(new ScavengableObstacleSaveData {
                    scavObstacleID = scave.GetObstacleID(),
                    markedToScavenge = scave.GetMarkedToScavenge(),
                    health = scave.GetHealth(),
                    hitsTaken = scave.GetHitsTaken(),
                    thresholdsTriggered = scave.GetSpawnsTriggered(),
                });

            } else {
                obstacleDataList.Add(new ObstacleSaveData { obstacleID = obstacle.GetObstacleID(), built = obstacle.GetBuilt() });
            }

        }

        ES3.Save("Obstacles", obstacleDataList, path);
        ES3.Save("ScavengableObstacles", scavengableObstacleDataList, path);
    }

    private void SaveChests(string path)    {
        var list = new List<ChestSaveData>();
        foreach (Chest chest in LevelManager.Instance.GetAllChests()) {
            list.Add(new ChestSaveData { 
                chestID = chest.GetChestID(),
                opened = chest.GetChestOpened(),
                chestLocked = chest.GetChestLocked(),
                chestPricePaid = chest.GetChestPricePaid(),
                rewardOfferedToPlayer = chest.GetRewardOfferedToPlayer(),
            });
        }

        ES3.Save("Chests", list, path);
    }

    private void SaveTrialAreas(string path) {
        var list = new List<TrialAreaSaveData>();
        foreach (TrialArea trialArea in LevelManager.Instance.GetAllTrialAreas()) {
            list.Add(new TrialAreaSaveData {
                trialAreaID = trialArea.GetTrialAreaID(),
                trialCompleted = trialArea.GetTrialAreaCompleted(),
            });
        }

        ES3.Save("TrialAreas", list, path);
    }
    private void SaveScavengables(string path) {
        if (ScavengableManager.Instance == null) return;

        var list = new List<ScavengableSaveData>();
        foreach (Scavengable scav in ScavengableManager.Instance.GetScavengablesList()) {
            list.Add(new ScavengableSaveData { 
                scavengableID = scav.GetScavengableID(), 
                health = scav.GetHealth(),
                hitsTaken = scav.GetHitsTaken(),
                timeToMineOneResource = scav.GetTimeToMineOneResource(),
                scavengingActive = scav.GetScavengingActive(),
                markedToScavenge = scav.GetMarkedToScavenge()
            });
        }

        ES3.Save("Scavengables", list, path);
    }

    #endregion

    #region LOAD

    [Button]
    public void LoadGame() {
        AudioListener.pause = true;
        isLoading = true;
        StartCoroutine(LoadGameCoroutine());
    }

    private IEnumerator LoadGameCoroutine() {
        yield return StartCoroutine(LoadPlayer());
        yield return StartCoroutine(LoadLevelState());
        yield return StartCoroutine(LoadCollectibles());
        yield return StartCoroutine(LoadWorkers());
        yield return StartCoroutine(LoadSpawners());
        yield return StartCoroutine(LoadStructures());
        yield return StartCoroutine(LoadTraps());
        yield return StartCoroutine(LoadObstacles());
        yield return StartCoroutine(LoadScavengableObstacles());
        yield return StartCoroutine(LoadScavengables());
        yield return StartCoroutine(LoadChests());
        yield return StartCoroutine(LoadTrialAreas());
        yield return StartCoroutine(LoadObjectives());

        // Quand tout est fini
        isLoading = false;
        CameraManager.Instance.SetCameraPositionToPlayer();
        AudioListener.pause = false;
        OnLoadGameEnded?.Invoke(this, EventArgs.Empty);
    }
    private IEnumerator LoadLevelState() {
        if (!ES3.KeyExists("LevelState", "LevelSave.es3")) yield break;

        yield return new WaitForEndOfFrame();

        LevelSaveData saveData = ES3.Load<LevelSaveData>("LevelState", "LevelSave.es3");

        DayNightManager.Instance.LoadCurrentDay(saveData.currentDay);

        CreaturesSpawnManager.Instance.SetCurrentSpecialWaveAmount(saveData.currentSpecialWaveAmount);
        LevelObjectives.Instance.SetEmberExtracted(saveData.emberExtracted);
        LevelObjectives.Instance.SetInitialFireLit(saveData.initialFireLit);
        LevelObjectives.Instance.SetDarklingNestFound(saveData.darklingNestFound);
        LevelObjectives.Instance.SetDarklingNestCleared(saveData.darklingNestCleared);
        LevelObjectives.Instance.SetRightDarklingNestFound(saveData.darklingNest_RightFound);
        LevelObjectives.Instance.SetRightDarklingNestDestroyed(saveData.darklingNest_RightDestroyed);
        LevelObjectives.Instance.SetLeftDarklingNestFound(saveData.darklingNest_LeftFound);
        LevelObjectives.Instance.SetLeftDarklingNestDestroyed(saveData.darklingNest_LeftDestroyed);

        LevelObjectives.Instance.SetReturnToHubObjectiveShown(saveData.returnToHubObjectiveShown);

        LevelManager.Instance.SetLevelHubMerchantHasTalkLinesToShow(saveData.hubMerchantHasTalkLinesToShow);
        LevelManager.Instance.SetConditionalLockedStructureLocationState(saveData.conditionalLockedStructureLocationUnlocked, saveData.conditionalLockedStructureLocationBuilt);
        LevelObjectives.Instance.SetLevelMerchantsHaveTalkLinesToShow(saveData.hubMerchantsHaveTalkLinesToShow_LevelObjectives);

        if(saveData.levelSucceeded) {
            LevelManager.Instance.LevelSuccess();
        }

        LevelObjectives.Instance.SetNPCInteractionsIndex(saveData.NPCInteractionsIndex);
        LevelManager.Instance.SetLevelHubMerchantInteractionIndex(saveData.levelManager_levelHubMerchantInteractionIndex);

        LevelObjectives.Instance.SetNightsSurvived(saveData.nightsSurvived);

        LevelObjectives.Instance.SetObstaclesRemoved(saveData.obstaclesRemoved);
        LevelObjectives.Instance.SetWatcherArtifactFillAmount(saveData.watcherArtifactFillUpAmount);

    }

    private IEnumerator LoadCollectibles() {
        if (!ES3.KeyExists("Collectibles", "LevelSave.es3")) yield break;

        List<CollectibleSaveData> collectibleData = ES3.Load<List<CollectibleSaveData>>("Collectibles", "LevelSave.es3 ");

        foreach (var data in collectibleData) {
            // Respawn du collectible
            Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(data.currencyType), new Vector3(data.posX, data.posY, 0), Quaternion.identity).GetComponent<Collectible>();
            collectible.SetCollectibleUnInteractable(.1f);
            collectible.SetCollectibleJustLoaded();
            collectible.transform.rotation = data.rotation;

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator LoadWorkers() {
        if (!ES3.KeyExists("Workers", "LevelSave.es3")) yield break;

        List<WorkerSaveData> workersData = ES3.Load<List<WorkerSaveData>>("Workers", "LevelSave.es3 ");

        foreach (var data in workersData) {
            // Respawn du worker
            Worker newWorker = Instantiate(WorkerManager.Instance.workerPrefab, new Vector3(data.posX, data.posY, 0), Quaternion.identity).GetComponent<Worker>();

            yield return null; // sécurité supplémentaire : attendre que ce Worker fasse son Start()

            // Job
            newWorker.RecruitWorker(false, false);
            WorkerAI ai = newWorker.GetComponent<WorkerAI>();
            ai.SetJob((WorkerAI.JobTypes)System.Enum.Parse(typeof(WorkerAI.JobTypes), data.jobType), false);

            // Camp side
            CampZoneManager.CampSide side = (CampZoneManager.CampSide)System.Enum.Parse(typeof(CampZoneManager.CampSide), data.campSide);
            newWorker.AssignSide(side);

            // Vie
            newWorker.SetHealth(data.health);

            // Currencies
            foreach (var kvp in data.currencies) {
                PlayerCurrencies.CurrencyType type = (PlayerCurrencies.CurrencyType)System.Enum.Parse(typeof(PlayerCurrencies.CurrencyType), kvp.Key);
                for (int i = 0; i < kvp.Value; i++) {
                    newWorker.CollectCurrency(type);
                }
            }
        }
    }
    private IEnumerator LoadSpawners() {
        if (!ES3.KeyExists("Spawners", "LevelSave.es3")) yield break;

        List<SpawnerSaveData> spawnersData = ES3.Load<List<SpawnerSaveData>>("Spawners", "LevelSave.es3");

        foreach (SpawnerSaveData data in spawnersData) {
            foreach (var s in SpawnersManager.Instance.GetAllSpawners()) {
                if (s == null) {
                    Debug.LogError("[Save] Null spawner dans la liste !");
                }
            }

            MobSpawner spawner = SpawnersManager.Instance
                .GetAllSpawners()
                .Find(s => s != null && s.GetSpawnerID() == data.spawnerID);

            if (spawner == null) {
                Debug.LogWarning($"[Save] Spawner {data.spawnerID} introuvable dans la scène !");
                continue;
            }


            yield return new WaitForEndOfFrame();

            // appliquer les données sauvegardées
            //Debug.Log("loading spawner " + spawner + " spawnerID " + data.spawnerID);
            spawner.SetMobsCanSpawnAtDawn(data.mobsCanSpawnAtDawn);
            spawner.SetAmbushSpawned(data.ambushSpawned);

            if (spawner is CreatureSpawnerContinuous continuousSpawner) {
                if (data.dead) {
                    continuousSpawner.SetDead(true);
                    continuousSpawner.SetLoaded();
                    continue;
                }
            }

            spawner.SpawnMobs(data.currentMobsAlive);
        }

        // Loading linked mob spawners
        foreach (SpawnerSaveData data in spawnersData) {
            foreach (var s in SpawnersManager.Instance.GetAllSpawners()) {
                if (s == null) {
                    Debug.LogError("[Save] Null spawner dans la liste !");
                }
            }

            MobSpawner spawner = SpawnersManager.Instance
                .GetAllSpawners()
                .Find(s => s != null && s.GetSpawnerID() == data.spawnerID);

            if (spawner == null) {
                Debug.LogWarning($"[Save] Spawner {data.spawnerID} introuvable dans la scène !");
                continue;
            }


            yield return new WaitForEndOfFrame();
            spawner.LoadLinkedMobSpawner();
        }
    }
    private IEnumerator LoadPlayer() {
        if (!ES3.KeyExists("Player", "LevelSave.es3"))
            yield break;

        PlayerSaveData data = ES3.Load<PlayerSaveData>("Player", "LevelSave.es3");

        // Position
        Player.Instance.transform.position = new Vector2(data.posX, data.posY);

        // HP
        Player.Instance.SetHP(data.health);

        // Inventory
        var currencyData = data.currencyData;
        var currencyRotationData = data.currencyRotationData;
        bool hasEmber = false;

        foreach (PlayerCurrencies.CurrencyType currencyType in Enum.GetValues(typeof(PlayerCurrencies.CurrencyType))) {
            string key = currencyType.ToString();

            if (currencyData.TryGetValue(key, out List<Vector3> positions)) {
                if (currencyRotationData.TryGetValue(key, out List<Quaternion> rotations)) {
                    UICurrencyManager.PlayerInventoryUI.LoadCurrencies(currencyType, positions, rotations);
                }

                if (currencyType == PlayerCurrencies.CurrencyType.ember && positions.Count != 0) {
                    hasEmber = true;
                }
            }
        }

        if(!hasEmber) {
            SoundManager.Instance.SetInitialEmberGiven(true);
        }

        yield return new WaitForEndOfFrame();

        // --- Primary gun ---
        if (data.primaryGun != null) {
            GunSO primarySO = PlayerShoot.Instance.GetGunSO(data.primaryGun.gunType);

            PlayerShoot.Instance.SetPrimaryWeaponSO(primarySO, false);
            PlayerShoot.Instance.SetActiveGun(primarySO.gunType, true, false);

            yield return new WaitForEndOfFrame();

            PlayerShoot.Instance.SetGunAmmo(primarySO, data.primaryGun.ammoClip, data.primaryGun.currentBullet);
            PlayerUI_AmmoBar.Instance.RefreshAmmoBar();
        }

        // --- Secondary gun ---
        if (data.hasSecondary && data.secondaryGun != null) {
            GunSO secondarySO = PlayerShoot.Instance.GetGunSO(data.secondaryGun.gunType);
            PlayerShoot.Instance.SetSecondaryWeaponSO(secondarySO, false);

            PlayerShoot.Instance.SetGunAmmo(secondarySO, data.secondaryGun.ammoClip, data.secondaryGun.currentBullet);
        }

        // --- Found guns (stock/inventaire) ---
        if (data.hasFoundGun && data.foundGuns != null) {
            foreach (GunData g in data.foundGuns) {
                GunSO gunSO = PlayerShoot.Instance.GetGunSO(g.gunType);
                PlayerShoot.Instance.SetGunSOInStock(gunSO);

                PlayerShoot.Instance.SetGunAmmo(gunSO, g.ammoClip, g.currentBullet);
            }
        }


        // --- Skills ---
        foreach (SkillData s in data.activeSkills) {
            SkillItem item = new SkillItem();
            item.Initialize(PlayerSkills.Instance.GetSkillSO(s.skillType)); // lookup ton SkillSO via un "SkillLibrary" ou SOManager
            item.currentLevel = s.level;

            PlayerSkills.Instance.AddActiveSkill(item, false);
        }

        foreach (SkillData s in data.passiveSkills) {
            SkillItem item = new SkillItem();
            item.Initialize(PlayerSkills.Instance.GetSkillSO(s.skillType));
            item.currentLevel = s.level;

            PlayerSkills.Instance.AddPassiveSkill(item, true, false);
        }

        PlayerSkills.Instance.SetPlayerSkillsInitialized();

        yield return null;
    }
    private IEnumerator LoadStructures() {
        if (!ES3.KeyExists("Structures", "LevelSave.es3"))
            yield break;

        yield return new WaitForEndOfFrame();

        Fire.Instance.gameObject.SetActive(true);
        Fire.Instance.ActivateInitialFire(false);
        StructuresManager.Instance.AddBuiltStructure(Fire.Instance);
        StructuresManager.Instance.AddBuiltStructure(Tent.Instance);
        Tent.Instance.gameObject.SetActive(true);
        Tent.Instance.SetStructureBuiltOnLoad(true);
        Tent.Instance.BuildInitialCampStructure();

        List<StructureSaveData> structuresData = ES3.Load<List<StructureSaveData>>("Structures", "LevelSave.es3 ");
        List<TrapSaveData> trapsData = ES3.Load<List<TrapSaveData>>("Structures_Traps", "LevelSave.es3 ");
        List<FireSaveData> fireData = ES3.Load<List<FireSaveData>>("Structures_Fire", "LevelSave.es3 ");
        List<CurrencyCrafterSaveData> crafterData = ES3.Load<List<CurrencyCrafterSaveData>>("Structures_CurrencyCrafters", "LevelSave.es3 ");
        List<SpecialTowerSaveData> towerData = ES3.Load<List<SpecialTowerSaveData>>("Structures_SpecialTowers", "LevelSave.es3 ");
        List<BarricadeSaveData> barricadeData = ES3.Load<List<BarricadeSaveData>>("Structures_Barricades", "LevelSave.es3 ");
        List<CurrencyStorageSaveData> storagesData = ES3.Load<List<CurrencyStorageSaveData>>("Structures_CurrencyStorages", "LevelSave.es3 ");

        int trapSaveDataIndex = 0;
        int fireSaveDataIndex = 0;
        int currencyCrafterSaveDataIndex = 0;
        int towerDataIndex = 0;
        int barricadesDataIndex = 0;
        int storageDataIndex = 0;

        foreach(StructureSaveData data in structuresData) {
            StructureSO.StructureType loadedStructureType = data.structureType;
            StructureSO loadedStructureSO = StructuresManager.Instance.GetStructureSO(loadedStructureType);
            Vector2 structurePos = new Vector2(data.posX, data.posY);


            if (loadedStructureSO.structureType != loadedStructureType) {
                Debug.LogError("Structure SO " + loadedStructureType + " Not referenced in StructuresManager");
            }

            bool structureBuilt = data.structureBuilt;

            if (structureBuilt) {
                yield return new WaitForEndOfFrame();

                if (loadedStructureType == StructureSO.StructureType.fire) {

                    if(fireData[fireSaveDataIndex].isMainFire) {
                        float fuelLevel = fireData[fireSaveDataIndex].currentFuelLevel;
                        Fire.Instance.SetFuelLevel(fuelLevel);
                        fireSaveDataIndex++;

                        continue;
                    }
                };

                if (data.structureType == StructureSO.StructureType.tent) {
                    Tent.Instance.transform.position = structurePos;
                    if (data.structureLevel != 1) {
                        yield return new WaitForEndOfFrame();
                        Tent.Instance.SetStructureLevel(data.structureLevel);
                    }
                    continue;
                };

                Vector3 position = new Vector3(structurePos.x, structurePos.y, 0);
                StructureLocation structureLocation = Instantiate(loadedStructureSO.structureLocationPrefab, position, Quaternion.identity).GetComponent<StructureLocation>();

                if (data.isWorldStructure) {
                    structureLocation.SetAsWorldStructureLocation();
                    structureLocation.SetStructureLocationWorldScaleX(data.worldStructureScaleX);
                }

                if (structureLocation is StructureLocation_Trap) {
                    StructureLocation_Trap structureLocation_Trap = (StructureLocation_Trap)structureLocation;
                    structureLocation_Trap.SetStructureSOToBuild(loadedStructureSO);
                }

                Structure structure = structureLocation.BuildStructure(true);

                yield return new WaitForEndOfFrame();

                if (data.structureLevel != 1) {
                    structure.SetStructureLevel(data.structureLevel);
                }

                yield return new WaitForEndOfFrame();


                if (loadedStructureSO.structureCategory == StructureSO.StructureCategory.trap) {
                    TrapSaveData trapData = trapsData[trapSaveDataIndex];
                    Structure_Trap trapStructure = structure as Structure_Trap;
                    trapStructure.SetCurrentRearmIndex(trapData.currentRearmIndex);
                    trapStructure.SetCurrentUseIndex(trapData.currentUseIndex);

                    trapSaveDataIndex++;
                };

                if (loadedStructureSO.structureCategory == StructureSO.StructureCategory.storage) {
                    CurrencyStorageSaveData storageData = storagesData[storageDataIndex];
                    CurrencyStorage storageStructure = structure as CurrencyStorage;
                    storageStructure.SetCurrencyAmountStored(storageData.currencyAmountStored);

                    if(loadedStructureSO.structureType == StructureSO.StructureType.currencyStorage_Objective) {
                        CurrencyStorage_Objective storageStructure_Obj = storageStructure as CurrencyStorage_Objective;
                        storageStructure_Obj.SetMaxCurrencyStorageIndex(storageData.maxCurrencyStorageIndex);
                    }

                    storageDataIndex++;
                };

                if (loadedStructureSO.structureType == StructureSO.StructureType.fire || loadedStructureSO.structureType == StructureSO.StructureType.secondaryFire) {

                    Fire fire = structure as Fire;
                    float fuelLevel = fireData[fireSaveDataIndex].currentFuelLevel;
                    fire.SetFuelLevel(fuelLevel);

                    if(fireData[fireSaveDataIndex].isSecondaryFire) {
                        fire.SetAsSecondaryFire();
                    }
                    if (fireData[fireSaveDataIndex].isEndLevelFire) {
                        fire.SetAsEndFire();
                    }

                    fireSaveDataIndex++;
                    continue;
                }; 
                
                if (loadedStructureSO.structureType == StructureSO.StructureType.barricade) {

                    Barricade barricade = structure as Barricade;
                    int barricadeHealth = barricadeData[barricadesDataIndex].barricadeHealth;
                    barricade.SetHealth(barricadeHealth);
                    barricadesDataIndex++;
                    continue;
                };

                if (loadedStructureSO.structureType == StructureSO.StructureType.machineGunTower || loadedStructureSO.structureType == StructureSO.StructureType.sniperTower || loadedStructureSO.structureType == StructureSO.StructureType.mortarTower) {

                    SpecialTower specialTower = structure as SpecialTower;
                    int ammoCount = towerData[towerDataIndex].currentAmmoClips;
                    specialTower.SetCurrentAmmoClips(ammoCount);
                    towerDataIndex++;
                    continue;
                };

                if (loadedStructureSO.structureType == StructureSO.StructureType.ammoCrafter || loadedStructureSO.structureType == StructureSO.StructureType.orbProcessor) {

                    CurrencyCrafter crafter = structure as CurrencyCrafter;
                    int currentBatches = crafterData[currencyCrafterSaveDataIndex].currentBatches;
                    float currencyCraftTimer = crafterData[currencyCrafterSaveDataIndex].currencyCraftTimer;
                    bool craftingCurrency = crafterData[currencyCrafterSaveDataIndex].craftingCurrency;
                    bool craftedCurrency = crafterData[currencyCrafterSaveDataIndex].craftedCurrency;
                    PlayerCurrencies.CurrencyType currencyTypeCrafted = crafterData[currencyCrafterSaveDataIndex].currencyTypeCrafted;

                    crafter.SetCurrencyTypeBeingCrafted(currencyTypeCrafted);
                    crafter.SetCurrencyCraftTimer(currencyCraftTimer);
                    crafter.SetCurrentBatches(currentBatches);
                    crafter.SetCraftingCurrency(craftingCurrency);
                    crafter.SetCraftedCurrency(craftedCurrency);

                    currencyCrafterSaveDataIndex++;
                    continue;
                };

            }
            else {

                PlayerCamp.Instance.AddStructureLocationLoaded(loadedStructureSO, structurePos, data.structureUnlocked);

            }
        }


        yield return null;
    }
    private IEnumerator LoadTraps() {
        if (ES3.KeyExists("TrapTypes", "LevelSave.es3")) {
            yield return new WaitForEndOfFrame();

            List<TrapItem.TrapType> trapTypes = ES3.Load<List<TrapItem.TrapType>>("TrapTypes", "LevelSave.es3");
            TrapManager.Instance.SetTrapTypesBoughtByPlayer(trapTypes);

        }


        if (ES3.KeyExists("TrapUpgrades", "LevelSave.es3")) {
            yield return new WaitForEndOfFrame();

            Dictionary<TrapItem.TrapType, Dictionary<TrapUpgradeSO.TrapUpgradeType, int>> trapUpgrades = ES3.Load<Dictionary<TrapItem.TrapType, Dictionary<TrapUpgradeSO.TrapUpgradeType, int>>>("TrapUpgrades", "LevelSave.es3");
            TrapManager.Instance.SetTrapUpgradeLevels(trapUpgrades);
        }


    }

    private IEnumerator LoadObstacles() {
        if (!ES3.KeyExists("Obstacles", "LevelSave.es3"))
            yield break;

        var list = ES3.Load<List<ObstacleSaveData>>("Obstacles", "LevelSave.es3");
        if (list == null || list.Count == 0)
            yield break;

        // on attend un frame LevelManager ait fini son Awake/Start
        yield return new WaitForEndOfFrame();

        foreach (var data in list) {
            if (string.IsNullOrEmpty(data.obstacleID)) continue;

            var obstacle = LevelManager.Instance
                .GetAllObstacles()
                .Find(o => o != null && o.GetObstacleID() == data.obstacleID);

            if (obstacle == null) {
                Debug.LogWarning($"[Save] Obstacle {data.obstacleID} introuvable.");
                continue;
            }

            if (data.built && !obstacle.GetBuilt()) {
                obstacle.BuildObstacle(false);
            }
        }
    }

    private IEnumerator LoadScavengableObstacles() {
        if (!ES3.KeyExists("ScavengableObstacles", "LevelSave.es3"))
            yield break;

        var list = ES3.Load<List<ScavengableObstacleSaveData>>("ScavengableObstacles", "LevelSave.es3");
        if (list == null || list.Count == 0)
            yield break;

        // on attend un frame LevelManager ait fini son Awake/Start
        yield return new WaitForEndOfFrame();

        foreach (var data in list) {
            if (string.IsNullOrEmpty(data.scavObstacleID)) continue;

            var obstacle = LevelManager.Instance
                .GetAllObstacles()
                .Find(o => o != null && o.GetObstacleID() == data.scavObstacleID);

            ScavengableObstacle scavObstacle = obstacle as ScavengableObstacle;
            yield return new WaitForEndOfFrame();

            if (scavObstacle == null) {
                Debug.LogWarning($"[Save] Obstacle {data.scavObstacleID} introuvable.");
                continue;
            }

            if(data.markedToScavenge) {
                scavObstacle.MarkToScavenge(true);
            }

            scavObstacle.SetHitsTaken(data.hitsTaken);
            scavObstacle.LoadHealth(data.health);
            scavObstacle.SetSpawnersTriggered(data.thresholdsTriggered);
        }
    }

    private IEnumerator LoadScavengables() {
        if (!ES3.KeyExists("Scavengables", "LevelSave.es3"))
            yield break;

        var list = ES3.Load<List<ScavengableSaveData>>("Scavengables", "LevelSave.es3");
        if (list == null || list.Count == 0)
            yield break;

        // on attend un frame LevelManager ait fini son Awake/Start
        yield return new WaitForEndOfFrame();

        foreach (var data in list) {
            if (string.IsNullOrEmpty(data.scavengableID)) continue;

            var scavengable = ScavengableManager.Instance
                .GetScavengablesList()
                .Find(o => o != null && o.GetScavengableID() == data.scavengableID);

            yield return new WaitForEndOfFrame();

            if (scavengable == null) {
                Debug.LogWarning($"[Save] Scavengable {data.scavengableID} introuvable.");
                continue;
            }

            if (data.markedToScavenge) {
                scavengable.MarkToScavenge(true);
            }

            scavengable.SetScavengingActive(data.scavengingActive);
            scavengable.SetHitsTaken(data.hitsTaken);
            scavengable.SetHealth(data.health);
            scavengable.SetTimeToMineOneResource(data.timeToMineOneResource);
        }
    }
    private IEnumerator LoadChests() {
        if (!ES3.KeyExists("Chests", "LevelSave.es3"))
            yield break;

        var list = ES3.Load<List<ChestSaveData>>("Chests", "LevelSave.es3");
        if (list == null || list.Count == 0)
            yield break;

        // on attend un frame LevelManager ait fini son Awake/Start
        yield return new WaitForEndOfFrame();

        foreach (var data in list) {
            if (string.IsNullOrEmpty(data.chestID)) continue;

            var chest
                = LevelManager.Instance
                .GetAllChests()
                .Find(o => o != null && o.GetChestID() == data.chestID);

            yield return new WaitForEndOfFrame();

            if (chest == null) {
                Debug.LogWarning($"[Save] Chest {data.chestID} introuvable.");
                continue;
            }

            chest.SetRewardOfferedToPlayer(data.rewardOfferedToPlayer);
            chest.SetChestPaid(data.chestPricePaid);
            chest.SetChestOpened(data.opened);
            chest.SetChestLocked(data.chestLocked);
        }
    }

    private IEnumerator LoadTrialAreas() {
        if (!ES3.KeyExists("TrialAreas", "LevelSave.es3"))
            yield break;

        var list = ES3.Load<List<TrialAreaSaveData>>("TrialAreas", "LevelSave.es3");
        if (list == null || list.Count == 0)
            yield break;

        // on attend un frame LevelManager ait fini son Awake/Start
        yield return new WaitForEndOfFrame();

        foreach (var data in list) {
            if (string.IsNullOrEmpty(data.trialAreaID)) continue;

            var trialArea
                = LevelManager.Instance
                .GetAllTrialAreas()
                .Find(o => o != null && o.GetTrialAreaID() == data.trialAreaID);

            yield return new WaitForEndOfFrame();

            if (trialArea == null) {
                Debug.LogWarning($"[Save] TrialArea {data.trialAreaID} introuvable.");
                continue;
            }

            trialArea.SetTrialAreaCompleted(data.trialCompleted);
        }
    }
    private IEnumerator LoadObjectives() {
        if (!ES3.KeyExists("LevelState", "LevelSave.es3")) yield break;

        yield return new WaitForEndOfFrame();

        LevelSaveData saveData = ES3.Load<LevelSaveData>("LevelState", "LevelSave.es3");

        LevelUI_ObjectiveUI.Instance.ShowObjectiveAfterDelay(saveData.currentObjectiveType, 2f);
        LevelUI_ObjectiveUI.Instance.SetSubObjectivesUIAfterDelay(saveData.currentSubObjectiveTypeList, 2f);
    }

    #endregion

    private void CreateLevelSaveCopy() {
        if (DebugManager.Instance.GetSaveAfterEachLevelDebug());

        string mainPath = "LevelSave.es3";

        if (!ES3.FileExists(mainPath)) return;

        int currentDay = DayNightManager.Instance.GetCurrentDay();
        string backupPath = "LevelSave_" + LevelManager.Instance.GetLevelSO() + "_Day_" + currentDay + ".es3";

        ES3.CopyFile(mainPath, backupPath);
    }

    public string GetLocalizedLastSaveText() {
        TimeSpan elapsed = DateTime.Now - lastSaveTime;
        int minutes = (int)elapsed.TotalMinutes;
        int seconds = elapsed.Seconds;

        // On envoie minutes et secondes dans les placeholders
        return LocalizationManager.Instance.GetLocalizedText("menu_lastProgressionSaved", minutes, seconds);
    }

    public bool GetLoadingSavedLevel() {
        return loadingSavedLevel;
    }
    public bool GetSavedOnce() {
        return gameSavedOnce;
    }

    public bool GetLoading() {
        return isLoading;
    }
}
