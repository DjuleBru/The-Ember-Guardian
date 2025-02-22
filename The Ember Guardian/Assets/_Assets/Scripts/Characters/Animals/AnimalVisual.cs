using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalVisual : MobVisual
{

    [SerializeField] protected SpriteRenderer glowSpriteRenderer;

    protected override void Awake() {
        base.Awake();
        glowSpriteRenderer.sortingOrder = bodySpriteRenderer.sortingOrder+1;
    }
}
