using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobVisual : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer bodySpriteRenderer;
    public static int currentMaxSortingOrder;

    protected Mob mob;

    protected virtual void Awake() {
        mob = GetComponentInParent<Mob>();
        currentMaxSortingOrder++;
        bodySpriteRenderer.sortingOrder = currentMaxSortingOrder;
    }
}
