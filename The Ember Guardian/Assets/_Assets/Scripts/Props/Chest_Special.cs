using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest_Special : Chest
{


    [SerializeField] private GunSO gunSOInChest;
    [SerializeField] private List<SkillSO> skillSOListInChest;
    private SkillSO skillSOSelected;

    private bool rewardOfferedToPlayerStarted;

    protected override void Start() {
        base.Start();
        if (chestType == ChestType.skillChest) {
            SelectRandomSkill();
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
            chestOpened = true;
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
            PlayerShoot.Instance.SetActiveGun(gunSOInChest);
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
                Collectible collectible = Instantiate(CurrenciesManager.Instance.GetCurrencyPrefab(currencyType), orbSpawnPosition.position, Quaternion.identity).GetComponent<Collectible>();

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

    protected override void OnTriggerEnter2D(Collider2D collision) {
        if (rewardOfferedToPlayerStarted) return;

        base.OnTriggerEnter2D(collision);
    }

}
