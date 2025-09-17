using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest_Special : Chest
{


    [SerializeField] private GunSO gunSOInChest;
    [SerializeField] private List<SkillSO> skillSOListInChest;
    [SerializeField] private bool replacePrimaryWeapon;
    [SerializeField] private bool findingWeaponUnlocksItInShop;
    [SerializeField] private HUBMerchantItem_GunMerchantItem.GunItemType weaponMerchantItemType;

    private SkillSO skillSOSelected;

    private bool rewardOfferedToPlayerStarted;

    protected override void Start() {
        base.Start();
        if (chestType == ChestType.skillChest) {
            StartCoroutine(CheckSkillStatueActive());
            SelectRandomSkill();
        }

        bool weaponBoughtInShop = MetaProgressionManager.Instance.GetMerchantItemBought(weaponMerchantItemType.ToString() + " " + gunSOInChest.ToString());
        if (findingWeaponUnlocksItInShop && weaponBoughtInShop) {
            gameObject.SetActive(false);
        }
    }

    private IEnumerator CheckSkillStatueActive() {
        yield return new WaitForSeconds(.1f);
        if (PlayerCamp.Instance.GetHasSkillMerchantInLayoutAndUnlocked()) {
            gameObject.SetActive(false);
        }
    }

    private void SelectRandomSkill() {
        skillSOSelected = skillSOListInChest[UnityEngine.Random.Range(0, skillSOListInChest.Count)];
    }

    protected override void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        if (playerPayingCurrencies) {
            payCurrencyUI.SetPlayerInteracting(false);
            return;
        }

        if (payToOpenChest) {
            if (!chestPricePaid) return;
        };

        if(!chestOpenedAnimationOver) {
            if (chestOpenedAnimationStarted) return;
            chestOpened = true;
            chestOpenedAnimationStarted = true;
            StartCoroutine(OpenChestCoroutine(false));
            InvokeOnChestOpened();

        } else {
            if (rewardOfferedToPlayerStarted) return;
            rewardOfferedToPlayerStarted = true;
            StartCoroutine(OfferRewardToPlayer());

        }
      
    }

    private IEnumerator OfferRewardToPlayer() {

        StartCoroutine(MakeChestDisappear(5f));

        if (chestType == ChestType.weaponChest) {

            PlayerShoot.Instance.ReplaceHeldWeaponSO(gunSOInChest);
            CheckUnlockNewWeapon();

            yield return new WaitForSeconds(.2f);
            PlayerShoot.Instance.SetGunToMaxAmmo(gunSOInChest);

        }
        if (chestType == ChestType.skillChest) {
            SkillItem skillItem = new SkillItem();
            skillItem.Initialize(skillSOSelected);

            if(PlayerSkills.Instance.GetActiveSkillLeft() != null && PlayerSkills.Instance.GetActiveSkillLeft().skillType == skillSOSelected.skillType) {
                skillItem.currentLevel = PlayerSkills.Instance.GetActiveSkillLeft().currentLevel + 1;
            }

            if (PlayerSkills.Instance.GetActiveSkillRight() != null && PlayerSkills.Instance.GetActiveSkillRight().skillType == skillSOSelected.skillType) {
                skillItem.currentLevel = PlayerSkills.Instance.GetActiveSkillRight().currentLevel + 1;
            }

            if (skillSOSelected.itemType == MerchantItem.MerchantItemType.ActiveSkill) {
                PlayerSkills.Instance.AddActiveSkill(skillItem);
            }
            if (skillSOSelected.itemType == MerchantItem.MerchantItemType.PassiveSkill) {
                PlayerSkills.Instance.AddPassiveSkill(skillItem);
            }
        }


        yield return new WaitForSeconds(.5f);

        int j = 0;
        foreach (PlayerCurrencies.CurrencyType currencyType in currencyTypeToRewardList) {
            int rewardAmount = rewardAmountList[j];

            for (int i = 0; i < rewardAmount; i++) {

                bool secondaryGunUnlocked = PlayerShoot.Instance.GetSecondaryGunSO() != null;
                PlayerCurrencies.CurrencyType currencyTypeToReward = currencyType;

                if (PlayerShoot.Instance.GetPrimaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special || (secondaryGunUnlocked && PlayerShoot.Instance.GetSecondaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special)) {
                    // Player has at least 1 special ammo weapon:
                    float randomFloat = UnityEngine.Random.value;
                    if (randomFloat < 0.5f) {
                        currencyTypeToReward = PlayerCurrencies.CurrencyType.ammo_special;
                        rewardAmount /= 2;
                    }
                    else {
                        if (PlayerShoot.Instance.GetPrimaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special && (secondaryGunUnlocked && PlayerShoot.Instance.GetSecondaryGunSO().ammoTypeUsed == PlayerCurrencies.CurrencyType.ammo_special)) {
                            // Player has at 2 special ammo weapons:
                            currencyTypeToReward = PlayerCurrencies.CurrencyType.ammo_special;
                            rewardAmount /= 2;
                        }
                    }
                }

                Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyTypeToReward), orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();

                InvokeOnAnyChestSpawnedCollectibles(currencyType);

                collectible.ApplyRandomUpwardsForce(5, 8);
                collectible.SetCollectibleUnInteractable(.75f);
                yield return new WaitForSeconds(.2f);
            }
            j++;
        }
    }

    public SkillSO GetSkillSO() {
        return skillSOSelected;
    }

    protected void CheckUnlockNewWeapon() {
        bool weaponBoughtInShop = MetaProgressionManager.Instance.GetMerchantItemBought(weaponMerchantItemType.ToString() + " " + gunSOInChest.ToString());
        if (weaponBoughtInShop) return;
        if (!findingWeaponUnlocksItInShop) return;

        string gunShopItemString = weaponMerchantItemType.ToString() + " " + gunSOInChest.ToString();

        MetaProgressionManager.Instance.SetHubMerchantItemUnlocked(gunShopItemString, true);
        MetaProgressionManager.Instance.SetHubMerchantItemBought(gunShopItemString, true);
        MetaProgressionManager.Instance.SetHubMerchantItemNewlyUnlocked(gunShopItemString, true);
        MetaProgressionManager.Instance.SetGunUnlocked(gunSOInChest, true);

    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        if (rewardOfferedToPlayerStarted) return;

        base.OnTriggerEnter2D(collision);
    }

}
