using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType {
    CombatOnly,
    Mine,
    Animals,
    Scavengables,
    ResourceChest,
    WeaponChest,
    TrapChest,
    Shop,
    FastTravelTP,
    None,
}

public enum ShopType {
    Weapons,
    Structures,
    Hero,
    Dog,
    Worker,
}

public class HordeModeBlock : MonoBehaviour
{

    [SerializeField] protected BlockSize blockSize;
    [SerializeField] protected Transform gridAndObstacleParent;
    [SerializeField] protected GameObject dayCreatureSpawnerTemplate; // Template de spawner
    [SerializeField] protected float blockWidth = 10f;
    [SerializeField] protected float bridgeWidth = 3f;
    [SerializeField] private float baseRadiusToRoam = 5f;           // Rayon de base pour les spawners

    [SerializeField] protected MobSpawner animalSpawner;
    [SerializeField] protected Chest resourceChest;
    [SerializeField] protected Chest_Special weaponChest;
    [SerializeField] protected Chest trapChest;
    [SerializeField] protected StructureLocation fastTravelTPLocation;

    public static List<GunSO> gunSOAssignedToWeaponChests = new List<GunSO>();

    protected List<BlockType> blockTypeList = new List<BlockType>();
    protected ShopType shopType;
    protected float direction;

    protected float tinyBlockSizeRewardMultiplier = .75f;
    protected float smallBlockSizeRewardMultiplier = 1f;
    protected float mediumBlockSizeRewardMultiplier = 1.5f;
    protected float largeBlockSizeRewardMultiplier = 2f;

    protected virtual void Start() {
        DayCreatureSpawnerManager_HordeMode.Instance.GenerateSpawnersForBlock(this);
    }

    public float GetTotalSpacing() {
        return blockWidth + bridgeWidth;
    }

    public void SetDir(float dir) {
        direction = dir;
        if (dir < 0) {
            Vector3 scale = new Vector3(-1, 1, 1);
            gridAndObstacleParent.transform.localScale = scale;
        }

        SetSpawnersToCenterPosition();
    }

    public void AddBlockType(BlockType blockType) {
        Debug.Log(this + " AddBlockType " + blockType);

        blockTypeList.Add(blockType);

        if (blockType == BlockType.Animals) {
            HandleAnimalBlock();
        } else {
            if (!HasBlockType(BlockType.Animals)) {
                animalSpawner.gameObject.SetActive(false);
            }
        }

        if (blockType == BlockType.ResourceChest) {
            HandleResourceChestBlock();
        } else {
            if (!HasBlockType(BlockType.ResourceChest)) {
                resourceChest.gameObject.SetActive(false);
            }
        }

        if(blockType == BlockType.Scavengables) {
            GetComponent<HordeModeBlockScavengables>().SetBlockAsScavengable(blockSize);
        }

        if (blockType == BlockType.Mine) {
            GetComponent<HordeModeBlockScavengables>().SetBlockAsMine(blockSize);
        }

        if (blockType == BlockType.WeaponChest) {
            StartCoroutine(HandleWeaponChestBlockAfterFrames());
        } else {
            if(!HasBlockType(BlockType.WeaponChest)) {
                weaponChest.gameObject.SetActive(false);
            }
        }

        if (blockType == BlockType.TrapChest) {
            StartCoroutine(HandleTrapChestBlockAfterFrames());
        }
        else {
            if (!HasBlockType(BlockType.TrapChest)) {
                trapChest.gameObject.SetActive(false);
            }
        }

        if (blockType == BlockType.FastTravelTP) {
            StartCoroutine(HandleFastTravelTPAfterFrames());
        }
        else {
            if (!HasBlockType(BlockType.FastTravelTP)) {
                fastTravelTPLocation.gameObject.SetActive(false);
            }
        }
    }

    public bool HasBlockType(BlockType blockType) {
        return blockTypeList.Contains(blockType);
    }

    protected void HandleAnimalBlock() {
        // Récupère toutes les valeurs de l'énum
        Array values = Enum.GetValues(typeof(AnimalSO.AnimalType));
        // Sélectionne une valeur aléatoire
        AnimalSO.AnimalType randomAnimal = (AnimalSO.AnimalType)values.GetValue(UnityEngine.Random.Range(0, values.Length));

        int animalAmountToSpawn = 0;
        if(randomAnimal == AnimalSO.AnimalType.Rat) {
            animalAmountToSpawn = 12;
        }
        if (randomAnimal == AnimalSO.AnimalType.Fox) {
            animalAmountToSpawn = 8;
        }
        if (randomAnimal == AnimalSO.AnimalType.Elk) {
            animalAmountToSpawn = 5;
        }
        if (randomAnimal == AnimalSO.AnimalType.Deer) {
            animalAmountToSpawn = 4;
        }

        float radiusToRoamAround = (blockWidth / 2f);

        animalAmountToSpawn = Mathf.RoundToInt(animalAmountToSpawn * GetSizeRewardMultiplier());

        animalSpawner.SetSpawnerParameters(AnimalManager.Instance.GetAnimalPrefab(randomAnimal), animalAmountToSpawn, radiusToRoamAround);

    }

