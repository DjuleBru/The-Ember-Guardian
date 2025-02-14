using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassObject : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Material vegetationMaterial;

    private float initialWindSpeed = 8;
    private float initialBend = .01f;
    private float initialRadialBend = .005f;

    private float initialWaveAmount = 0f;
    private float initialWaveStrength = 0f;
    private float playerPassingWaveAmount = 0.8f;
    private float playerPassingWaveStrength = 1.2f;

    private float playerPassingDuration = .25f;
    private float playerPassingWindSpeed = 12f;
    private float playerPassingBend = .01f;
    private float playerPassingRadialBend = .01f;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        vegetationMaterial = spriteRenderer.material;

        float initialWindSpeedRandomized = UnityEngine.Random.Range(initialWindSpeed - .4f, initialWindSpeed + .4f);
        vegetationMaterial.SetFloat("_GrassSpeed", initialWindSpeedRandomized);

    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Player>() == null) return;
        StartCoroutine(PlayerPass());
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.GetComponent<Player>() == null) return;
    }

    private IEnumerator PlayerPass() {
       
        vegetationMaterial.SetFloat("_WaveAmount", playerPassingWaveAmount);
        vegetationMaterial.SetFloat("_WaveStrength", playerPassingWaveStrength);

        yield return new WaitForSeconds(playerPassingDuration);

        vegetationMaterial.SetFloat("_WaveAmount", initialWaveAmount);
        vegetationMaterial.SetFloat("_WaveStrength", initialWaveStrength);
    }


}
