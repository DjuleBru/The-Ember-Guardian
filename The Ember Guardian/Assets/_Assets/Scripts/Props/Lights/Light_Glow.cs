using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Light_Glow : MonoBehaviour
{
    [SerializeField] private bool animateGlow;
    [SerializeField] private float glowAttempTime = 2f;
    [SerializeField] private float glowProbability = .5f;
    private float glowAttemptTimer;

    private Animator lightAnimator;

    private void Awake() {
        lightAnimator = GetComponent<Animator>();

        if (!animateGlow) {
            lightAnimator.enabled = false;
        }
    }

    private void Update() {
        glowAttemptTimer += Time.deltaTime;
        if(glowAttemptTimer > glowAttempTime ) {
            glowAttemptTimer = 0;

            float randomNumber = UnityEngine.Random.value;

            if(randomNumber < glowProbability) {
                lightAnimator.SetTrigger("Glow");
            }
        }
    }

}
