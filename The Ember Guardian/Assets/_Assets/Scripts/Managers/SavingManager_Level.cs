using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingManager_Level : MonoBehaviour
{
    public static SavingManager_Level Instance;
    private bool loadingSavedLevel;

    private void Awake() {
        Instance = this;

        if (ES3.FileExists("LevelSave.es3")) {
            Debug.Log($"[Save] Save file found for scene" + "LevelSave" + " loading...");
            loadingSavedLevel = true;
        }
        else {
            Debug.Log($"[Save] No save file for scene {"LevelSave"}, starting fresh.");
        }
    }

    private void Start() {

        if (loadingSavedLevel) {
            LoadGame();
        }

        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
    }



    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        if (DayNightManager.Instance.GetCurrentDay() == 0) return;
        SaveGame();
    }

    #region SAVE

    private void SaveGame() {
        StartCoroutine(SaveAfterDelay(1f));
    }
    private IEnumerator SaveAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        SaveRecruitedWorkers();
        SaveSpawners();
        SavePlayer();
        SaveStructures();
    }

    private void SaveRecruitedWorkers() {
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
            ES3.Save("Workers", workersData, "LevelSave.es3");
        }
    }

    private void SaveSpawners() {
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

        ES3.Save("Spawners", spawnersData, "LevelSave.es3");
    }

    private void SavePlayer() {
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

        ES3.Save("Player", data, "LevelSave.es3");
    }

    private void SaveStructures() {
        List<StructureSaveData> structuresData = new List<StructureSaveData>();
        List<TrapSaveData> trapsData = new List<TrapSaveData>();

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

            data.structureType = structure.GetStructureSO().structureType;
            data.posX = structure.transform.position.x;
            data.posY = structure.transform.position.y;
            data.structureBuilt = true;
            data.structureLevel = structure.GetStructureLevel(); ;

            structuresData.Add(data);

            if (structure.GetStructureSO().structureCategory == StructureSO.StructureCategory.trap) {
                Structure_Trap trapStructure = structure as Structure_Trap;

                TrapSaveData data_trap = new TrapSaveData();
                data_trap.currentRearmIndex = trapStructure.GetCurrentRearmIndex();
                data_trap.currentUseIndex = trapStructure.GetCurrentUseIndex();

                trapsData.Add(data_trap);
            }
        }


        ES3.Save("Structures", structuresData, "LevelSave.es3");
        ES3.Save("Structures_Traps", trapsData, "LevelSave.es3");
    }

    #endregion

    #region LOAD

    [Button]
    public void LoadGame() {
        StartCoroutine(LoadWorkers());
        StartCoroutine(LoadSpawners());
        StartCoroutine(LoadPlayer());
        StartCoroutine(LoadStructures());
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

        foreach (PlayerCurrencies.CurrencyType currencyType in Enum.GetValues(typeof(PlayerCurrencies.CurrencyType))) {
            string key = currencyType.ToString();

            if (currencyData.TryGetValue(key, out List<Vector3> positions)) {
                if (currencyRotationData.TryGetValue(key, out List<Quaternion> rotations)) {
                    UICurrencyManager.PlayerInventoryUI.LoadCurrencies(currencyType, positions, rotations);
                }
            }
            else {
                Debug.LogWarning($"[Save] Aucun data trouvé pour {currencyType}, inventaire vide.");
            }
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

            PlayerSkills.Instance.AddPassiveSkill(item, false);
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

        List<StructureSaveData> structuresData = ES3.Load<List<StructureSaveData>>("Structures", "LevelSave.es3 ");
        List<TrapSaveData> trapsData = ES3.Load<List<TrapSaveData>>("Structures_Traps", "LevelSave.es3 ");

        int trapSaveDataIndex = 0;

        foreach(StructureSaveData data in structuresData) {
            StructureSO.StructureType loadedStructureType = data.structureType;
            StructureSO loadedStructureSO = StructuresManager.Instance.GetStructureSO(loadedStructureType);
            Vector2 structurePos = new Vector2(data.posX, data.posY);

            bool structureBuilt = data.structureBuilt;

            if (structureBuilt) {

                if(data.structureType == StructureSO.StructureType.fire) continue;

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

                if(structureLocation is StructureLocation_Trap) {
                    StructureLocation_Trap structureLocation_Trap = (StructureLocation_Trap)structureLocation;
                    structureLocation_Trap.SetStructureSOToBuild(loadedStructureSO);
                }

                Structure structure = structureLocation.BuildStructure(true);
                yield return new WaitForEndOfFrame();

                if (data.structureLevel != 1) {
                    structure.SetStructureLevel(data.structureLevel);
                }


                if (loadedStructureSO.structureCategory == StructureSO.StructureCategory.trap) {
                    TrapSaveData trapData = trapsData[trapSaveDataIndex];
                    Structure_Trap trapStructure = structure as Structure_Trap;
                    trapStructure.SetCurrentRearmIndex(trapData.currentRearmIndex);
                    trapStructure.SetCurrentUseIndex(trapData.currentUseIndex);

                    trapSaveDataIndex++;
                };


            }
            else {

                PlayerCamp.Instance.AddStructureLocationLoaded(loadedStructureSO, structurePos, data.structureUnlocked);

            }
        }


        yield return null;
    }

    #endregion


    public bool GetLoadingSavedLevel() {
        return loadingSavedLevel;
    }
}
