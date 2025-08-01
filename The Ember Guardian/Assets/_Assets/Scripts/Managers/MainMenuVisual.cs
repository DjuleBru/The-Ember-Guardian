using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Water2D;

public class MainMenuVisual : MonoBehaviour
{
    [SerializeField] private bool showDebugEnvironment;
    [SerializeField] private LevelSO.LevelEnvironment debugEnvironment;
    [SerializeField] private bool isDemo;

    [SerializeField] private GameObject mainGameLogo;
    [SerializeField] private GameObject demoLogo;
    [SerializeField] private SpriteRenderer skySpriteRenderer;
    [SerializeField] private SpriteRenderer water2DSpriteRenderer;

    [SerializeField] private GameObject cityWaterGO;
    [SerializeField] private GameObject verdantGraveyardWaterGO;
    [SerializeField] private GameObject corruptedCityWaterGO;
    [SerializeField] private GameObject lumenHollowWaterGO;
    [SerializeField] private GameObject fracturedDistrictWaterGO;

    [SerializeField] private GameObject cityParallaxGO;
    [SerializeField] private GameObject verdantGraveyardParallaxGO;
    [SerializeField] private GameObject lostGreensParallaxGO;
    [SerializeField] private GameObject corruptedCityParallaxGO;
    [SerializeField] private GameObject lumenHollowParallaxGO;
    [SerializeField] private GameObject fracturedDistrictParallaxGO;

    [SerializeField] private GameObject cityGridGO;
    [SerializeField] private GameObject verdantGridGO;
    [SerializeField] private GameObject lostGridGO;
    [SerializeField] private GameObject corruptedGridGO;
    [SerializeField] private GameObject lumenHollowGridGO;
    [SerializeField] private GameObject facturedDistrictGridGO;

    [SerializeField] private GameObject cityPropsGO;
    [SerializeField] private GameObject verdantPropsGO;
    [SerializeField] private GameObject lostPropsGO;
    [SerializeField] private GameObject corruptedPropsGO;
    [SerializeField] private GameObject lumenHollowPropsGO;
    [SerializeField] private GameObject fracturedDistrictPropsGO;

    [SerializeField] private Light2D moonLight;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Transform moonTransform;

    [SerializeField] private float cityLightIntensity;
    [SerializeField] private float verdantGraveyardIntensity;
    [SerializeField] private float lostGreensIntensity;
    [SerializeField] private float corruptedCityIntensity;
    [SerializeField] private float lumenHollowIntensity;
    [SerializeField] private float fracturedDistrictIntensity;

    [SerializeField] private float cityMoonLightIntensity;
    [SerializeField] private float verdanyGraveyardMoonIntensity;
    [SerializeField] private float lostGreensMoonIntensity;
    [SerializeField] private float corruptedMoonCityIntensity;
    [SerializeField] private float lumenHollowMoonIntensity;
    [SerializeField] private float fracturedDistrictMoonIntensity;

    [SerializeField] private float cityMoonLightOuterRadius;
    [SerializeField] private float verdanyGraveyardOuterRadius;
    [SerializeField] private float lostGreensOuterRadius;
    [SerializeField] private float corruptedMoonOuterRadius;
    [SerializeField] private float lumenHollowMoonOuterRadius;
    [SerializeField] private float fracturedDistrictOuterRadius;

    [SerializeField] private Transform cityMoonPosition;
    [SerializeField] private Transform verdantGraveyardMoonPosition;
    [SerializeField] private Transform lostGreensMoonPosition;
    [SerializeField] private Transform corruptedCityMoonPosition;
    [SerializeField] private Transform lumenHollowMoonPosition;
    [SerializeField] private Transform fracturedDistrictMoonPosition;


    [SerializeField] private Color citySkyColor;
    [SerializeField] private Color verdantGraveyardSkyColor;
    [SerializeField] private Color lostGreensSkyColor;
    [SerializeField] private Color corruptedCitySkyColor;
    [SerializeField] private Color lumenHollowSkyColor;
    [SerializeField] private Color fracturedDistrictSkyColor;

    private LevelSO.LevelEnvironment levelEnvironment;

