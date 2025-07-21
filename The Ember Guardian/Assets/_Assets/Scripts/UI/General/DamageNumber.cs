using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TextMeshPro damageNumberText;
    [SerializeField] private Color weakSpotHitColor;
    [SerializeField] private Color critHitColor;
    [SerializeField] private Color critHitInWeakSpotColor;
    [SerializeField] private Color surgeWindowBuffedColor;

    private float timeBeforeFade = 0.3f;
    private float fadeDuration = 0.2f;

    private float minDamageNumberTextSize = 3f;
    private float maxDamageNumberTextSize = 6f;
    private int damageNumberForMinSize = 5;
    private int damageNumberForMaxSize = 50;

    private IEnumerator FadeOut() {
        Color originalColor = damageNumberText.color;

        // Attente avant de commencer le fade
        yield return new WaitForSeconds(timeBeforeFade);

        float elapsed = 0f;
        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            damageNumberText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // On détruit l'objet une fois le fade terminé
        DamageNumberPool.Instance.ReturnToPool(this);
    }

    public void Initialize(int damage, bool weakSpotHit, bool critHit) {
        damageNumberText.color = Color.white;
        damageNumberText.text = damage.ToString();

        float t = Mathf.InverseLerp(damageNumberForMinSize, damageNumberForMaxSize, damage);
        float fontSize = Mathf.Lerp(minDamageNumberTextSize, maxDamageNumberTextSize, t);
        damageNumberText.fontSize = fontSize;

        Vector3 randomForceApplied = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(3, 7), 0);
        GetComponent<Rigidbody2D>().AddForce(randomForceApplied, ForceMode2D.Impulse);

        bool surgeWindowBuffed = PlayerShoot.Instance.GetHeldGun().GetDamageSurgeBuffedLastBullet();
        if (surgeWindowBuffed) {
            damageNumberText.color = surgeWindowBuffedColor;
        } else {

            if (weakSpotHit && critHit) {

                damageNumberText.color = critHitInWeakSpotColor;

            }
            else {

                if (critHit) {
                    damageNumberText.color = critHitColor;
                }

                if (weakSpotHit) {
                    damageNumberText.color = weakSpotHitColor;
                }
            }
        }
        StartCoroutine(FadeOut());
    }

}