    protected void HandleResourceChestBlock() {
        int rewardType = UnityEngine.Random.Range(1, 4);
        Chest.ChestType chestType = Chest.ChestType.orbChest;
        List< PlayerCurrencies.CurrencyType > currencyTypeToRewardList = new List<PlayerCurrencies.CurrencyType>();
        List<int> rewardAmountList = new List<int>();

        if (rewardType == 1) {
            // Full Big Orbs
            chestType = Chest.ChestType.orbChest;
            currencyTypeToRewardList.Add(PlayerCurrencies.CurrencyType.bigBlueOrb);

            int orbsToReward = 12;
            orbsToReward = Mathf.RoundToInt(orbsToReward * GetSizeRewardMultiplier());
            rewardAmountList.Add(orbsToReward);
        }

        if (rewardType == 2) {
            // Big Orbs, Small Orbs
            chestType = Chest.ChestType.orbChest;
            currencyTypeToRewardList.Add(PlayerCurrencies.CurrencyType.bigBlueOrb);
            int orbsToReward = 6;
            orbsToReward = Mathf.RoundToInt(orbsToReward * GetSizeRewardMultiplier());
            rewardAmountList.Add(orbsToReward);

            currencyTypeToRewardList.Add(PlayerCurrencies.CurrencyType.smallBlueOrb);
            int smallBlueOrbReward = 8;
            smallBlueOrbReward = Mathf.RoundToInt(smallBlueOrbReward * GetSizeRewardMultiplier());
            rewardAmountList.Add(smallBlueOrbReward);
        }

        if (rewardType == 3) {
            // Ammo, Big Orbs
            chestType = Chest.ChestType.ammoChest;
            currencyTypeToRewardList.Add(PlayerCurrencies.CurrencyType.ammo);

            int ammoReward = 8;
            ammoReward = Mathf.RoundToInt(ammoReward * GetSizeRewardMultiplier());
            rewardAmountList.Add(ammoReward);

            currencyTypeToRewardList.Add(PlayerCurrencies.CurrencyType.bigBlueOrb);

            int orbReward = 4;
            orbReward = Mathf.RoundToInt(orbReward * GetSizeRewardMultiplier());
            rewardAmountList.Add(orbReward);
        }

        if (rewardType == 4) {
            //  Big Orbs, Ammo
            chestType = Chest.ChestType.orbChest;
            currencyTypeToRewardList.Add(PlayerCurrencies.CurrencyType.bigBlueOrb);
            int orbReward = 8;
            orbReward = Mathf.RoundToInt(orbReward * GetSizeRewardMultiplier());
            rewardAmountList.Add(orbReward);

            currencyTypeToRewardList.Add(PlayerCurrencies.CurrencyType.ammo);

            int ammoReward = 4;
            ammoReward = Mathf.RoundToInt(ammoReward * GetSizeRewardMultiplier());
            rewardAmountList.Add(ammoReward);
        }

        resourceChest.SetChestParameters(chestType, currencyTypeToRewardList, rewardAmountList);

    }

    protected void HandleWeaponChestBlock() {
        List<GunSO> gunTypesUnlocked = GetWeaponSOUnlockedAndNotCarriedByPlayerAndNotAssigned();

        if (gunTypesUnlocked.Count > 0) {
            weaponChest.gameObject.SetActive(true);
            GunSO gunSO = gunTypesUnlocked[UnityEngine.Random.Range(0, gunTypesUnlocked.Count)];
            weaponChest.SetGunSOInChest(gunSO);
            gunSOAssignedToWeaponChests.Add(gunSO);
        } else {
            weaponChest.gameObject.SetActive(false);
        }

    }

    protected void HandleFastTravelTP() {
        fastTravelTPLocation.gameObject.SetActive(true);
        fastTravelTPLocation.UnlockStructureLocation();
        fastTravelTPLocation.SetAsWorldStructureLocation();
        fastTravelTPLocation.SetBought();
    }

    protected IEnumerator HandleFastTravelTPAfterFrames() {
        yield return new WaitForSeconds(.1f);
        HandleFastTravelTP();
    }

    public List<GunSO> GetWeaponSOUnlockedAndNotCarriedByPlayerAndNotAssigned() {
        List<GunSO> gunsSOUnlockedList = new List<GunSO>();

        foreach (GunSO gunSO in PlayerShoot.Instance.GetAllGunSOList()) {
            if (HordeModeProgressionManager.Instance.GetWeaponUnlocked(gunSO.gunType) && !PlayerShoot.Instance.GetIsCarryingGun(gunSO) && !gunSOAssignedToWeaponChests.Contains(gunSO)) {
                gunsSOUnlockedList.Add(gunSO);
            }

        }

        return gunsSOUnlockedList;
    }

