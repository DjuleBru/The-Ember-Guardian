using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fog_Back : MonoBehaviour
{
    private SpriteRenderer fogRenderer;
    private float initialAlpha;

    private void Awake() {
        fogRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        initialAlpha = LevelManager.Instance.GetLevelSO().fogBackAlpha;
        fogRenderer.color = new Color(fogRenderer.color.r, fogRenderer.color.g, fogRenderer.color.b, initialAlpha);
    }

    public void SetAlpha(float newAlpha) {
        Color c = fogRenderer.color;
        c.a = Mathf.Clamp01(newAlpha);
        fogRenderer.color = c;
    }

    public void FadeIn(float duration, float targetAlpha) {
        StartFade(targetAlpha, duration);
    }

    public void FadeOut(float duration) {
        StartFade(0f, duration);
    }

    private void StartFade(float targetAlpha, float duration) {
        StartCoroutine(FadeRoutine(targetAlpha, duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration) {
        float startAlpha = fogRenderer.color.a;
        float t = 0f;

        while (t < duration) {
            t += Time.deltaTime;
            float blend = t / duration;

            float a = Mathf.Lerp(startAlpha, targetAlpha, blend);
            SetAlpha(a);

            yield return null;
        }

        SetAlpha(targetAlpha);
    }

}
