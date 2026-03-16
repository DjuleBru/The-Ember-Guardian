using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_BulletRevolver : GunProjectile_Bullet
{

    private Creature FindClosestCreature() {
        float searchRadius = 6f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            searchRadius,
            raycastMask
        );

        Creature closest = null;
        float closestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++) {

            Creature creature = hits[i].GetComponent<Creature>();

            if (creature == null) {
                continue;
            }

            if (creaturesHit.Contains(creature)) {
                continue;
            }

            float dist = Vector2.Distance(
                transform.position,
                creature.transform.position
            );

            if (dist < closestDist) {
                closestDist = dist;
                closest = creature;
            }
        }

        return closest;
    }


    protected override void PierceEnemy(Creature creatureHit) {
        base.PierceEnemy(creatureHit);

        if (penetrationIndex >= penetrationMaxAmount) {
            return;
        }

        Creature nextTarget = FindClosestCreature();

        if (nextTarget == null) {
            return;
        }

        Vector2 dir = (nextTarget.transform.position - transform.position).normalized;

        float speed = rb.velocity.magnitude;

        rb.velocity = dir * speed;
        lifetimeTimer += .075f;
    }
}
