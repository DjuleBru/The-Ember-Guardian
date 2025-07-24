using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog_Front : MonoBehaviour
{
    private SpriteRenderer fogRenderer;
    private Material fogMaterial;

    [SerializeField] private float distanceFromFireToFullyShowFog = 20f;
    private float initialAlpha;
    private float fadeDuration = 3f; // Durée de fondu (en secondes)

    private float screenRadius = 10f;
    private bool fireLitAndOutsideFogDisappeared;
    private bool inCavern;

    private void Awake() {
        fogRenderer = GetComponent<SpriteRenderer>();
        fogMaterial = fogRenderer.material;
    }


    private void Start() {
        if (!LevelManager.Instance.GetLevelSO().hasFog) {

            gameObject.SetActive(false);
            return;
        }

        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;

        initialAlpha = LevelManager.Instance.GetLevelSO().fogFrontAlpha;
        fogRenderer.color = new Color(fogRenderer.color.r, fogRenderer.color.g, fogRenderer.color.b, initialAlpha);
    }

    private void Update() {
        if (initialAlpha == 0) return;
        if (inCavern) return;
        if(fireLitAndOutsideFogDisappeared) {
            AdjustFogAlphaBasedOnDistance();
        }
    }
    private void AdjustFogAlphaBasedOnDistance() {
        float distanceToFireOutsideLimit = 0;

        if (Player.Instance.transform.position.x > 0) {
            float positionToCalculateDistanceFrom = Fire.Instance.GetCurrentFireRadius() + screenRadius;
            distanceToFireOutsideLimit = Player.Instance.transform.position.x - positionToCalculateDistanceFrom;
        } else {
            float positionToCalculateDistanceFrom = -Fire.Instance.GetCurrentFireRadius() - screenRadius;
            distanceToFireOutsideLimit = positionToCalculateDistanceFrom - Player.Instance.transform.position.x;
        }

        float alpha = Mathf.Clamp01(distanceToFireOutsideLimit / distanceFromFireToFullyShowFog);
        fogRenderer.color = new Color(fogRenderer.color.r, fogRenderer.color.g, fogRenderer.color.b, alpha * initialAlpha);
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        StartCoroutine(FadeOutFog());
    }

    private IEnumerator FadeOutFog() {
        float elapsed = 0f;

        // Obtenir l'alpha initial
        float startFade = fogRenderer.material.GetFloat("_FadeAmount");

        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Appliquer une courbe Ease-In (quadratique)
            t = t * t;

            // Calculer la nouvelle valeur de fade
            float newFade = Mathf.Lerp(startFade, 1f, t);

            // Mettre à jour l'alpha
            fogRenderer.material.SetFloat("_FadeAmount", newFade);
            yield return null;
        }

        // Fin du fade-out
        fireLitAndOutsideFogDisappeared = true;

        fogRenderer.color = new Color(fogRenderer.color.r, fogRenderer.color.g, fogRenderer.color.b, 0f);
        fogRenderer.material.SetFloat("_FadeAmount", 0);
    }
    private IEnumerator FadeOutFogAlpha() {
        float elapsed = 0f;

        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Appliquer une courbe Ease-In (quadratique)
            t = t * t;

            // Calculer la nouvelle valeur de fade
            float newAlpha = Mathf.Lerp(1, 0, t);

            // Mettre à jour l'alpha
            fogRenderer.color = new Color(fogRenderer.color.r, fogRenderer.color.g, fogRenderer.color.b, newAlpha);
            yield return null;
        }

        // Fin du fade-out
        fireLitAndOutsideFogDisappeared = true;

        fogRenderer.color = new Color(fogRenderer.color.r, fogRenderer.color.g, fogRenderer.color.b, 0f);
        fogRenderer.material.SetFloat("_FadeAmount", 0);
    }

    private IEnumerator FadeInFogAlpha() {
        float elapsed = 0f;

        // Obtenir l'alpha initial
        float startFade = 0;

        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            // Appliquer une courbe Ease-In (quadratique)
            t = t * t;

            // Calculer la nouvelle valeur de fade
            float newFade = Mathf.Lerp(1, 0, t);

            // Mettre à jour l'alpha
            fogRenderer.color = new Color(fogRenderer.color.r, fogRenderer.color.g, fogRenderer.color.b, newFade);
            yield return null;
        }

        // Fin du fade-out
        fogRenderer.color = new Color(fogRenderer.color.r, fogRenderer.color.g, fogRenderer.color.b, 1);
    }

    public void SetInCavern(bool inCavern) {
        if (!LevelManager.Instance.GetLevelSO().hasFog) return;
        this.inCavern = inCavern;

        if(inCavern) {
            StartCoroutine(FadeOutFogAlpha());
        } else {
            StartCoroutine(FadeInFogAlpha());
        }
    }

}
