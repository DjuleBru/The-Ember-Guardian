using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponStatsPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject pelletsPerShotGameObject;

    [SerializeField] private TextMeshProUGUI dpsStatText;
    [SerializeField] private TextMeshProUGUI bulletDmgStat;
    [SerializeField] private TextMeshProUGUI pelletsPerShotStat;
    [SerializeField] private TextMeshProUGUI cooldownStat;
    [SerializeField] private TextMeshProUGUI shotsPerClipStat;
    [SerializeField] private TextMeshProUGUI maxAmmoStat;
    [SerializeField] private TextMeshProUGUI reloadTimeStat;
    [SerializeField] private TextMeshProUGUI rangeStat;
    [SerializeField] private TextMeshProUGUI spreadStat;
    [SerializeField] private TextMeshProUGUI critStat;

    [SerializeField] private TextMeshProUGUI dpsDiffStatText;
    [SerializeField] private TextMeshProUGUI bulletDiffDmgStat;
    [SerializeField] private TextMeshProUGUI pelletsDiffPerShotStat;
    [SerializeField] private TextMeshProUGUI cooldownDiffStat;
    [SerializeField] private TextMeshProUGUI shotsPerClipDiffStat;
    [SerializeField] private TextMeshProUGUI maxAmmoDiffStat;
    [SerializeField] private TextMeshProUGUI reloadTimeDiffStat;
    [SerializeField] private TextMeshProUGUI rangeDiffStat;
    [SerializeField] private TextMeshProUGUI spreadDiffStat;
    [SerializeField] private TextMeshProUGUI critDiffStat;

    [SerializeField] private Material cleanTextMaterial;
    [SerializeField] private Material greenTextMaterial;
    [SerializeField] private Material redTextMaterial;

    private GunSO currentGunSODisplayed;

    private void Start() {
        WeaponChangeButton.OnAnyWeaponChangeButtonPressed += WeaponChangeButton_OnAnyWeaponChangeButtonPressed;
        WeaponReplaceButton.OnWeaponReplaceButtonPressed += WeaponReplaceButton_OnWeaponReplaceButtonPressed;
        WeaponReplaceButton.OnAnyWeaponReplaceButtonHovered += WeaponReplaceButton_OnAnyButtonHovered;
        WeaponReplaceButton.OnAnyWeaponReplaceButtonUnhovered += WeaponReplaceButton_OnAnyButtonUnhovered;

        DisplayGunStats(PlayerShoot.Instance.GetPrimaryGunSO());
        ShowDiffText(false);
    }

    private void WeaponReplaceButton_OnAnyButtonUnhovered(object sender, System.EventArgs e) {
        HideGunStatsComparison();
    }

    private void WeaponReplaceButton_OnAnyButtonHovered(object sender, System.EventArgs e) {
        WeaponReplaceButton replaceButton = (WeaponReplaceButton)sender;

        GunSO gunSOToDiplay = replaceButton.GetLinkedGunSO();

        if (gunSOToDiplay != null) {
            DisplayGunStatsComparison(gunSOToDiplay);
        }
    }

    private void WeaponReplaceButton_OnWeaponReplaceButtonPressed(object sender, System.EventArgs e) {
        WeaponReplaceButton replaceButton = (WeaponReplaceButton)sender;

        GunSO gunSOToDiplay = replaceButton.GetLinkedGunSO();

        if (gunSOToDiplay != null) {
            ShowDiffText(false);
            DisplayGunStats(replaceButton.GetLinkedGunSO());
        }
    }

    private void WeaponChangeButton_OnAnyWeaponChangeButtonPressed(object sender, System.EventArgs e) {
        WeaponChangeButton changeButton = (WeaponChangeButton)sender;

        GunSO gunSOToDiplay = changeButton.GetLinkedGunSO();

        if(gunSOToDiplay != null) {
            DisplayGunStats(changeButton.GetLinkedGunSO());
        }
    }

    private void DisplayGunStats(GunSO gunSO) {
        ResetTextMaterials();

        int bulletDmg = PlayerShoot.Instance.GetGun(gunSO).GetDamagePerBullet();
        bulletDmgStat.text = HandleStringTrim(bulletDmg.ToString());

        int pelletsPerShot = PlayerShoot.Instance.GetGun(gunSO).GetPelletsPerBullet();
        pelletsPerShotStat.text = HandleStringTrim(pelletsPerShot.ToString());

        if (pelletsPerShot == 1) {
            //pelletsPerShotGameObject.SetActive(false); 
        } else {
            //pelletsPerShotGameObject.SetActive(true);
        }

        float cooldown = PlayerShoot.Instance.GetGun(gunSO).GetCooldownTime();
        cooldownStat.text = HandleStringTrim(cooldown.ToString()) + "s";

        float dps = bulletDmg * pelletsPerShot * cooldown;
        dpsStatText.text = HandleStringTrim(dps.ToString());

        int shotsPerClip = PlayerShoot.Instance.GetGun(gunSO).GetShotsPerClip();
        shotsPerClipStat.text = HandleStringTrim(shotsPerClip.ToString());

        int maxAmmo = PlayerShoot.Instance.GetGun(gunSO).GetMaxAmmo();
        maxAmmoStat.text = HandleStringTrim(maxAmmo.ToString());

        float reloadTime = PlayerShoot.Instance.GetGun(gunSO).GetReloadTime();
        reloadTimeStat.text = HandleStringTrim(reloadTime.ToString()) + "s";

        float range = PlayerShoot.Instance.GetGun(gunSO).GetBulletLifetime() * gunSO.bulletSpeed;
        rangeStat.text = HandleStringTrim(range.ToString()) + "m";

        float spread = PlayerShoot.Instance.GetGun(gunSO).GetDefaultShootAngle();
        spreadStat.text = HandleStringTrim(spread.ToString()) + "\u00B0";

        float crit = PlayerShoot.Instance.GetGun(gunSO).GetCritChance();
        critStat.text = HandleStringTrim(crit.ToString()) + "%";

        currentGunSODisplayed = gunSO;
    }
    private void DisplayGunStatsComparison(GunSO gunSO) {
        ShowDiffText(true);

        int bulletDmg = PlayerShoot.Instance.GetGun(gunSO).GetDamagePerBullet();
        bulletDmgStat.text = HandleStringTrim(bulletDmg.ToString());
        int currentGunBulletDmg = PlayerShoot.Instance.GetGun(currentGunSODisplayed).GetDamagePerBullet();
        int dmgDiff = bulletDmg - currentGunBulletDmg;
        bulletDiffDmgStat.text = HandleStringTrim(dmgDiff.ToString());
        SetStatDiffColor(bulletDmgStat, bulletDiffDmgStat, dmgDiff, true);


        int pelletsPerShot = PlayerShoot.Instance.GetGun(gunSO).GetPelletsPerBullet();
        pelletsPerShotStat.text = HandleStringTrim(pelletsPerShot.ToString());
        int currentGunPelletsPerShot = PlayerShoot.Instance.GetGun(currentGunSODisplayed).GetPelletsPerBullet();
        int pelletsDiff = pelletsPerShot - currentGunPelletsPerShot;
        pelletsDiffPerShotStat.text = HandleStringTrim(pelletsDiff.ToString());

        if (pelletsPerShot == 1) {
            //pelletsPerShotGameObject.SetActive(false);
        }
        else {
            //pelletsPerShotGameObject.SetActive(true);
            SetStatDiffColor(pelletsPerShotStat, pelletsDiffPerShotStat, pelletsDiff, true);
        }

        float cooldown = PlayerShoot.Instance.GetGun(gunSO).GetCooldownTime();
        cooldownStat.text = HandleStringTrim(cooldown.ToString()) + "s";
        float currentGunCooldown = PlayerShoot.Instance.GetGun(currentGunSODisplayed).GetCooldownTime();
        float cooldownDiff = cooldown - currentGunCooldown;
        cooldownDiffStat.text = HandleStringTrim(cooldownDiff.ToString()) + "s";
        SetStatDiffColor(cooldownStat, cooldownDiffStat, cooldownDiff, false);


        float gunDPS = bulletDmg * pelletsPerShot * cooldown;
        dpsStatText.text = HandleStringTrim(gunDPS.ToString());
        float currentGunDps = currentGunBulletDmg * currentGunPelletsPerShot * currentGunCooldown;
        float dpsDiff = gunDPS - currentGunDps;
        dpsDiffStatText.text = HandleStringTrim(dpsDiff.ToString());
        SetStatDiffColor(dpsStatText, dpsDiffStatText, dpsDiff, true);


        int shotsPerClip = PlayerShoot.Instance.GetGun(gunSO).GetShotsPerClip();
        shotsPerClipStat.text = HandleStringTrim(shotsPerClip.ToString());
        int currentshotsPerClip = PlayerShoot.Instance.GetGun(currentGunSODisplayed).GetShotsPerClip();
        int shotsPerClipDiff = shotsPerClip - currentshotsPerClip;
        shotsPerClipDiffStat.text = HandleStringTrim(shotsPerClipDiff.ToString());
        SetStatDiffColor(shotsPerClipStat, shotsPerClipDiffStat, shotsPerClipDiff, true);


        int maxAmmo = PlayerShoot.Instance.GetGun(gunSO).GetMaxAmmo();
        maxAmmoStat.text = HandleStringTrim(maxAmmo.ToString());
        int currentmaxAmmo = PlayerShoot.Instance.GetGun(currentGunSODisplayed).GetMaxAmmo();
        int maxAmmoDiff = maxAmmo - currentmaxAmmo;
        maxAmmoDiffStat.text = HandleStringTrim(maxAmmoDiff.ToString());
        SetStatDiffColor(maxAmmoStat, maxAmmoDiffStat, maxAmmoDiff, true);


        float reloadTime = PlayerShoot.Instance.GetGun(gunSO).GetReloadTime();
        reloadTimeStat.text = HandleStringTrim(reloadTime.ToString()) + "s";
        float currentGunreloadTime = PlayerShoot.Instance.GetGun(currentGunSODisplayed).GetReloadTime();
        float reloadTimeDiff = reloadTime - currentGunreloadTime;
        reloadTimeDiffStat.text = HandleStringTrim(reloadTimeDiff.ToString()) + "s";
        SetStatDiffColor(reloadTimeStat, reloadTimeDiffStat, reloadTimeDiff, false);


        float range = PlayerShoot.Instance.GetGun(gunSO).GetBulletLifetime() * gunSO.bulletSpeed;
        rangeStat.text = HandleStringTrim(range.ToString()) + "m";
        float currentGunrange = PlayerShoot.Instance.GetGun(currentGunSODisplayed).GetBulletLifetime() * gunSO.bulletSpeed;
        float rangeDiff = range - currentGunrange;
        rangeDiffStat.text = HandleStringTrim(rangeDiff.ToString()) + "m";
        SetStatDiffColor(rangeStat, rangeDiffStat, rangeDiff, true);


        float spread = PlayerShoot.Instance.GetGun(gunSO).GetDefaultShootAngle();
        spreadStat.text = HandleStringTrim(spread.ToString()) + "\u00B0";
        float currentGunspread = PlayerShoot.Instance.GetGun(currentGunSODisplayed).GetDefaultShootAngle();
        float spreadDiff = spread - currentGunspread;
        spreadDiffStat.text = HandleStringTrim(spreadDiff.ToString()) + "\u00B0";
        SetStatDiffColor(spreadStat, spreadDiffStat, spreadDiff, false);


        float crit = PlayerShoot.Instance.GetGun(gunSO).GetCritChance();
        critStat.text = HandleStringTrim(crit.ToString()) + "%";
        float currencrit = PlayerShoot.Instance.GetGun(currentGunSODisplayed).GetCritChance();
        float critDiff = crit - currencrit;
        critDiffStat.text = HandleStringTrim(critDiff.ToString()) + "%";
        SetStatDiffColor(critStat, critDiffStat, critDiff, true);


    }

    private void HideGunStatsComparison() {
        ShowDiffText(false);
        DisplayGunStats(currentGunSODisplayed);
    }

    private void ShowDiffText(bool show) {
        dpsDiffStatText.gameObject.SetActive(show);
        bulletDiffDmgStat.gameObject.SetActive(show);
        pelletsDiffPerShotStat.gameObject.SetActive(show);
        cooldownDiffStat.gameObject.SetActive(show);
        shotsPerClipDiffStat.gameObject.SetActive(show);
        maxAmmoDiffStat.gameObject.SetActive(show);
        reloadTimeDiffStat.gameObject.SetActive(show);
        rangeDiffStat.gameObject.SetActive(show);
        spreadDiffStat.gameObject.SetActive(show);
        critDiffStat.gameObject.SetActive(show);
    }

    private void SetStatDiffColor(TextMeshProUGUI originalText, TextMeshProUGUI diffText, float diffValue, bool betterIfGreater) {
        if(diffValue > 0) {
            diffText.text = "+" + diffText.text;
            if(betterIfGreater) {
                originalText.fontMaterial = greenTextMaterial;
            } else {
                originalText.fontMaterial = redTextMaterial;
            }
        }

        if(diffValue < 0) {
            if (betterIfGreater) {
                originalText.fontMaterial = redTextMaterial;
            }
            else {
                originalText.fontMaterial = greenTextMaterial;
            }
        }

        if(diffValue == 0) {
            diffText.gameObject.SetActive(false);
            originalText.fontMaterial = cleanTextMaterial;
        }
    }

    private void ResetTextMaterials() {
        dpsStatText.fontMaterial = cleanTextMaterial;
        bulletDmgStat.fontMaterial = cleanTextMaterial;
        pelletsPerShotStat.fontMaterial = cleanTextMaterial;
        cooldownStat.fontMaterial = cleanTextMaterial;
        shotsPerClipStat.fontMaterial = cleanTextMaterial;
        maxAmmoStat.fontMaterial = cleanTextMaterial;
        reloadTimeStat.fontMaterial = cleanTextMaterial;
        rangeStat.fontMaterial = cleanTextMaterial;
        spreadStat.fontMaterial = cleanTextMaterial;
        critStat.fontMaterial = cleanTextMaterial;
    }

    private string HandleStringTrim(string stringToTrim) {

        if (stringToTrim.Contains(",")) {
            // Retirer les zéros inutiles après la virgule, puis la virgule si elle est seule
            stringToTrim = stringToTrim;
        }
        return stringToTrim;
    }

    private void OnDestroy() {
        WeaponChangeButton.OnAnyWeaponChangeButtonPressed -= WeaponChangeButton_OnAnyWeaponChangeButtonPressed;
        WeaponReplaceButton.OnWeaponReplaceButtonPressed -= WeaponReplaceButton_OnWeaponReplaceButtonPressed;
        WeaponReplaceButton.OnAnyWeaponReplaceButtonHovered -= WeaponReplaceButton_OnAnyButtonHovered;
        WeaponReplaceButton.OnAnyWeaponReplaceButtonUnhovered -= WeaponReplaceButton_OnAnyButtonUnhovered;
    }
}
