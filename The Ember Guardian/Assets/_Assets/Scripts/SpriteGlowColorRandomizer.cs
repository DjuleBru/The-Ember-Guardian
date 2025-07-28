using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteGlowColorRandomizer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Gradient colorGradient;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Color color = colorGradient.Evaluate(UnityEngine.Random.value);
        spriteRenderer.material.SetColor("_GlowColor", color);
    }
}
