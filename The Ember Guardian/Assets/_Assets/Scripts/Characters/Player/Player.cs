using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{

    public static Player Instance;

    [SerializeField] private Transform projectileTarget;

    private Rigidbody2D rb;
    private bool canDropOrbOnTheFloor = true;

    private void Awake() {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetCanDropOrbOnTheFloor(bool canDrop) {
        canDropOrbOnTheFloor = canDrop;
    }

    public bool GetCanDropOrbOnTheFloor() {
        return canDropOrbOnTheFloor;
    }

    public void AddKnockBack(Vector2 knockbackDir) {
        rb.AddForce(knockbackDir, ForceMode2D.Impulse);
    }

    public void TakeDamage(int damage, Vector3 damageSourcePosition) {
        Debug.Log("player take damage "+ damage);
    }

    public void Die() {
    }

    public Transform GetProjectileTarget() {
        return projectileTarget;
    }

    public Transform GetMeleeAttackPosition() {
        return transform;
    }
}
