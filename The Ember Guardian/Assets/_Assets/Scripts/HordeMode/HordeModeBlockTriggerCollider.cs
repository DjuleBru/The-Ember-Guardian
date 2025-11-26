using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeModeBlockTriggerCollider : MonoBehaviour
{
    [SerializeField] private HordeModeBlock hordeModeBlock;



    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.GetComponent<Player>() != null) {
            hordeModeBlock.PlayerEnteredBlock();
        }

    }
}
