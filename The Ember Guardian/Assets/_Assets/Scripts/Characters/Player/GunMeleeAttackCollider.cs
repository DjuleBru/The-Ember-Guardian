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
        if (creatureHit != null) {
            creatureHit.TakeDamage(meleeAttackDamage, Player.Instance.transform, false);

            // Calcule l'angle pour orienter l'explosion prefab
            Vector3 hitPosition = collision.ClosestPoint(transform.position);
            Vector3 hitDir = (creatureHit.transform.position - transform.position).normalized;
            float angle = Mathf.Atan2(hitDir.y, hitDir.x) * Mathf.Rad2Deg;

            creatureHit.InstantiateHitPS(angle, hitPosition.y, false, meleeAttackDamage, hitPosition.x);
            meleeAttackHasHit = true;

            OnGunMeleeAttackHit?.Invoke(this, EventArgs.Empty);
            OnAnyGunMeleeAttackHit?.Invoke(this, EventArgs.Empty);
        }
    }
}
