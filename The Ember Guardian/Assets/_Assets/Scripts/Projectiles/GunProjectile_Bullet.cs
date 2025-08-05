using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_Bullet : GunProjectile
{
    [SerializeField] private int penetrationMaxAmount = 1;
    private int penetrationIndex;

    protected override void OnTriggerEnter2D(Collider2D collision) {
        Creature creatureHit = collision.GetComponent<Creature>();

        if (creatureHit != null) {
            if(penetrationIndex >= penetrationMaxAmount) {
                rb.velocity = Vector3.zero;
                return;
            }

            penetrationIndex++;
            DamageCreatureHit(creatureHit, collision);
        }

        // Détection du sol (Layer "Ground")
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            if (projectileExploded) return;

            if (explodeOnContact) {
                Explode();
            }
        }
    }
}
