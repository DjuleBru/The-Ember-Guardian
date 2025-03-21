using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCollectibleCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        Collectible collectible = collision.GetComponent<Collectible>();
        Creature creature = collision.GetComponent<Creature>();
        if (collectible !=  null) {
            Destroy(collectible.gameObject);
        }

        if(creature != null) {
            creature.Die();
            Debug.Log("Creature collided with destroy creature collider");
        }
    }
}
