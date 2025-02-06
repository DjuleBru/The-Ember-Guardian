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

    #region GUN SECONDARY ABILITIES INSTRUCTIONS
    private string rifleText1 = "Press";
    private string rifleText2 = "Switch fire modes";
    private string shotgunText1 = "Hold";
    private string shotgunText2 = "Load focused shot";
    private string smgText1 = "Hold";
    private string smgText2 = "Activate overclock";
    private string sniperText1 = "Hold";
    private string sniperText2 = "Aim sight";
    #endregion

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnWeaponSecondaryAbilityPerformed += GameInput_OnWeaponSecondaryAbilityPerformed;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;

        gunSOAbilityPreparedList = ES3.Load("gunSOAbilityPreparedList", new List<GunSO>());
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        TryShowGunSecondaryAbilityTooltip();
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, System.EventArgs e) {

        if(!MetaProgressionManager.Instance.GetFirstGunBoughtTooltipShown()) {
            StartCoroutine(ShowPreparedTooltipInstructionAfterDelay(1f));
            MetaProgressionManager.Instance.SetFirstGunBoughtTooltipShown();
        } else {
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
        Debug.Log("PrepareGunSecondaryAbilityTooltipInstruction" + gunSO);
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
