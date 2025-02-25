using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowProp : MonoBehaviour
{
    

    public enum AnimationType {
        Shine,
        Flicker,
        IntensityVariation,
    }
    [SerializeField] private bool isAnimatedGlow;
    [SerializeField] private AnimationType animationType;

    private Animator glowAnimator;

    private void Start() {
        glowAnimator = GetComponent<Animator>();
        if (isAnimatedGlow) {
            glowAnimator.enabled = true;
            glowAnimator.SetBool(animationType.ToString(), true);
        } else {
            glowAnimator.enabled = false;
        }
    }
}
