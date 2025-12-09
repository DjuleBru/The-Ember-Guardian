using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeModeEnvironmentManager : MonoBehaviour
{
    public static HordeModeEnvironmentManager Instance;

    [SerializeField] private GameObject verdantGraveyardParallax;
    [SerializeField] private GameObject corruptedCityParallax;
    [SerializeField] private GameObject lumenHollowParallax;
    [SerializeField] private GameObject fracturedDistrictParallax;

    [SerializeField] private GameObject verdantGraveyardWaterGO;
    [SerializeField] private GameObject corruptedCityWaterGO;
    [SerializeField] private GameObject lumenHollowWaterGO;
    [SerializeField] private GameObject fracturedDistrictWaterGO;

    [SerializeField] private Portal enterPortal_VG;
    [SerializeField] private Portal enterPortal_CC;
    [SerializeField] private Portal enterPortal_LH;
    [SerializeField] private Portal enterPortal_FD;
    private Portal environmentPortal;


    [SerializeField] private Color dawnLightColor_VG;
    [SerializeField] private Color dawnSkyColor_VG;
    [SerializeField] private float dawnLightIntensity_VG;

    [SerializeField] private Color dayLightColor_VG;
    [SerializeField] private Color daySkyColor_VG;
    [SerializeField] private float dayLightIntensity_VG;

    [SerializeField] private Color duskLightColor_VG;
    [SerializeField] private Color duskSkyColor_VG;
    [SerializeField] private float duskLightIntensity_VG;

    [SerializeField] private Color nightLightColor_VG;
    [SerializeField] private Color nightSkyColor_VG;
    [SerializeField] private float nightLightIntensity_VG;

    [SerializeField] private Color dawnLightColor_CC;
    [SerializeField] private Color dawnSkyColor_CC;
    [SerializeField] private float dawnLightIntensity_CC;

    [SerializeField] private Color dayLightColor_CC;
    [SerializeField] private Color daySkyColor_CC;
    [SerializeField] private float dayLightIntensity_CC;

    [SerializeField] private Color duskLightColor_CC;
    [SerializeField] private Color duskSkyColor_CC;
    [SerializeField] private float duskLightIntensity_CC;

    [SerializeField] private Color nightLightColor_CC;
    [SerializeField] private Color nightSkyColor_CC;
    [SerializeField] private float nightLightIntensity_CC;

    [SerializeField] private Color dawnLightColor_LH;
    [SerializeField] private Color dawnSkyColor_LH;
    [SerializeField] private float dawnLightIntensity_LH;

    [SerializeField] private Color dayLightColor_LH;
    [SerializeField] private Color daySkyColor_LH;
    [SerializeField] private float dayLightIntensity_LH;

    [SerializeField] private Color duskLightColor_LH;
    [SerializeField] private Color duskSkyColor_LH;
    [SerializeField] private float duskLightIntensity_LH;

    [SerializeField] private Color nightLightColor_LH;
    [SerializeField] private Color nightSkyColor_LH;
    [SerializeField] private float nightLightIntensity_LH;

    [SerializeField] private Color dawnLightColor_FD;
    [SerializeField] private Color dawnSkyColor_FD;
    [SerializeField] private float dawnLightIntensity_FD;

    [SerializeField] private Color dayLightColor_FD;
    [SerializeField] private Color daySkyColor_FD;
    [SerializeField] private float dayLightIntensity_FD;

    [SerializeField] private Color duskLightColor_FD;
    [SerializeField] private Color duskSkyColor_FD;
    [SerializeField] private float duskLightIntensity_FD;

    [SerializeField] private Color nightLightColor_FD;
    [SerializeField] private Color nightSkyColor_FD;
    [SerializeField] private float nightLightIntensity_FD;

    private void Awake() {
        Instance = this;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            SetEnvironmentParallaxAndWater();
            SetEnvironmentLights();
            SetEnvironmentPortals();
        }
    }

   
    private void SetEnvironmentPortals() {

        LevelSO.LevelEnvironment selectedEnv = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();

        if (selectedEnv == LevelSO.LevelEnvironment.TheVerdantGraveyard) {
            environmentPortal = enterPortal_VG;
        }
        if (selectedEnv == LevelSO.LevelEnvironment.CorruptedCity) {
            environmentPortal = enterPortal_CC;
        }
        if (selectedEnv == LevelSO.LevelEnvironment.TheLumenHollow) {
            environmentPortal = enterPortal_LH;
        }
        if (selectedEnv == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            environmentPortal = enterPortal_FD;
        }
    }

    private void SetEnvironmentParallaxAndWater() {
        verdantGraveyardParallax.gameObject.SetActive(false);
        corruptedCityParallax.gameObject.SetActive(false);
        lumenHollowParallax.gameObject.SetActive(false);
        fracturedDistrictParallax.gameObject.SetActive(false);


        verdantGraveyardWaterGO.gameObject.SetActive(false);
        corruptedCityWaterGO.gameObject.SetActive(false);
        lumenHollowWaterGO.gameObject.SetActive(false);
        fracturedDistrictWaterGO.gameObject.SetActive(false);

        LevelSO.LevelEnvironment selectedEnv = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();

        if (selectedEnv == LevelSO.LevelEnvironment.TheVerdantGraveyard) {
            verdantGraveyardParallax.gameObject.SetActive(true);
            verdantGraveyardWaterGO.gameObject.SetActive(true);
        }
        if (selectedEnv == LevelSO.LevelEnvironment.CorruptedCity) {
            corruptedCityParallax.gameObject.SetActive(true);
            corruptedCityWaterGO.gameObject.SetActive(true);
        }
        if (selectedEnv == LevelSO.LevelEnvironment.TheLumenHollow) {
            lumenHollowParallax.gameObject.SetActive(true);
            lumenHollowWaterGO.gameObject.SetActive(true);
        }
        if (selectedEnv == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            fracturedDistrictParallax.gameObject.SetActive(true);
            fracturedDistrictWaterGO.gameObject.SetActive(true);
        }
    }

    private void SetEnvironmentLights() {
        Color dawnSkyColor = dawnSkyColor_VG;
        Color dawnLightColor = dawnLightColor_VG;
        float dawnLightIntensity = dawnLightIntensity_VG;

        Color daySkyColor = daySkyColor_VG;
        Color dayLightColor = dayLightColor_VG;
        float dayLightIntensity = dayLightIntensity_VG;

        Color duskSkyColor = duskSkyColor_VG;
        Color duskLightColor = duskLightColor_VG;
        float duskLightIntensity = duskLightIntensity_VG;

        Color nightSkyColor = nightSkyColor_VG;
        Color nightLightColor = nightLightColor_VG;
        float nightLightIntensity = nightLightIntensity_VG;

        LevelSO.LevelEnvironment selectedEnv = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();

        if (selectedEnv == LevelSO.LevelEnvironment.CorruptedCity) {
            dawnSkyColor = dawnSkyColor_CC;
            dawnLightColor = dawnLightColor_CC;
            dawnLightIntensity = dawnLightIntensity_CC;

            daySkyColor = daySkyColor_CC;
            dayLightColor = dayLightColor_CC;
            dayLightIntensity = dayLightIntensity_CC;

            duskSkyColor = duskSkyColor_CC;
            duskLightColor = duskLightColor_CC;
            duskLightIntensity = duskLightIntensity_CC;

            nightSkyColor = nightSkyColor_CC;
            nightLightColor = nightLightColor_CC;
            nightLightIntensity = nightLightIntensity_CC;
        }

        if (selectedEnv == LevelSO.LevelEnvironment.TheLumenHollow) {
            dawnSkyColor = dawnSkyColor_LH;
            dawnLightColor = dawnLightColor_LH;
            dawnLightIntensity = dawnLightIntensity_LH;

            daySkyColor = daySkyColor_LH;
            dayLightColor = dayLightColor_LH;
            dayLightIntensity = dayLightIntensity_LH;

            duskSkyColor = duskSkyColor_LH;
            duskLightColor = duskLightColor_LH;
            duskLightIntensity = duskLightIntensity_LH;

            nightSkyColor = nightSkyColor_LH;
            nightLightColor = nightLightColor_LH;
            nightLightIntensity = nightLightIntensity_LH;
        }

        if (selectedEnv == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            dawnSkyColor = dawnSkyColor_FD;
            dawnLightColor = dawnLightColor_FD;
            dawnLightIntensity = dawnLightIntensity_FD;

            daySkyColor = daySkyColor_FD;
            dayLightColor = dayLightColor_FD;
            dayLightIntensity = dayLightIntensity_FD;

            duskSkyColor = duskSkyColor_FD;
            duskLightColor = duskLightColor_FD;
            duskLightIntensity = duskLightIntensity_FD;

            nightSkyColor = nightSkyColor_FD;
            nightLightColor = nightLightColor_FD;
            nightLightIntensity = nightLightIntensity_FD;
        }

        DayNightVisualsManager.Instance.SetDawnLightColor(dawnLightColor);
        DayNightVisualsManager.Instance.SetDawnSkyColor(dawnSkyColor);
        DayNightVisualsManager.Instance.SetDawnLightIntensity(dawnLightIntensity);

        DayNightVisualsManager.Instance.SetDayLightColor(dayLightColor);
        DayNightVisualsManager.Instance.SetDaySkyColor(daySkyColor);
        DayNightVisualsManager.Instance.SetDayLightIntensity(dayLightIntensity);

        DayNightVisualsManager.Instance.SetDuskLightColor(duskLightColor);
        DayNightVisualsManager.Instance.SetDuskSkyColor(duskSkyColor);
        DayNightVisualsManager.Instance.SetDuskLightIntensity(duskLightIntensity);

        DayNightVisualsManager.Instance.SetNightLightColor(nightLightColor);
        DayNightVisualsManager.Instance.SetNightSkyColor(nightSkyColor);
        DayNightVisualsManager.Instance.SetNightLightIntensity(nightLightIntensity);
    }

    public bool GetIsEnvironmentPortal(Portal portal) {
        return portal == environmentPortal;
    }
}
