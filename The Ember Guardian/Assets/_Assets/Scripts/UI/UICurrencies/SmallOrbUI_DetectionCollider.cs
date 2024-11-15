using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallOrbUI_DetectionCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("DestroySmallOrb")) {
            Destroy(gameObject);
        }
    }
}
