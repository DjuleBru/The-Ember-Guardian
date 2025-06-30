using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticProjectile_ContinuousDamage : StaticProjectile
{
    private List<Mob> mobsInTriggerArea = new List<Mob>();
    private float damageTickCooldown;
    private float damageTickTimer;

    protected override void Update() {
        damageTickTimer += Time.deltaTime;
        if(damageTickTimer >= damageTickCooldown) {
            damageTickTimer = 0;

            List<Mob> mobsInTriggerAreaCopy = new List<Mob>();
            foreach(Mob mob in mobsInTriggerArea) {
                mobsInTriggerAreaCopy.Add(mob);
            }
            StartCoroutine(DealTickDamageToAllMobsInTriggerArea(mobsInTriggerAreaCopy));
        }

        projectileLifetimer += Time.deltaTime;

        if (projectileLifetimer > projectileLifetime && projectileIsActive) {

            projectileIsActive = false;

            if (isPlayerStaticProjectile) {
                Destroy(gameObject);
            }
            else {
                ResetInProjectilePool();
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        Mob mobHit = collision.GetComponent<Mob>();
        if (mobHit == null) return;
        if (mobsInTriggerArea.Contains(mobHit)) return;
        mobsInTriggerArea.Add(mobHit);
        DealTickDamage(mobHit);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Mob mobHit = collision.GetComponent<Mob>();
        if (mobHit == null) return;
        if (!mobsInTriggerArea.Contains(mobHit)) return;
        mobsInTriggerArea.Remove(mobHit);
    }
    public void InitializeContinuous(float watchDir, Mob parentMob, int damagePerTick, float tickCooldown, bool isPlayerStaticProjectile = false, bool takeWatchDirInAccount = false) {
        this.parentMob = parentMob;
        this.damage = damagePerTick;
        damageTickCooldown = tickCooldown;
        this.isPlayerStaticProjectile = isPlayerStaticProjectile;

        if (takeWatchDirInAccount) {
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

    private IEnumerator DealTickDamageToAllMobsInTriggerArea(List<Mob> mobs) {
        foreach (Mob mob in mobs) {
            DealTickDamage(mob);
            yield return new WaitForSeconds(.05f);
        }
    } 

    private void DealTickDamage(Mob mob) {

        Creature creatureHit = mob.GetComponent<Creature>();
        InvokeOnStaticProjectileHitCreature();
        if (creatureHit != null) {
            creatureHit.TakeDamage(damage, transform);

            if (burningEffect) {
                creatureHit.ApplyBurning(burnDuration);
            }
            if (immobilizeEffect) {
                creatureHit.ApplyImmobilizeEffect(immobilizeDuration, creatureHit.transform.position);
            }
            if (poisonEffect) {
                creatureHit.ApplyPoisonEffect(poisonAmount);
            }
            if (knockbackEffect) {
                Vector2 knockBackDirNormalized = (creatureHit.transform.position - transform.position).normalized;
                knockBackDirNormalized.y = 0;
                creatureHit.TakeKnockback(knockbackAmount, knockBackDirNormalized);
            }
        }

    }
}
