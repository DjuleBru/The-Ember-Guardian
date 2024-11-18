using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbProcessorOrbCollider : MonoBehaviour
{
    public event EventHandler OnOrbFellInOrbProcessor;

    private void OnTriggerEnter2D(Collider2D collision) {
        Collectible collectibleCollided = collision.GetComponent<Collectible>();

        if (collectibleCollided != null) {
            if (collectibleCollided.GetCurrencyType() == PlayerCurrencies.CurrencyType.smallBlueOrb) {
                if (collectibleCollided.GetDroppedByPlayer()) return;
                if (collectibleCollided.GetMovingForPayment()) return;

                OnOrbFellInOrbProcessor?.Invoke(this, EventArgs.Empty);
                Destroy(collectibleCollided.gameObject);
            };
        }
    }
}
