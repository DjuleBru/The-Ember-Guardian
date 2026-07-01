using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGOAfterDelay : MonoBehaviour {
    private float lifetime = 1f;
    private float lifeTimer;
    private void Update() {
        lifeTimer += Time.deltaTime;
        if(lifeTimer > lifetime) {
            Destroy(gameObject);
        }
    }

}
