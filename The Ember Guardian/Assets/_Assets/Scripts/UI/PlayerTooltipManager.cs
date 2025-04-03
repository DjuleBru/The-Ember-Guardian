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
    #endregion

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnWeaponSecondaryAbilityPerformed += GameInput_OnWeaponSecondaryAbilityPerformed;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        PlayerShoot.Instance.OnPlayerTryReload_EmptyAmmoBeltButAmmoInBag += PlayerShoot_OnPlayerTryReload_EmptyAmmoBeltButAmmoInBag;
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;

        gunSOAbilityPreparedList = ES3.Load("gunSOAbilityPreparedList", new List<GunSO>());

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            rifleText1 = LocalizationManager.Instance.GetLocalizedText("menu_press");
            rifleText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_rifleText2");
            shotgunText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
            shotgunText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_shotgunText2");
            smgText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
            smgText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_smgText2");
            sniperText1 = LocalizationManager.Instance.GetLocalizedText("menu_hold");
            sniperText2 = LocalizationManager.Instance.GetLocalizedText("tooltip_sniperText2");
        }
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        tryReloadAttemptAmount = 0;
    }

    private void PlayerShoot_OnPlayerTryReload_EmptyAmmoBeltButAmmoInBag(object sender, System.EventArgs e) {
        tryReloadAttemptAmount++;

        if(tryReloadAttemptAmount > 3) {
            tooltipLeft.ShowTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("Hold"), LocalizationManager.Instance.GetLocalizedText("tooltip_ammoTip"), InputControlIcons.Control.Reload, 4f);
        }
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        TryShowGunSecondaryAbilityTooltip();
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {

        HubMerchant hubMerchant = (HubMerchant)sender;
        if(hubMerchant.GetHubMerchantType() == HubMerchant.HubMerchantType.GunMerchant) {

            TryShowGunSecondaryAbilityTooltip();

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

            preparedDisplayTime = 10f;
            StartCoroutine(ShowPreparedTooltipInstructionAfterDelay(1.5f));
            gunSOAbilityPreparedList.Remove(PlayerShoot.Instance.GetHeldGunSO());
            secondaryWeaponAbilityShown = true;

            ES3.Save("gunSOAbilityPreparedList", gunSOAbilityPreparedList);
        }

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
    }
}
