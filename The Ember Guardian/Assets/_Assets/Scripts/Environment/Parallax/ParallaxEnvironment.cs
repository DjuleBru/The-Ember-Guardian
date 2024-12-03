using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEnvironment : MonoBehaviour
{
    private SpriteRenderer[] parallaxBackgrounds;

    [SerializeField] private bool isCampParallax;
    [SerializeField] private bool customTransparency;

    private void Awake() {
        parallaxBackgrounds = GetComponentsInChildren<SpriteRenderer>();

        if (customTransparency) return;
        if(isCampParallax) {
            SetParallaxTransparency(1f);
        } else {
            SetParallaxTransparency(0f);
        }
    }

    private void Start() {
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
    }

    private void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        if(isCampParallax) {
            SetParallaxTransparency(1f);
        } else {
            SetParallaxTransparency(0f);
        }
    }

    public void SetParallaxTransparency(float transparency) {

        if(transparency < .05f) {
            transparency = 0;
        }
        if (transparency > .95f) {
            transparency = 1;
        }

        foreach (SpriteRenderer spriteRenderer in parallaxBackgrounds) {
            Color color = spriteRenderer.color;
            color.a = transparency;

            spriteRenderer.color = color;
        }
    }
}
