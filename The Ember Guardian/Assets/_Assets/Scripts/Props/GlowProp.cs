using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlowProp : MonoBehaviour
{
    

    public enum AnimationType {
        Shine,
        Flicker,
        IntensityVariation,
    }
    [SerializeField] private bool isAnimatedGlow;
    [SerializeField] private bool switchOffLightAtDay;
    [SerializeField] private AnimationType animationType;
    [SerializeField] private Light2D propLight;
    [SerializeField] private SpriteRenderer glowSpriteRenderer;

    private float initialGlow;
    private Animator glowAnimator;

    private void Start() {
        glowAnimator = GetComponent<Animator>();
        if (isAnimatedGlow) {
            glowAnimator.enabled = true;
            glowAnimator.SetBool(animationType.ToString(), true);

            // Décale la phase de l’animation de façon aléatoire
            float randomOffset = Random.Range(0f, 1f);
            glowAnimator.Play(0, -1, randomOffset);
        }
        else {
            if(glowAnimator != null) {
                glowAnimator.enabled = false;
            }

        }

        if(switchOffLightAtDay) {
            if(DayNightManager.Instance != null) {
                DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
                DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
            }

        }

        if(glowSpriteRenderer != null) {
            initialGlow = glowSpriteRenderer.material.GetFloat("_Glow");
        }

    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(SwitchOnLightWithDelay());
    }

    private void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(SwitchOffLightWithRandomDelay());
    }

    private IEnumerator SwitchOffLightWithRandomDelay() {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1f));
        if (propLight == null) yield break;

        if (propLight != null) {
            propLight.enabled = false;
        }

        if (glowAnimator != null) {
            glowAnimator.enabled = false;
        }

        glowSpriteRenderer.material.SetFloat("_Glow", 0);
        if (GetComponent<ElectricitySparkPS>() != null) {
            GetComponent<ElectricitySparkPS>().enabled = false;
        }
    }

    private IEnumerator SwitchOnLightWithDelay() {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 3f));


        if (propLight == null) yield break;

        if (propLight != null) {
            propLight.enabled = true;
        }

        if (glowAnimator != null) {
            glowAnimator.enabled = true;
        }

        glowSpriteRenderer.material.SetFloat("_Glow", initialGlow);

        if (GetComponent<ElectricitySparkPS>() != null) {
            GetComponent<ElectricitySparkPS>().enabled = true;
        }
    }
}
