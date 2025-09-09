using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DogAbilityTemplate : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private Transform abilityStatContainer;
    [SerializeField] private Transform abilityStatTemplate;
    [SerializeField] private TextMeshProUGUI abilityStatText;
    [SerializeField] private TextMeshProUGUI abilityStatValue;


    public void InitializeAbility(HUBMerchantItem_DogTamerItem.DogTamerItemType itemType) {
        abilityStatTemplate.gameObject.SetActive(true);
        RefreshFonts();

        switch (itemType) {

            case HUBMerchantItem_DogTamerItem.DogTamerItemType.GermanShepherd_BiteAbility:
                abilityNameText.text = LocalizationManager.Instance.GetLocalizedText("dog_bite");

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_damage");
                abilityStatValue.text = DogStats.Instance.GetGermanShepherdBiteDamage().ToString();
                Instantiate(abilityStatTemplate, abilityStatContainer);

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_cooldown");
                abilityStatValue.text = DogStats.Instance.GetGermanShepherdBiteCooldown().ToString() + "s";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                break;

            case HUBMerchantItem_DogTamerItem.DogTamerItemType.GermanShepherd_DigResourceAbility:
                abilityNameText.text = LocalizationManager.Instance.GetLocalizedText("dog_dig");


                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("general_probability");
                abilityStatValue.text = DogStats.Instance.GetGermanShepherdDigResourceProbility().ToString() + "%";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                abilityStatText.text = "x2 " + LocalizationManager.Instance.GetLocalizedText("general_probability");
                abilityStatValue.text = DogStats.Instance.GetGermanShepherdDigResourceDoubleProbability().ToString() + "%";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_cooldown");
                abilityStatValue.text = DogStats.Instance.GetGermanShepherdDigResourceCooldown().ToString() + "s";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                break;

            case HUBMerchantItem_DogTamerItem.DogTamerItemType.GermanShepherd_AmbushDetectionAbility:
                abilityNameText.text = LocalizationManager.Instance.GetLocalizedText("dog_ambush");

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("general_probability");
                abilityStatValue.text = DogStats.Instance.GetGermanShepherdDetectAmbushProbability().ToString() + "%";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                break;



            case HUBMerchantItem_DogTamerItem.DogTamerItemType.Retreiver_BiteAbility:
                abilityNameText.text = LocalizationManager.Instance.GetLocalizedText("dog_bite");

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_damage");
                abilityStatValue.text = DogStats.Instance.GetRetreiverBiteDamage().ToString();
                Instantiate(abilityStatTemplate, abilityStatContainer);

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_cooldown");
                abilityStatValue.text = DogStats.Instance.GetRetreiverBiteCooldown().ToString() + "s";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                break;

            case HUBMerchantItem_DogTamerItem.DogTamerItemType.Retreiver_BuffWorkersAbility:
                abilityNameText.text = LocalizationManager.Instance.GetLocalizedText("dog_buffWorkers");

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("general_attackSpeed");
                abilityStatValue.text = DogStats.Instance.GetRetreiverBuffWorkersAmount().ToString() + "%";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("general_radius");
                abilityStatValue.text = DogStats.Instance.GetRetreiverBuffWorkersRadius().ToString() + "m";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                break;

            case HUBMerchantItem_DogTamerItem.DogTamerItemType.Retreiver_PickUpItemsAbility:
                abilityNameText.text = LocalizationManager.Instance.GetLocalizedText("Fetch Items");

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_range");
                abilityStatValue.text = DogStats.Instance.GetRetreiverInitialResourceDetectionRange().ToString() + "m";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                break;



            case HUBMerchantItem_DogTamerItem.DogTamerItemType.DarkCompanion_BiteAbility:
                abilityNameText.text = LocalizationManager.Instance.GetLocalizedText("Laser Shot");

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_damage");
                abilityStatValue.text = DogStats.Instance.GetDarkCompanionBiteDamage().ToString();
                Instantiate(abilityStatTemplate, abilityStatContainer);

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_cooldown");
                abilityStatValue.text = DogStats.Instance.GetDarkCompanionBiteCooldown().ToString() + "s";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                break;

            case HUBMerchantItem_DogTamerItem.DogTamerItemType.DarkCompanion_LaserAbility:
                abilityNameText.text = LocalizationManager.Instance.GetLocalizedText("Charged Beam");

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_damagePerSecond");
                abilityStatValue.text = ((int)(DogStats.Instance.GetDarkCompanionLaserDamage() / DogStats.Instance.GetDarkCompanionLaserTickCooldown())).ToString() + "/s";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_cooldown");
                abilityStatValue.text = DogStats.Instance.GetDarkCompanionLaserCooldown().ToString() + "s";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                break;

            case HUBMerchantItem_DogTamerItem.DogTamerItemType.DarkCompanion_StompAbility:
                abilityNameText.text = LocalizationManager.Instance.GetLocalizedText("Ground Stomp");

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_damage");
                abilityStatValue.text = DogStats.Instance.GetDarkCompanionStompDamage().ToString();
                Instantiate(abilityStatTemplate, abilityStatContainer);

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("general_stun");
                abilityStatValue.text = DogStats.Instance.GetDarkCompanionStompStunDuration().ToString() + "s";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                abilityStatText.text = LocalizationManager.Instance.GetLocalizedText("card_cooldown");
                abilityStatValue.text = DogStats.Instance.GetDarkCompanionStompCooldown().ToString() + "s";
                Instantiate(abilityStatTemplate, abilityStatContainer);

                break;

        }


        abilityStatTemplate.gameObject.SetActive(false);
    }

    private void RefreshFonts() {
        TMP_FontAsset font = LocalizationManager.Instance.GetCurrentFont();
        abilityNameText.font = font;
        abilityStatText.font = font;
        abilityStatValue.font = font;
    }
}
