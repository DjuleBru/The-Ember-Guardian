using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyCollectibleCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        Collectible collectible = collision.GetComponent<Collectible>();
        if (collectible !=  null) {
            Destroy(collectible.gameObject);
        }
    }
}
