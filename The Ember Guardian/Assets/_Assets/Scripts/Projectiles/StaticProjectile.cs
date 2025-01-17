using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticProjectile : MonoBehaviour
{
    [SerializeField] private float projectileLifetime;

    private Mob parentMob;
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
            Player.Instance.TakeDamage(1, transform);
            hasHit = true;
        }

        if (collision.GetComponent<Barricade>() != null) {
            collision.GetComponent<Barricade>().TakeDamage(1, transform);
            hasHit = true;
        }

        if (collision.GetComponent<Worker>() != null) {
            collision.GetComponent<Worker>().TakeDamage(1, transform, false);
            hasHit = true;
        }

        // Hit Barricade
        Fire fire = collision.gameObject.GetComponent<Fire>();
        if (fire != null) {
            collision.GetComponent<Fire>().TakeDamage(1, transform, false);
            parentMob.Die();
            hasHit = true;
        }
    }

    public void Initialize(float watchDir, Mob parentMob) {
        this.parentMob = parentMob;
        if(watchDir < 0) {
            Vector3 localScale = Vector3.one;
            localScale.x = -1f;
            transform.localScale = localScale;
        }
    }
}
