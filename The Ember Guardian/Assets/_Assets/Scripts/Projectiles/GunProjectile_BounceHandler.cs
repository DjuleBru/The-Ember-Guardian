using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_BounceHandler : MonoBehaviour
{
    [SerializeField] private Collider2D physicsCollider;

    private bool bouncedOnCreature;
    private bool bouncedOnGround;
    public event EventHandler OnProjectileBouncedOnGround;
    public event EventHandler OnProjectileBouncedOnCreature;


    private void OnTriggerEnter2D(Collider2D collision) {
        Creature creatureHit = collision.GetComponent<Creature>();

        if (creatureHit != null && !bouncedOnCreature) {
            // Trouver l'index de la layer "Creatures"
            int creatureLayer = LayerMask.NameToLayer("Creatures");
            // Ajouter cette layer aux exclusions du Rigidbody2D
            physicsCollider.excludeLayers |= (1 << creatureLayer);
            bouncedOnCreature = true;

            if (bouncedOnGround) return;
            OnProjectileBouncedOnCreature?.Invoke(this, EventArgs.Empty);
        }

        // Détection du sol (Layer "Ground")
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) {
            OnProjectileBouncedOnGround?.Invoke(this, EventArgs.Empty);
            bouncedOnGround = true;
        }
    }
}
