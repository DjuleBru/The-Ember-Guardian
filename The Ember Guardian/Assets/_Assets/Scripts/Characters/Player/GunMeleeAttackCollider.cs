using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunMeleeAttackCollider : MonoBehaviour
{
    private bool meleeAttackHasHit;
    private int meleeAttackDamage = 5;

    public event EventHandler OnGunMeleeAttackHit;
    public static event EventHandler OnAnyGunMeleeAttackHit;

    private void Start() {
        PlayerMeleeAttack.Instance.OnMeleeAttackStarted += PlayerMeleeAttack_OnMeleeAttackStarted;
    }

    private void PlayerMeleeAttack_OnMeleeAttackStarted(object sender, System.EventArgs e) {
        meleeAttackHasHit = false;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (meleeAttackHasHit) return;

        Creature creatureHit = collision.GetComponent<Creature>(); 
        CreatureSpawnerContinuous spawnerHit = collision.GetComponent<CreatureSpawnerContinuous>();
        Collider2D colliderHit = collision.GetComponent<Collider2D>();

        if (colliderHit.CompareTag("CritHitZone")) {
            creatureHit = colliderHit.GetComponentInParent<Creature>();
        }

        if (creatureHit != null) {
            creatureHit.TakeDamage(meleeAttackDamage, Player.Instance.transform, false);

            // Calcule l'angle pour orienter l'explosion prefab et le magma shot
            Vector3 hitPosition = collision.ClosestPoint(transform.position);
            Vector3 hitDir = (creatureHit.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(hitDir.y, hitDir.x) * Mathf.Rad2Deg;

            creatureHit.InstantiateHitPS(angle, hitPosition.y, false, meleeAttackDamage, hitPosition.x);
            meleeAttackHasHit = true;

            if(PlayerSkills.Instance.GetMeleeAttackMagmaShot()) {
                StaticProjectile magmaShot = Instantiate(PlayerSkills.Instance.GetMagmaShotPrefab(), hitPosition, Quaternion.Euler(0, 0, angle)).GetComponent<StaticProjectile>();
                magmaShot.Initialize(PlayerAim.Instance.GetAimDirFloat(), null, PlayerSkills.Instance.GetMeleeAttackMagmaShotDamage(), true);
                magmaShot.InitializeCarriedStatusEffects(true, PlayerSkills.Instance.GetMeleeAttackMagmaShotBurnAmount());
            }

            OnGunMeleeAttackHit?.Invoke(this, EventArgs.Empty);
            OnAnyGunMeleeAttackHit?.Invoke(this, EventArgs.Empty);
            return;
        }

        if(spawnerHit != null) {
            spawnerHit.TakeDamage(meleeAttackDamage, Player.Instance.transform, false);
            OnGunMeleeAttackHit?.Invoke(this, EventArgs.Empty);
            OnAnyGunMeleeAttackHit?.Invoke(this, EventArgs.Empty);
        }
    }
}
