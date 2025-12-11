using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeModeBlockScavengables : MonoBehaviour
{

    [SerializeField] private List<Transform> spawnPositions;
    [SerializeField] private Transform mineSpawnPosition;
    [SerializeField] private Transform scavengableSpawnPositionWithMine1;
    [SerializeField] private Transform scavengableSpawnPositionWithMine2;

    [SerializeField] private Transform mine_VG;
    [SerializeField] private Transform mine_CC;
    [SerializeField] private Transform mine_LH;
    [SerializeField] private Transform mine_FD;
    private Scavengable mineSpawned;

    [SerializeField] private Transform orbScavengable_VG_CC;
    [SerializeField] private Transform orbScavengable_LH;
    [SerializeField] private Transform orbScavengable_FD;

    [SerializeField] private Transform smallOrbScavengable_VG_CC;
    [SerializeField] private Transform smallOrbScavengable_LH;
    [SerializeField] private Transform smallOrbScavengable_FD;

    [SerializeField] private Transform ammoScavengable_VG_CC;
    [SerializeField] private Transform ammoScavengable_LH;
    [SerializeField] private Transform ammoScavengable_FD;

    private int standardScavengableOrbsToCollect = 10;
    private int standardScavengableSmallOrbsToCollect = 15;
    private int standardScavengableAmmoToCollect = 8;

    private int standardScavengableHitsToCollectOrb = 20;
    private int standardScavengableHitsToCollectSmallOrb = 5;
    private int standardScavengableHitsToCollectAmmo = 20;

    private void Awake() {
        foreach(Transform transform in spawnPositions) {
            transform.GetComponent<SpriteRenderer>().enabled = false;
        }

        if(mineSpawnPosition != null) {
            mineSpawnPosition.GetComponent<SpriteRenderer>().enabled = false;
        }

        if(scavengableSpawnPositionWithMine1 != null) {
            scavengableSpawnPositionWithMine1.GetComponent<SpriteRenderer>().enabled = false;

        }
        if (scavengableSpawnPositionWithMine2 != null) {
            scavengableSpawnPositionWithMine2.GetComponent<SpriteRenderer>().enabled = false;

        }
    }

    public void SetBlockAsScavengable(BlockSize blockSize, BlockSaveData saveData) {
        if(saveData != null) {
            LoadScavengables(saveData);
            return;
        }

        int randomType = UnityEngine.Random.Range(1, 3);
        int scavAmount = GetScavengableSpawnAmount(blockSize);
        int orbsToCollect = standardScavengableOrbsToCollect;
        int hitsToCollect = standardScavengableHitsToCollectOrb;

        if (randomType == 1) {
            // Only Big Orbs
            for(int i = 0; i < scavAmount; i++) {
                Transform prefab = GetOrbScavengablePrefab();
                SpawnScavengable(prefab, spawnPositions[i], orbsToCollect, hitsToCollect);
            }
        }

        if (randomType == 2) {
            // Big Orbs, Small Orbs
            for (int i = 0; i < scavAmount; i++) {
                Transform prefab = GetOrbScavengablePrefab();

                if(i > (float)scavAmount / 2) {
                    orbsToCollect = standardScavengableSmallOrbsToCollect;
                    hitsToCollect = standardScavengableHitsToCollectSmallOrb;
                    prefab = GetSmallOrbScavengablePrefab();
                }

                SpawnScavengable(prefab, spawnPositions[i], orbsToCollect, hitsToCollect);
            }
        }

        if (randomType == 3) {
            // Big Orbs, Ammo
            for (int i = 0; i < scavAmount; i++) {
                Transform prefab = GetOrbScavengablePrefab();

                if (i > (float)scavAmount / 2) {
                    prefab = GetAmmoScavengablePrefab();
                    orbsToCollect = standardScavengableAmmoToCollect;
                    hitsToCollect = standardScavengableHitsToCollectAmmo;
                }

                SpawnScavengable(prefab, spawnPositions[i], orbsToCollect, hitsToCollect);    
            }
        }

    }

    public void SetBlockAsMine(BlockSize blockSize, BlockSaveData saveData) {

        Scavengable scav = Instantiate(GetMinePrefab(), mineSpawnPosition.position, Quaternion.identity, mineSpawnPosition).GetComponent<Scavengable>();
        mineSpawned = scav;
        ScavengableManager.Instance.AddScavengable(scav);

        if(transform.position.x < 0) {
            mineSpawned.transform.localScale = new Vector3(-1, 1, 1);
        }

        if (saveData != null) {
            LoadScavengables(saveData);
            StartCoroutine(LoadScavengableAfterFrame(mineSpawned, saveData.mineSaveData));
            return;
        }

        // If block is big, add 2 scavengables
        if (blockSize == BlockSize.Medium) {
            SpawnScavengable(GetOrbScavengablePrefab(), scavengableSpawnPositionWithMine1, standardScavengableOrbsToCollect, standardScavengableHitsToCollectOrb);
        }

        if (blockSize == BlockSize.Big) {
            SpawnScavengable(GetOrbScavengablePrefab(), scavengableSpawnPositionWithMine1, standardScavengableOrbsToCollect, standardScavengableHitsToCollectOrb);
            SpawnScavengable(GetOrbScavengablePrefab(), scavengableSpawnPositionWithMine2, standardScavengableOrbsToCollect, standardScavengableHitsToCollectOrb);
        }
    }

    public void LoadScavengables(BlockSaveData saveData) {

        foreach (HordeScavengableSaveData scavSaveData in saveData.scavengableSaveDataList) {

            Transform prefab = FindPrefabByName(scavSaveData.prefabName);
            if (prefab == null) continue;

            var pos = spawnPositions[scavSaveData.hordeModeBlockIndex];

            Scavengable scav = Instantiate(prefab, pos.position, Quaternion.identity, pos)
                .GetComponent<Scavengable>();

            StartCoroutine(LoadScavengableAfterFrame(scav, scavSaveData));

        }
    }

    public IEnumerator LoadScavengableAfterFrame(Scavengable scav, HordeScavengableSaveData scavSaveData) {
        yield return new WaitForEndOfFrame();

        scav.SetParameters(scavSaveData.currenciesToCollect, scavSaveData.hitsToCollect);
        scav.SetScavengingActive(scavSaveData.scavengingActive);
        scav.SetHitsTaken(scavSaveData.hitsTaken);
        scav.SetHealth(scavSaveData.health);
        scav.SetTimeToMineOneResource(scavSaveData.timeToMineOneResource);

        if (scavSaveData.markedToScavenge) {
            scav.MarkToScavenge(true);
        }

        ScavengableManager.Instance.AddScavengable(scav);

    }

    private void SpawnScavengable(Transform prefab, Transform parent, int currenciesToCollect, int hitsToCollect) {
        Scavengable scav = Instantiate(prefab, parent.position, Quaternion.identity, parent).GetComponent<Scavengable>();
        scav.SetParameters(currenciesToCollect, hitsToCollect);

        StartCoroutine(AddScavengableToManagerAfterFrame(scav));
    }

    private Transform GetOrbScavengablePrefab() {
        LevelSO.LevelEnvironment env = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();

        if(env == LevelSO.LevelEnvironment.TheLumenHollow) {
            return orbScavengable_LH;
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            return orbScavengable_FD;
        }

        return orbScavengable_VG_CC;
    }
    private Transform GetSmallOrbScavengablePrefab() {
        LevelSO.LevelEnvironment env = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();

        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            return smallOrbScavengable_LH;
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            return smallOrbScavengable_FD;
        }

        return smallOrbScavengable_VG_CC;
    }
    private Transform GetAmmoScavengablePrefab() {
        LevelSO.LevelEnvironment env = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();

        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            return ammoScavengable_LH;
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            return ammoScavengable_FD;
        }

        return ammoScavengable_VG_CC;
    }
    private Transform GetMinePrefab() {
        LevelSO.LevelEnvironment env = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();

        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            return mine_LH;
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            return mine_FD;
        }
        if (env == LevelSO.LevelEnvironment.CorruptedCity) {
            return mine_CC;
        }

        return mine_VG;
    }
    private int GetScavengableSpawnAmount(BlockSize blockSize) {
        if (blockSize == BlockSize.Tiny) return 1;
        if (blockSize == BlockSize.Small) return 2;
        if (blockSize == BlockSize.Medium) return 4;
        if (blockSize == BlockSize.Big) return 6;
        return 0;
    }

    public Scavengable SpawnSingleScav(Transform parent, int currencies, int hits) {
        Scavengable scav = Instantiate(GetOrbScavengablePrefab(), parent.position, Quaternion.identity, parent)
            .GetComponent<Scavengable>();

        scav.SetParameters(currencies, hits);
        StartCoroutine(AddScavengableToManagerAfterFrame(scav));
        return scav;
    }

    private IEnumerator AddScavengableToManagerAfterFrame(Scavengable scav) {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        ScavengableManager.Instance.AddScavengable(scav);
    }

    public List<HordeScavengableSaveData> GetScavengableSaveData() {
        // Save scavengables
        List<HordeScavengableSaveData> saveDataList = new List<HordeScavengableSaveData>();

        for (int i = 0; i < spawnPositions.Count; i++) {
            Transform pos = spawnPositions[i];

            foreach (Transform child in pos) {
                Scavengable scav = child.GetComponent<Scavengable>();
                if (scav == null) continue;

                HordeScavengableSaveData saveData = new HordeScavengableSaveData {
                    markedToScavenge = scav.GetMarkedToScavenge(),
                    health = scav.GetHealth(),
                    hitsTaken = scav.GetHitsTaken(),
                    timeToMineOneResource = scav.GetTimeToMineOneResource(),
                    scavengingActive = scav.GetScavengingActive(),
                    currencyTypeCollected = scav.GetCurrencyTypeCollected(),
                    currenciesToCollect = scav.GetCurrencyAmountCollected(),
                    hitsToCollect = scav.GetHitsToCollect(),
                    hordeModeBlockIndex = i,
                    prefabName = scav.transform.name,
                };


                Debug.Log(saveData.prefabName);
                saveDataList.Add(saveData);
            }
        }

        return saveDataList;
    }

    public HordeScavengableSaveData GetMineSaveData() {
        // Save scavengables

        HordeScavengableSaveData saveData = new HordeScavengableSaveData {
            markedToScavenge = mineSpawned.GetMarkedToScavenge(),
            health = mineSpawned.GetHealth(),
            hitsTaken = mineSpawned.GetHitsTaken(),
            timeToMineOneResource = mineSpawned.GetTimeToMineOneResource(),
            scavengingActive = mineSpawned.GetScavengingActive(),
            currencyTypeCollected = mineSpawned.GetCurrencyTypeCollected(),
            currenciesToCollect = mineSpawned.GetCurrencyAmountCollected(),
            hitsToCollect = mineSpawned.GetHitsToCollect(),
            prefabName = mineSpawned.transform.name,
        };

        return saveData;
    }
    public Transform FindPrefabByName(string prefabName) {
        if (string.IsNullOrEmpty(prefabName))
            return null;

        string cleanName = prefabName.Replace("(Clone)", "").Trim();
        // Mines
        if (mine_VG != null && mine_VG.name == cleanName) return mine_VG;
        if (mine_CC != null && mine_CC.name == cleanName) return mine_CC;
        if (mine_LH != null && mine_LH.name == cleanName) return mine_LH;
        if (mine_FD != null && mine_FD.name == cleanName) return mine_FD;

        // Big Orbs
        if (orbScavengable_VG_CC != null && orbScavengable_VG_CC.name == cleanName) return orbScavengable_VG_CC;
        if (orbScavengable_LH != null && orbScavengable_LH.name == cleanName) return orbScavengable_LH;
        if (orbScavengable_FD != null && orbScavengable_FD.name == cleanName) return orbScavengable_FD;

        // Small Orbs
        if (smallOrbScavengable_VG_CC != null && smallOrbScavengable_VG_CC.name == cleanName) return smallOrbScavengable_VG_CC;
        if (smallOrbScavengable_LH != null && smallOrbScavengable_LH.name == cleanName) return smallOrbScavengable_LH;
        if (smallOrbScavengable_FD != null && smallOrbScavengable_FD.name == cleanName) return smallOrbScavengable_FD;

        // Ammo
        if (ammoScavengable_VG_CC != null && ammoScavengable_VG_CC.name == cleanName) return ammoScavengable_VG_CC;
        if (ammoScavengable_LH != null && ammoScavengable_LH.name == cleanName) return ammoScavengable_LH;
        if (ammoScavengable_FD != null && ammoScavengable_FD.name == cleanName) return ammoScavengable_FD;

        return null;
    }
}
