using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticProjectile : MonoBehaviour
{
    [SerializeField] private float projectileLifetime;

    private Mob parentMob;
    private Animator staticProjectileAnimator;
    private float projectileLifetimer;
    private bool hasHit;
    private bool projectileIsActive;
    private int damage;


    private void Update() {
        projectileLifetimer += Time.deltaTime;
        if (projectileLifetimer > projectileLifetime && projectileIsActive) {
            projectileIsActive = false;
            ResetInProjectilePool();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if ((hasHit)) return;

        if (collision.GetComponent<Player>() != null) {
            Player.Instance.TakeDamage(damage, transform);
            hasHit = true;
        }

        if (collision.GetComponent<Barricade>() != null) {
            collision.GetComponent<Barricade>().TakeDamage(1, transform);
            hasHit = true;
        }

        if (collision.GetComponent<Worker>() != null) {
            Worker worker = collision.GetComponent<Worker>();
            if(worker.GetRecruited()) {
                collision.GetComponent<Worker>().TakeDamage(1, transform, false);
                hasHit = true;
            }
        }

        // Hit Barricade
        Fire fire = collision.gameObject.GetComponent<Fire>();
        if (fire != null) {
            collision.GetComponent<Fire>().TakeDamage(1, transform, false);
            parentMob.Die();
            hasHit = true;
        }
    }

    public void Initialize(float watchDir, Mob parentMob, int damage) {
        this.parentMob = parentMob;
        this.damage = damage;
        if (watchDir < 0) {
            Vector3 localScale = Vector3.one;
            localScale.x = -1f;
            transform.localScale = localScale;
        }

        staticProjectileAnimator = GetComponent<Animator>();
        staticProjectileAnimator.Play(staticProjectileAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
        staticProjectileAnimator.Update(0); // Force une mise à jour immédiate
    }

    private void ResetInProjectilePool() {
        parentMob.GetComponent<MobAttack>().ResetStaticProjectileInObjectPool(this);
        gameObject.SetActive(false);
        hasHit = false;
        projectileIsActive = true;
    }
}
