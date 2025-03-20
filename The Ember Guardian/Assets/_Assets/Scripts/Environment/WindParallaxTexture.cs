using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindParallaxTexture : MonoBehaviour
{

    [SerializeField] private SpriteRenderer windTextureSprite;
    private Material windTextureMaterial; 
    private float fadeDuration = 2f; // Durée totale du fade-in

    private void Start() {
        windTextureMaterial = windTextureSprite.material;

        WindManager.Instance.OnWindStrengthChanged += WindManager_OnWindStrengthChanged;
        SetTextureVariables();
    }

    private void WindManager_OnWindStrengthChanged(object sender, System.EventArgs e) {
        SetTextureVariables();
    }

    private void SetTextureVariables() {

        WindManager.WindStrength currentWindStrength = WindManager.Instance.GetWindStrength();
        float windStrength = GetWindStrengthForWindTexture(currentWindStrength) * -WindManager.Instance.GetWindDir();

        windTextureMaterial.SetFloat("_TextureScrollXSpeed", windStrength);


        float targetAlpha = GetAlphaForWindTexture(currentWindStrength);

        // Démarre une nouvelle coroutine pour le fade-in
        StartCoroutine(FadeAlpha(targetAlpha));
    }

    private IEnumerator FadeAlpha(float targetAlpha) {
        Color textureColor = Color.white;
        float startAlpha = windTextureMaterial.GetColor("_Color").a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration) {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);

            textureColor.a = newAlpha;
            windTextureMaterial.SetColor("_Color", textureColor);

            yield return null; // Attendre la prochaine frame
        }

        // S'assure que l'alpha final est bien atteint
        Color finalColor = windTextureMaterial.GetColor("_Color");
        finalColor.a = targetAlpha;
        windTextureMaterial.SetColor("_Color", finalColor);
    }


    public float GetWindStrengthForWindTexture(WindManager.WindStrength windStrength) {

        if (windStrength == WindManager.WindStrength.soft) {
            return .1f;
        }
        if (windStrength == WindManager.WindStrength.medium) {
            return .25f;
        }
        if (windStrength == WindManager.WindStrength.strong) {
            return .4f;
        }
        if (windStrength == WindManager.WindStrength.extreme) {
            return .7f;
        }

        return 0;
    }

    public float GetAlphaForWindTexture(WindManager.WindStrength windStrength) {

        if (windStrength == WindManager.WindStrength.soft) {
            return .03f;
        }
        if (windStrength == WindManager.WindStrength.medium) {
            return .03f;
        }
        if (windStrength == WindManager.WindStrength.strong) {
            return .04f;
        }
        if (windStrength == WindManager.WindStrength.extreme) {
            return .05f;
        }

        return 0;
    }
}
