using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEnvironment : MonoBehaviour
{
    private SpriteRenderer[] parallaxBackgrounds;

    [SerializeField] private bool isCampParallax;

    private void Awake() {
        parallaxBackgrounds = GetComponentsInChildren<SpriteRenderer>();

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