    private void Start() {
        // This is saved in LevelManager Start and HubManager Start
        levelEnvironment = ES3.Load("lastLevelEnvironment", LevelSO.LevelEnvironment.TheVerdantGraveyard);
        Debug.Log(levelEnvironment);

        if(isDemo) {
            mainGameLogo.SetActive(false);
            demoLogo.SetActive(true);
        } else {
            mainGameLogo.SetActive(true);
            demoLogo.SetActive(false);
        }

        if(showDebugEnvironment) {
            levelEnvironment = debugEnvironment;
        }

        DisableAllVisuals();

        switch (levelEnvironment) {
            case LevelSO.LevelEnvironment.City:
                cityParallaxGO.SetActive(true);
                cityPropsGO.SetActive(true);
                cityGridGO.SetActive(true);

                moonTransform.position = cityMoonPosition.position;
                skySpriteRenderer.color = citySkyColor;
                globalLight.intensity = cityLightIntensity;
                moonLight.intensity = cityMoonLightIntensity;
                moonLight.pointLightOuterRadius = cityMoonLightOuterRadius;

                cityWaterGO.SetActive(true);
                break;
            case LevelSO.LevelEnvironment.TheVerdantGraveyard:
                verdantGraveyardParallaxGO.SetActive(true);
                verdantPropsGO.SetActive(true);
                verdantGridGO.SetActive(true);

                moonTransform.position = verdantGraveyardMoonPosition.position;
                skySpriteRenderer.color = verdantGraveyardSkyColor;
                globalLight.intensity = verdantGraveyardIntensity;
                moonLight.intensity = verdanyGraveyardMoonIntensity;
                moonLight.pointLightOuterRadius = verdanyGraveyardOuterRadius;

                verdantGraveyardWaterGO.SetActive(true);
                break;

            case LevelSO.LevelEnvironment.TheLostGreens:
                lostGreensParallaxGO.SetActive(true);
                lostPropsGO.SetActive(true);
                lostGridGO.SetActive(true);

                moonTransform.position = lostGreensMoonPosition.position;
                skySpriteRenderer.color = lostGreensSkyColor;
                globalLight.intensity = lostGreensIntensity;
                moonLight.intensity = lostGreensMoonIntensity;
                moonLight.pointLightOuterRadius = lostGreensOuterRadius;

                verdantGraveyardWaterGO.SetActive(true);
                break;

            case LevelSO.LevelEnvironment.CorruptedCity:
                cityParallaxGO.SetActive(true);
                corruptedPropsGO.SetActive(true);
                corruptedGridGO.SetActive(true);

                moonTransform.position = corruptedCityMoonPosition.position;
                skySpriteRenderer.color = corruptedCitySkyColor;
                globalLight.intensity = corruptedCityIntensity;
                moonLight.intensity = corruptedMoonCityIntensity;
                moonLight.pointLightOuterRadius = corruptedMoonOuterRadius;

                break;

            case LevelSO.LevelEnvironment.TheLumenHollow:
                lumenHollowParallaxGO.SetActive(true);
                lumenHollowPropsGO.SetActive(true);
                lumenHollowGridGO.SetActive(true);

                moonTransform.position = lumenHollowMoonPosition.position;
                skySpriteRenderer.color = lumenHollowSkyColor;
                globalLight.intensity = lumenHollowIntensity;
                moonLight.intensity = lumenHollowMoonIntensity;
                moonLight.pointLightOuterRadius = lumenHollowMoonOuterRadius;

                break;
        }
    }


    private void DisableAllVisuals() {
        cityWaterGO.SetActive(false);
        verdantGraveyardWaterGO.SetActive(false);

        cityParallaxGO.SetActive(false);
        verdantGraveyardParallaxGO.SetActive(false);
        lostGreensParallaxGO.SetActive(false);

        cityPropsGO.SetActive(false);
        verdantPropsGO.SetActive(false);
        lostPropsGO.SetActive(false);
        corruptedPropsGO.SetActive(false);

        cityGridGO.SetActive(false);
        verdantGridGO.SetActive(false);
        lostGridGO.SetActive(false);
        corruptedGridGO.SetActive(false);

    }
}
