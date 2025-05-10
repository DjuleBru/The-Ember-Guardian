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
    private bool isPlayerStaticProjectile;
    private bool projectileIsActive;
    private int damage;

    private bool burningEffect;
    private int burnDuration;
    private bool immobilizeEffect;
    private float immobilizeDuration;

    public event EventHandler OnStaticProjectileHitCreature;
    private void Update() {
        projectileLifetimer += Time.deltaTime;

        if (projectileLifetimer > projectileLifetime && projectileIsActive) {

            projectileIsActive = false;

            if (isPlayerStaticProjectile) {
                Destroy(gameObject);
            } else {
                ResetInProjectilePool();
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if ((hasHit)) return;

        if(isPlayerStaticProjectile) {
            Creature creatureHit = collision.GetComponent<Creature>();
            if (creatureHit != null) {
                creatureHit.TakeDamage(damage, transform);

                if(burningEffect) {
                    creatureHit.ApplyBurning(burnDuration);
                }
                if (immobilizeEffect) {
                    creatureHit.ApplyImmobilizeEffect(immobilizeDuration, creatureHit.transform.position);
                }
                OnStaticProjectileHitCreature?.Invoke(this, EventArgs.Empty);
            }

        } else {

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
                if (worker.GetRecruited()) {
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

    }

    public void Initialize(float watchDir, Mob parentMob, int damage, bool isPlayerStaticProjectile = false, bool takeWatchDirInAccount = false) {
        this.parentMob = parentMob;
        this.damage = damage;
        this.isPlayerStaticProjectile = isPlayerStaticProjectile;

        if(takeWatchDirInAccount) {
            if (watchDir < 0) {
                Vector3 localScale = Vector3.one;
                localScale.x = -1f;
                transform.localScale = localScale;
            }
        }

        staticProjectileAnimator = GetComponent<Animator>();
        staticProjectileAnimator.Play(staticProjectileAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
        staticProjectileAnimator.Update(0); // Force une mise à jour immédiate

        projectileIsActive = true;
    }

    public void InitializeBurning(int burnDuration = 5) {
        burningEffect = true;
        this.burnDuration = burnDuration;
    }
    public void InitializeImmobilize(float immobilizeDuration = 5) {
        Debug.Log("immobilizeDuration " + immobilizeDuration);
        immobilizeEffect = true;
        this.immobilizeDuration = immobilizeDuration;
    }
    private void ResetInProjectilePool() {
        parentMob.GetComponent<MobAttack>().ResetStaticProjectileInObjectPool(this);
        gameObject.SetActive(false);
        hasHit = false;
        projectileIsActive = true;
        projectileLifetimer = 0;
    }
}
