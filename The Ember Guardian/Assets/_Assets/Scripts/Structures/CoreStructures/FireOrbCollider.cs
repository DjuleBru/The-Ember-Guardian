using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireOrbCollider : MonoBehaviour
{
    public event EventHandler OnOrbFellInFire;

    private void OnTriggerEnter2D(Collider2D collision) {
        Collectible collectibleCollided = collision.GetComponent<Collectible>();

        if(collectibleCollided != null) {
            if(collectibleCollided.GetCurrencyType() == PlayerCurrencies.CurrencyType.bigBlueOrb) {
                if (collectibleCollided.GetDroppedByPlayer()) return;
                if (collectibleCollided.GetPaying()) return;

                OnOrbFellInFire?.Invoke(this, EventArgs.Empty);
                Destroy(collectibleCollided.gameObject);
            };
        }
    }
}