    protected void HandleTrapChestBlock() {
        List<TrapSO> trapTypesUnlocked = HordeModeProgressionManager.Instance.GetTrapUnlockedList();

        List<PlayerCurrencies.CurrencyType> rewardsList = new List<PlayerCurrencies.CurrencyType>();

        PlayerCurrencies.CurrencyType reward = CurrenciesManager.Instance.GetTrapCurrencyType(trapTypesUnlocked[UnityEngine.Random.Range(0, trapTypesUnlocked.Count)].trapType);
        rewardsList.Add(reward);

        int amount = UnityEngine.Random.Range(2, 5);
        if (reward == PlayerCurrencies.CurrencyType.spikeEjector) {
            amount = UnityEngine.Random.Range(3, 7);
        }
        List<int> rewardsListAmount = new List<int> { amount };
        trapChest.SetChestParameters(Chest.ChestType.trapChest, rewardsList, rewardsListAmount);
    }

    protected IEnumerator HandleWeaponChestBlockAfterFrames() {
        yield return new WaitForSeconds(.1f);

        HandleWeaponChestBlock();
    }

    protected IEnumerator HandleTrapChestBlockAfterFrames() {
        yield return new WaitForSeconds(.1f);

        HandleTrapChestBlock();
    }

    protected void SetSpawnersToCenterPosition() {
        Vector3 localCenterPosition = new Vector3(blockWidth / 2 * direction, 0, 0);
        animalSpawner.transform.localPosition = localCenterPosition;
        resourceChest.transform.localPosition = localCenterPosition;
    }

    public void SetShopType(ShopType shopType) {
        Debug.Log("SetShopType " + shopType);
        this.shopType = shopType;
    }

    public float GetSizeRewardMultiplier() {
        if (blockSize == BlockSize.Big) return largeBlockSizeRewardMultiplier;
        if (blockSize == BlockSize.Small) return smallBlockSizeRewardMultiplier;
        if (blockSize == BlockSize.Medium) return mediumBlockSizeRewardMultiplier;
        if (blockSize == BlockSize.Tiny) return tinyBlockSizeRewardMultiplier;
        return tinyBlockSizeRewardMultiplier;
    }

    public Vector3 GetRightExitPosition() {
        // Toujours à droite dans l'espace monde
        return transform.position + Vector3.right * GetTotalSpacing() * direction;
    }

    public void PlayerEnteredBlock() {
        HordeModeMapGenerationManager.Instance.PlayerEnteredBlock(this);
    }

    public virtual void GenerateCreatureSpawners(List<CreatureSO> selectedCreatures, List<int> spawnAmounts, List<int> eliteAmounts) {
        foreach (var creatureIndex in System.Linq.Enumerable.Range(0, selectedCreatures.Count)) {

            Vector3 randomPosition = GetRandomSpawnerPosition();

            Debug.Log(this + " randomPosition " + randomPosition);
            GameObject spawnerObj = Instantiate(dayCreatureSpawnerTemplate, randomPosition, Quaternion.identity, transform);
            spawnerObj.SetActive(true);

            DayCreatureSpawner spawner = spawnerObj.GetComponent<DayCreatureSpawner>();
            float radius = UnityEngine.Random.Range(baseRadiusToRoam * 0.25f, baseRadiusToRoam * 2f);

            spawner.InitializeDayCreatureSpawner(selectedCreatures[creatureIndex], spawnAmounts[creatureIndex], eliteAmounts[creatureIndex], radius);
        }
    }

    private Vector3 GetRandomSpawnerPosition() {
        // On garde le centre comme position de base
        float halfWidth = blockWidth / 2f;

        // Décalage horizontal max : on laisse 60% de marge à gauche et à droite
        float margin = blockWidth * 0.2f;
        float minX = halfWidth + margin;
        float maxX = halfWidth - margin;

        float randomX = UnityEngine.Random.Range(minX, maxX);

        // Appliquer le facteur direction pour que ce soit dans le bon sens
        randomX *= direction;

        Vector3 spawnerPos = new Vector3(transform.position.x + randomX, .5f, 0f);

        Debug.Log(this + " minX " + minX);
        Debug.Log(this + " maxX " + maxX);
        Debug.Log(this + " randomX " + randomX);
        Debug.Log(this + " GetRandomSpawnerPosition " + spawnerPos + " transfor position " + transform.position);

        // Z reste à 0 (2D)
        return spawnerPos;
    }

    public BlockSize GetBlockSize() {
        return blockSize;
    }

}
