using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : Structure, IDamageable {

    [SerializeField] private Transform projectileTarget;
    [SerializeField] private Transform meleeAttackPosition;

    public void Die() {

    }

    public Transform GetProjectileTarget() {
        return projectileTarget;
    }

    public void TakeDamage(int damage, Vector3 damageSourcePosition) {
        Debug.Log("Barricade take damage " + damage);
    }

    public Transform GetMeleeAttackPosition() {
        return meleeAttackPosition;
    }
}
