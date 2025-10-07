using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTooltipManager : MonoBehaviour
{
    public static PlayerTooltipManager Instance;

    [SerializeField] private PlayerWorldUITooltip tooltipLeft;
    [SerializeField] private PlayerWorldUITooltip tooltipRight;

    private InputControlIcons.Control preparedControlType;
    private string preparedText1ToShow;
    private string preparedText2ToShow;
    private float preparedDisplayTime;

    private List<GunSO> gunSOAbilityPreparedList;
    private bool prepareSwapGunTooltip;
    private bool selectOtherGunTooltipShown;
    private bool selectOtherGunTooltipBeingShown;
    private bool huntingFlagTooFarShown;
    private bool secondaryWeaponAbilityShown;

    private int tryReloadAttemptAmount;

    #region GUN SECONDARY ABILITIES INSTRUCTIONS
    private string rifleText1;
    private string rifleText2;
    private string shotgunText1;
    private string shotgunText2;
    private string smgText1;
    private string smgText2;
    private string sniperText1;
    private string sniperText2;
    private string AAGunText1;
    private string AAGunText2;
    private string AssaultRifleText1;
    private string AssaultRifleText2;
    private string GrenadeLauncherText1;
    private string GrenadeLauncherText2;
    private string LMGText1;
    private string LMGText2;
    private string MinigunText1;
    private string MinigunText2;
    private string PistolText1;
    private string PistolText2;
    private string RevolverText1;
    private string RevolverText2;
    private string RocketLauncherText1;
    private string RocketLauncherText2;
    #endregion

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnWeaponSecondaryAbilityPerformed += GameInput_OnWeaponSecondaryAbilityPerformed;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        HubMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_OnAnyHubMerchantItemBought;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        PlayerShoot.Instance.OnPlayerTryReload_EmptyAmmoBeltButAmmoInBag += PlayerShoot_OnPlayerTryReload_EmptyAmmoBeltButAmmoInBag;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenuUI_OnPlayerTabOpened;
        HuntingFlag_PlayerDefined.OnHuntingFlagTooFarCarriedByPlayer += HuntingFlag_PlayerDefined_OnHuntingFlagTooFar;

        gunSOAbilityPreparedList = ES3.Load("gunSOAbilityPreparedList", new List<GunSO>());
        selectOtherGunTooltipShown = ES3.Load("selectOtherGunTooltipShown", false);
        huntingFlagTooFarShown = ES3.Load("huntingFlagTooFarShown", false);

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {

            rifleText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
            rifleText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
            shotgunText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
            shotgunText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_shotgunText2");
            smgText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
            smgText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_smgText2");
            sniperText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
            sniperText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_sniperText2");
            AAGunText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
            AAGunText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_AAGunText2");
            AssaultRifleText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
            AssaultRifleText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_AssaultRifleText2");
            GrenadeLauncherText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
            GrenadeLauncherText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_GrenadeLauncherText2");
            LMGText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
            LMGText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_LMGText2");
            MinigunText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
            MinigunText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_MinigunText2");
            PistolText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
            PistolText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_PistolText2");
            RevolverText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
            RevolverText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_RevolverText2");
            RocketLauncherText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
            RocketLauncherText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_RocketLauncherText2");
        }
    }

    private void PlayerTabMenuUI_OnPlayerTabOpened(object sender, System.EventArgs e) {
        if(selectOtherGunTooltipBeingShown) {
            tooltipLeft.HideTooltip();
        }
    }

    private void HubMerchantItem_OnAnyHubMerchantItemBought(object sender, System.EventArgs e) {
        HubMerchantItem merchantItem = (HubMerchantItem)sender;
        if (merchantItem != null) {
            if (merchantItem is HUBMerchantItem_GunMerchantItem) {
                HUBMerchantItem_GunMerchantItem gunItem = merchantItem as HUBMerchantItem_GunMerchantItem;

                if (gunItem.GetGunItemCategory() == HUBMerchantItem_GunMerchantItem.GunItemCategory.newGun) {
                    if (selectOtherGunTooltipShown) return;

                    prepareSwapGunTooltip = true;
                }
            }
        }
    }
    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        tryReloadAttemptAmount = 0;
    }

    private void PlayerShoot_OnPlayerTryReload_EmptyAmmoBeltButAmmoInBag(object sender, System.EventArgs e) {
        tryReloadAttemptAmount++;

        if(tryReloadAttemptAmount > 3) {
            tooltipLeft.ShowTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("menu_hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_ammoTip"), InputControlIcons.Control.Reload, 4f);
        }
    }

    private void HuntingFlag_PlayerDefined_OnHuntingFlagTooFar(object sender, System.EventArgs e) {
        if (huntingFlagTooFarShown) return;

        PlayerTalkUI.Instance.ShowTalkText(LocalizationManager.Instance.GetLocalizedText("tooltip_huntingFlagTooFar"), 5f);
        ES3.Save("huntingFlagTooFarShown", true);
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        TryShowGunSecondaryAbilityTooltip();
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {

        HubMerchant hubMerchant = (HubMerchant)sender;
        if(hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.GunMerchant) {

            TryShowGunSecondaryAbilityTooltip();
            TryShowChangeGunTooltip();

        }
    }

    private void TryShowGunSecondaryAbilityTooltip() {
        if (gunSOAbilityPreparedList.Contains(PlayerShoot.Instance.GetHeldGunSO())) {

            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.Rifle) {
                PrepareTooltipInstruction(rifleText1, rifleText2, InputControlIcons.Control.SecondaryGunAbility);
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.Shotgun) {
                PrepareTooltipInstruction(shotgunText1, shotgunText2, InputControlIcons.Control.SecondaryGunAbility);
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.UZI) {
                PrepareTooltipInstruction(smgText1, smgText2, InputControlIcons.Control.SecondaryGunAbility);
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.Sniper) {
                PrepareTooltipInstruction(sniperText1, sniperText2, InputControlIcons.Control.SecondaryGunAbility);
            }

            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.AAGun) {
                PrepareTooltipInstruction(AAGunText1, AAGunText2, InputControlIcons.Control.SecondaryGunAbility);
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.AssaultRifle) {
                PrepareTooltipInstruction(AssaultRifleText1, AssaultRifleText2, InputControlIcons.Control.SecondaryGunAbility);
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.GrenadeLauncher) {
                PrepareTooltipInstruction(GrenadeLauncherText1, GrenadeLauncherText2, InputControlIcons.Control.SecondaryGunAbility);
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.LMG) {
                PrepareTooltipInstruction(LMGText1, LMGText2, InputControlIcons.Control.SecondaryGunAbility);
            }

            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.Pistol) {
                PrepareTooltipInstruction(PistolText1, PistolText2, InputControlIcons.Control.SecondaryGunAbility);
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.Revolver) {
                PrepareTooltipInstruction(RevolverText1, RevolverText2, InputControlIcons.Control.SecondaryGunAbility);
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.RocketLauncher) {
                PrepareTooltipInstruction(RocketLauncherText1, RocketLauncherText2, InputControlIcons.Control.SecondaryGunAbility);
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.MiniGun) {
                PrepareTooltipInstruction(MinigunText1, MinigunText2, InputControlIcons.Control.SecondaryGunAbility);
            }

            preparedDisplayTime = 10f;
            StartCoroutine(ShowPreparedTooltipInstructionAfterDelay(1.5f));
            gunSOAbilityPreparedList.Remove(PlayerShoot.Instance.GetHeldGunSO());
            secondaryWeaponAbilityShown = true;

            ES3.Save("gunSOAbilityPreparedList", gunSOAbilityPreparedList);
        }

    }

    private void TryShowChangeGunTooltip() {
        if (!prepareSwapGunTooltip) return;
        if (selectOtherGunTooltipShown) return;

        PrepareTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("tooltip_selectOtherWeapon"), InputControlIcons.Control.OpenPlayerMenu, 99f);
        StartCoroutine(ShowPreparedTooltipInstructionAfterDelay(1.5f));

        selectOtherGunTooltipShown = true;
        selectOtherGunTooltipBeingShown = true;
        ES3.Save("selectOtherGunTooltipShown", true);
    }

    private void GameInput_OnWeaponSecondaryAbilityPerformed(object sender, System.EventArgs e) {
        if(secondaryWeaponAbilityShown) {
            tooltipLeft.HideTooltip(3f);
            secondaryWeaponAbilityShown = false;
        }
    }

    private IEnumerator ShowPreparedTooltipInstructionAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        tooltipLeft.ShowTooltipInstruction(preparedText1ToShow, preparedText2ToShow, preparedControlType, preparedDisplayTime);
    }

    public void PrepareGunSecondaryAbilityTooltipInstruction(GunSO gunSO) {
        if (gunSO.gunType == GunSO.GunType.Rifle) {
            PrepareTooltipInstruction(rifleText1, rifleText2, InputControlIcons.Control.SecondaryGunAbility);
        }
        if (gunSO.gunType == GunSO.GunType.Shotgun) {
            PrepareTooltipInstruction(shotgunText1, shotgunText2, InputControlIcons.Control.SecondaryGunAbility);
        }
        if (gunSO.gunType == GunSO.GunType.UZI) {
            PrepareTooltipInstruction(smgText1, smgText2, InputControlIcons.Control.SecondaryGunAbility);
        }
        if (gunSO.gunType == GunSO.GunType.Sniper) {
            PrepareTooltipInstruction(sniperText1, sniperText2, InputControlIcons.Control.SecondaryGunAbility);
        }
    }

    public void PrepareTooltipInstruction(string text1ToShow, string text2ToShow, InputControlIcons.Control controlType, float displayTime = 0) {
        preparedText1ToShow = text1ToShow;
        preparedText2ToShow = text2ToShow;
        preparedControlType = controlType;
        preparedDisplayTime = displayTime;
    }

    public void SetGunSOAbilityPrepared(GunSO gunSO) {
        gunSOAbilityPreparedList.Add(gunSO);
        ES3.Save("gunSOAbilityPreparedList", gunSOAbilityPreparedList);
    }

    public PlayerWorldUITooltip GetTooltipLeft() {
        return tooltipLeft;
    }

    public PlayerWorldUITooltip GetTooltipRight() {
        return tooltipRight;
    }

    public void SetWeaponSecondaryAbilityShown() {
        secondaryWeaponAbilityShown = true;
    }

    private void OnDestroy() {
        GameInput.Instance.OnWeaponSecondaryAbilityPerformed -= GameInput_OnWeaponSecondaryAbilityPerformed;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        PlayerShoot.Instance.OnPlayerSwappedGun -= PlayerShoot_OnPlayerSwappedGun;
        HubMerchantItem.OnAnyHubMerchantItemBought -= HubMerchantItem_OnAnyHubMerchantItemBought;
        HuntingFlag_PlayerDefined.OnHuntingFlagTooFarCarriedByPlayer -= HuntingFlag_PlayerDefined_OnHuntingFlagTooFar;
    }
}
