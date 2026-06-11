using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DogStatsPanel : MonoBehaviour
{
    [SerializeField] private Transform abilityContainer;
    [SerializeField] private Transform abilityTemplate;

    private void Start() {
        Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;
        DogStats.Instance.OnNewAbilityUnlocked += Dog_OnNewAbilityUnlocked;
        DogStats.Instance.OnAbilityUpgraded += Dog_OnAbilityUpgraded;

        RefreshDogAbilities(Dog.Instance.GetDogType());
    }

    private void Dog_OnAbilityUpgraded(object sender, System.EventArgs e) {
        RefreshDogAbilities(Dog.Instance.GetDogType());
    }

    private void Dog_OnNewAbilityUnlocked(object sender, System.EventArgs e) {
        RefreshDogAbilities(Dog.Instance.GetDogType());
    }

    private void Dog_OnDogTypeChanged(object sender, System.EventArgs e) {
        RefreshDogAbilities(Dog.Instance.GetDogType());
    }

    private void RefreshDogAbilities(Dog.DogType dogType) {
        abilityTemplate.gameObject.SetActive(true);

        foreach (Transform child in abilityContainer) {
            if (child == abilityTemplate) continue;
            Destroy(child.gameObject);
        }

        if(dogType == Dog.DogType.GermanShepherd) {
            if(DogStats.Instance.GetGermanShepherdBiteAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.GermanShepherd_BiteAbility);
            }
            if (DogStats.Instance.GetGermanShepherdDigResourceAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.GermanShepherd_DigResourceAbility);
            }
            if (DogStats.Instance.GetGermanShepherdDetectAmbushAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.GermanShepherd_AmbushDetectionAbility);
            }
        }

        if (dogType == Dog.DogType.GoldenRetreiver) {
            if (DogStats.Instance.GetRetreiverBiteAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.Retreiver_BiteAbility);
            }
            if (DogStats.Instance.GetRetreiverBuffWorkersAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.Retreiver_BuffWorkersAbility);
            }
            if (DogStats.Instance.GetRetreiverPickUpItemsAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.Retreiver_PickUpItemsAbility);
            }
        }

        if (dogType == Dog.DogType.DarkCompanion) {
            if (DogStats.Instance.GetDarkCompanionBiteAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.DarkCompanion_BiteAbility);
            }
            if (DogStats.Instance.GetDarkCompanionLaserAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.DarkCompanion_LaserAbility);
            }
            if (DogStats.Instance.GetDarkCompanionStompAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.DarkCompanion_StompAbility);
            }
        }

        if (dogType == Dog.DogType.Robodog) {
            if (DogStats.Instance.GetRobodogMissilesAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.Robodog_MissilesAbility);
            }
            if (DogStats.Instance.GetRobodogSpeedUpAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.Robodog_SpeedUpAbility);
            }
            if (DogStats.Instance.GetRobodogAmmoCraftAbilityUnlocked()) {
                DogAbilityTemplate ability = Instantiate(abilityTemplate, abilityContainer).GetComponent<DogAbilityTemplate>();
                ability.InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType.Robodog_AmmoFactory);
            }
        }

        abilityTemplate.gameObject.SetActive(false);
    }
}
