using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticProjectile : MonoBehaviour
{
    [SerializeField] private float projectileLifetime;

    private float projectileLifetimer;
    private bool hasHit;

    private void Update() {
        projectileLifetimer += Time.deltaTime;
        if (projectileLifetimer > projectileLifetime) {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if ((hasHit)) return;

        if (collision.GetComponent<Player>() != null) {
            Player.Instance.TakeDamage(1, transform, false);
            hasHit = true;
        }
    }

    public void Initialize(float watchDir) {
        if(watchDir < 0) {
            Vector3 localScale = Vector3.one;
            localScale.x = -1f;
            transform.localScale = localScale;
        }
    }
}
