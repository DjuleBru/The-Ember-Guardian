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

    private List<GunSO.GunType> gunTypesAbilityPreparedList;
    private bool prepareSwapGunTooltip;
    private bool selectOtherGunTooltipShown;
    private bool selectOtherGunTooltipBeingShown;
    private bool huntingFlagTooFarShown;
    private bool shootBarricadeShown;
    private bool secondaryWeaponAbilityShown;

    private bool prepareSwapDogTooltip;
    private bool selectOtherDogTooltipShown;
    private bool selectOtherDogTooltipBeingShown;

    private int tryReloadAttemptAmount;

    #region GUN SECONDARY ABILITIES INSTRUCTIONS
    private string rifleText1;
    private string rifleText2;    
    private string rifleText3;
    private string rifleText4;
    private string shotgunText1;
    private string shotgunText2;
    private string shotgunText3;
    private string shotgunText4;
    private string smgText1;
    private string smgText2;
    private string smgText3;
    private string smgText4;
    private string sniperText1;
    private string sniperText2;
    private string sniperText3;
    private string sniperText4;
    private string AAGunText1;
    private string AAGunText2;
    private string AAGunText3;
    private string AAGunText4;
    private string AssaultRifleText1;
    private string AssaultRifleText2;
    private string AssaultRifleText3;
    private string AssaultRifleText4;
    private string GrenadeLauncherText1;
    private string GrenadeLauncherText2;
    private string GrenadeLauncherText3;
    private string GrenadeLauncherText4;
    private string LMGText1;
    private string LMGText2;
    private string LMGText3;
    private string LMGText4;
    private string MinigunText1;
    private string MinigunText2;
    private string MinigunText3;
    private string MinigunText4;
    private string PistolText1;
    private string PistolText2;
    private string PistolText3;
    private string PistolText4;
    private string RevolverText1;
    private string RevolverText2;
    private string RevolverText3;
    private string RevolverText4;
    private string RocketLauncherText1;
    private string RocketLauncherText2;
    private string RocketLauncherText3;
    private string RocketLauncherText4;
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
        ParticleCollision.OnAnyParticleHitBarricade += ParticleCollision_OnAnyParticleHitBarricade;
        Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;

        //gunSOAbilityPreparedList = ES3.Load("gunSOAbilityPreparedList", new List<GunSO>());
        gunTypesAbilityPreparedList = ES3.Load("gunTypesAbilityPreparedList", new List<GunSO.GunType>());
        selectOtherGunTooltipShown = ES3.Load("selectOtherGunTooltipShown", false);
        huntingFlagTooFarShown = ES3.Load("huntingFlagTooFarShown", false);
        shootBarricadeShown = ES3.Load("shootBarricadeShown", false);
        selectOtherDogTooltipShown = ES3.Load("selectOtherDogTooltipShown", false);
        prepareSwapDogTooltip = ES3.Load("prepareSwapDogTooltip", false);

        rifleText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        rifleText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        rifleText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        rifleText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        shotgunText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
        shotgunText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_shotgunText2");
        shotgunText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        shotgunText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        smgText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
        smgText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_smgText2");
        smgText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        smgText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        sniperText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
        sniperText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_sniperText2");
        sniperText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        sniperText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        AAGunText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        AAGunText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_AAGunText2");
        AAGunText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        AAGunText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        AssaultRifleText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
        AssaultRifleText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_AssaultRifleText2");
        AssaultRifleText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        AssaultRifleText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        GrenadeLauncherText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        GrenadeLauncherText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_GrenadeLauncherText2");
        GrenadeLauncherText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        GrenadeLauncherText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        LMGText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        LMGText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_LMGText2");
        LMGText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        LMGText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        MinigunText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
        MinigunText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_MinigunText2");
        MinigunText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        MinigunText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        PistolText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        PistolText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_PistolText2");
        PistolText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        PistolText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        RevolverText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        RevolverText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_RevolverText2");
        RevolverText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        RevolverText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
        RocketLauncherText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        RocketLauncherText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_RocketLauncherText2");
        RocketLauncherText3 = LocalizationManager.Instance.GetLocalizedText("menu_press");
        RocketLauncherText4 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
    }

    private void ParticleCollision_OnAnyParticleHitBarricade(object sender, System.EventArgs e) {
        if (shootBarricadeShown) return;

        PlayerTalkUI.Instance.ShowTalkText(LocalizationManager.Instance.GetLocalizedText("tooltip_shootBarricade"), 5f);
        ES3.Save("shootBarricadeShown", true);
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

        if (hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.DogTamer) {

            TryShowChangeDogTooltip();

        }
    }

    private void TryShowGunSecondaryAbilityTooltip() {
        //if (gunSOAbilityPreparedList == null) return;

        if (gunTypesAbilityPreparedList.Contains(PlayerShoot.Instance.GetHeldGunSO().gunType)) {

            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.Rifle) {
                if(PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(rifleText1, rifleText2, InputControlIcons.Control.SecondaryGunAbility);
                } else {
                    PrepareTooltipInstruction(rifleText3, rifleText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.Shotgun) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(shotgunText1, shotgunText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(shotgunText3, shotgunText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.SMG) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(smgText1, smgText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(smgText3, smgText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.Sniper) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(sniperText1, sniperText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(sniperText3, sniperText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }

            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.AAGun) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(AAGunText1, AAGunText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(AAGunText3, AAGunText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.AssaultRifle) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(AssaultRifleText1, AssaultRifleText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(AssaultRifleText3, AssaultRifleText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.GrenadeLauncher) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(GrenadeLauncherText1, GrenadeLauncherText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(GrenadeLauncherText3, GrenadeLauncherText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.LMG) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(LMGText1, LMGText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(LMGText3, LMGText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }

            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.Pistol) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(PistolText1, PistolText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(PistolText3, PistolText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.Revolver) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(RevolverText1, RevolverText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(RevolverText3, RevolverText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.RocketLauncher) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(RocketLauncherText1, RocketLauncherText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(RocketLauncherText3, RocketLauncherText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }
            if (PlayerShoot.Instance.GetHeldGunSO().gunType == GunSO.GunType.MiniGun) {
                if (PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                    PrepareTooltipInstruction(MinigunText1, MinigunText2, InputControlIcons.Control.SecondaryGunAbility);
                }
                else {
                    PrepareTooltipInstruction(MinigunText3, MinigunText4, InputControlIcons.Control.SecondaryGunAbility);
                }
            }

            preparedDisplayTime = 10f;
            StartCoroutine(ShowPreparedTooltipInstructionAfterDelay(1.5f));
            gunTypesAbilityPreparedList.Remove(PlayerShoot.Instance.GetHeldGunSO().gunType);
            secondaryWeaponAbilityShown = true;

            ES3.Save("gunTypesAbilityPreparedList", gunTypesAbilityPreparedList);
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

    private void TryShowChangeDogTooltip() {
        if (!prepareSwapDogTooltip) return;
        if (selectOtherDogTooltipShown) return;

        PrepareTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("tooltip_changeDogType"), InputControlIcons.Control.OpenPlayerMenu, 99f);
        StartCoroutine(ShowPreparedTooltipInstructionAfterDelay(1.5f));

        selectOtherDogTooltipBeingShown = true;
        selectOtherDogTooltipShown = true;
        prepareSwapDogTooltip = false;
        ES3.Save("selectOtherDogTooltipShown", true);
        ES3.Save("prepareSwapDogTooltip", false);
    }

    private void Dog_OnDogTypeChanged(object sender, Dog.OnDogTypeChangedEventArgs e) {
        if (!selectOtherDogTooltipBeingShown) return;
        tooltipLeft.HideTooltip(1f);
        selectOtherDogTooltipBeingShown = false;
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
    
    public void PrepareTooltipInstruction(string text1ToShow, string text2ToShow, InputControlIcons.Control controlType, float displayTime = 0) {
        preparedText1ToShow = text1ToShow;
        preparedText2ToShow = text2ToShow;
        preparedControlType = controlType;
        preparedDisplayTime = displayTime;
    }

    public void SetGunSOAbilityPrepared(GunSO gunSO) {
        gunTypesAbilityPreparedList.Add(gunSO.gunType);
        ES3.Save("gunTypesAbilityPreparedList", gunTypesAbilityPreparedList);
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
        ParticleCollision.OnAnyParticleHitBarricade -= ParticleCollision_OnAnyParticleHitBarricade;
    }
}
