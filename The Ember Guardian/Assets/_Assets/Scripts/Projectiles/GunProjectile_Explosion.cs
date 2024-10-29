using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunProjectile_Explosion : MonoBehaviour
{
    [SerializeField] private float lifetime = .3f;

    private void Awake() {
        Invoke("DestroySelf", lifetime);
    }

    private void DestroySelf() {
        Destroy(gameObject);
    }
}
