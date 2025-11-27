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

    [SerializeField] private BlockSize blockSize;
    [SerializeField] private Transform gridAndObstacleParent;
    [SerializeField] private float blockWidth = 10f;
    [SerializeField] private float bridgeWidth = 3f;

    [SerializeField] private MobSpawner animalSpawner;
    [SerializeField] private Chest resourceChest;
    [SerializeField] private Transform mobSpawnerPrefab;
    [SerializeField] private Transform weaponChestPrefab;
    [SerializeField] private Transform trapChestPrefab;
    [SerializeField] private Transform minePrefab;

    [SerializeField] private Transform weaponShopPrefab;
    [SerializeField] private Transform structuresShopPrefab;
    [SerializeField] private Transform heroShopPrefab;
    [SerializeField] private Transform dogShopPrefab;
    [SerializeField] private Transform workerShopPrefab;

    private BlockType blockType;
    private ShopType shopType;
    private float direction;

    private float tinyBlockSizeRewardMultiplier = .75f;
    private float smallBlockSizeRewardMultiplier = 1f;
    private float mediumBlockSizeRewardMultiplier = 1.5f;
    private float largeBlockSizeRewardMultiplier = 2f;

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

    public void SetBlockType(BlockType blockType) {
        Debug.Log("SetBlockType " + blockType);

        this.blockType = blockType;

        if (blockType == BlockType.Animals) {
            HandleAnimalBlock();
        } else {
            animalSpawner.gameObject.SetActive(false);
        }

        if (blockType == BlockType.ResourceChest) {
            HandleResourceChestBlock();
        } else {
            resourceChest.gameObject.SetActive(false);
        }

        if(blockType == BlockType.Scavengables) {
            GetComponent<HordeModeBlockScavengables>().SetBlockAsScavengable(blockSize);
        }

        if (blockType == BlockType.Mine) {
            GetComponent<HordeModeBlockScavengables>().SetBlockAsMine(blockSize);
        }
    }

    private void HandleAnimalBlock() {
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

    private void HandleResourceChestBlock() {
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

    private void SetSpawnersToCenterPosition() {
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

}
