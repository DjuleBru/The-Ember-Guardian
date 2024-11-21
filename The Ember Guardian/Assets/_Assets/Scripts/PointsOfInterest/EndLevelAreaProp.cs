using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelAreaProp : MonoBehaviour
{
    [SerializeField] private Animator propMaterialAnimator;
    [SerializeField] private SpriteRenderer bodySpriteRenderer;

    public static event EventHandler OnAnyEndLevelAreaPropBurned;


    private void Awake() {
        propMaterialAnimator.enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D collision) {

        if(collision.gameObject.GetComponentInParent<Fire>() != null) {
            BurnProp();
        }
     }

    public void SetGlow(float glow) {
        bodySpriteRenderer.material.SetFloat("_Glow", glow);
    }

    public void BurnProp() {
        OnAnyEndLevelAreaPropBurned?.Invoke(this, EventArgs.Empty);
        propMaterialAnimator.enabled = true;
    }
}
