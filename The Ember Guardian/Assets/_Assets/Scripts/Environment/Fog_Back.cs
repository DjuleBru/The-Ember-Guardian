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

}
