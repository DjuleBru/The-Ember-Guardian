using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyUI_DetectionCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("DestroyCurrency")) {
            Destroy(gameObject);
        }
    }
}
