using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{

    private Collider2D platformCollider2D;

    private void Awake() {
        platformCollider2D = GetComponent<Collider2D>();
    }

    public void DisablePlatformCollider() {
        StartCoroutine(DisablePlatformColliderCoroutine());
    }

    private IEnumerator DisablePlatformColliderCoroutine() {
        platformCollider2D.enabled = false;

        yield return new WaitForSeconds(.5f);

        platformCollider2D.enabled = true;
    }
}
