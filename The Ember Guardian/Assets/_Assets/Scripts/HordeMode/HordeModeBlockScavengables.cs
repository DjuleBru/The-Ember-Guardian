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

    public void SetBlockAsScavengable(BlockSize blockSize) {
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
    public void SetBlockAsMine(BlockSize blockSize) {
        Scavengable scav = Instantiate(GetMinePrefab(), mineSpawnPosition.position, Quaternion.identity, mineSpawnPosition).GetComponent<Scavengable>();
        ScavengableManager.Instance.AddScavengable(scav);

        // If block is big, add 2 scavengables
        if (blockSize == BlockSize.Medium) {
            SpawnScavengable(GetOrbScavengablePrefab(), scavengableSpawnPositionWithMine1, standardScavengableOrbsToCollect, standardScavengableHitsToCollectOrb);
        }

        if (blockSize == BlockSize.Big) {
            SpawnScavengable(GetOrbScavengablePrefab(), scavengableSpawnPositionWithMine1, standardScavengableOrbsToCollect, standardScavengableHitsToCollectOrb);
            SpawnScavengable(GetOrbScavengablePrefab(), scavengableSpawnPositionWithMine2, standardScavengableOrbsToCollect, standardScavengableHitsToCollectOrb);
        }
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
}
