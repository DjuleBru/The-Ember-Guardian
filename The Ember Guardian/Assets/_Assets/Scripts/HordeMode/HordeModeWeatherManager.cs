using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeModeWeatherManager : MonoBehaviour
{
    [SerializeField] private Fog_Front fogFront;
    [SerializeField] private Fog_Back fogBack;

    private bool firstDayPassed;

    private bool fogActive;
    private float fogFrontAlpha;
    private float fogBackAlpha = 1f;

    private void Start() {
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if(fogActive) {
            EndFog();
        }

    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {

        float randomValue = UnityEngine.Random.value;

        if(randomValue > .5f) {

            float randomValue2 = UnityEngine.Random.value;
            if(randomValue2 < .33f) {
                StartFog();
                // FOG
            }

            if (randomValue2 >= .33f && randomValue2 < .66f) {
                // Rain
                RainManager.Instance.SetRandomRainLevel();
            }

            if (randomValue2 >= .66f) {
                // WIND
                if(firstDayPassed) {
                    WindManager.Instance.SetRandomWindStrength();
                } else {
                    WindManager.Instance.SetWindStrengthExternal(WindManager.WindStrength.soft);
                }

            }
        }

        if (!firstDayPassed) {
            firstDayPassed = true;
        }
    }

    private void StartFog() {
        fogFront.SetInitialAlpha(1f);
        fogBack.FadeIn(3f, fogBackAlpha);
        fogActive = true;

        if(!firstDayPassed) {
            fogFront.SetAlpha(1f);
        }
    }

    private void EndFog() {
        fogFront.SetInitialAlpha(0f);
        fogBack.FadeOut(3f);
        fogFront.FadeOutFog();
        fogActive = false;
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        fogFront.SetFireLit(true);

        if(fogActive) {
            fogFront.FadeOutFog();
        }

    }

}
