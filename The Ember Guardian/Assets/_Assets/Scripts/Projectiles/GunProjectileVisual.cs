using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectileVisual : MonoBehaviour
{
    protected GunProjectile gunProjectile;
    protected Animator animator;
    [SerializeField] protected int explodeVariantCount = 1;
    [SerializeField] protected GunProjectile_Bullet bullet;

    protected void Awake() {
        animator = GetComponent<Animator>();
        gunProjectile = GetComponentInParent<GunProjectile>();
        gunProjectile.OnProjectileExploded += GunProjectile_OnProjectileExploded;

        if(bullet != null) {
            bullet.OnProjectileFadedOut += Bullet_OnProjectileFadedOut;
        }
    }

    protected void Bullet_OnProjectileFadedOut(object sender, System.EventArgs e) {
        animator.SetTrigger("Explode");
    }

    protected void GunProjectile_OnProjectileExploded(object sender, System.EventArgs e) {
        int randomIndex = Random.Range(0, explodeVariantCount);
        animator.SetInteger("ExplodeIndex", randomIndex);
        animator.SetTrigger("Explode");
    }
}
