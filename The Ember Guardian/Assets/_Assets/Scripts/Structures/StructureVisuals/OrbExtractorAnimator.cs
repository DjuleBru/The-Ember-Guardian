using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbExtractorAnimator : MonoBehaviour {

    [SerializeField] private OrbExtractor orbExtractor;
    [SerializeField] private ParticleSystem dirtPS;
    [SerializeField] private ParticleSystem sparksPS;
    private Animator animator;

    private float delayBetweenStartDrillAndPS = .4f;

    private void Awake() {
        animator = GetComponent<Animator>();
        orbExtractor.OnExtractorStartedDrilling += OrbExtractor_OnExtractorStartedDrilling;
    }

    private void OrbExtractor_OnExtractorStartedDrilling(object sender, System.EventArgs e) {
        animator.SetTrigger("Drill");
        StartCoroutine(TriggerParticleSystemsAfterDelay());
    }

    private IEnumerator TriggerParticleSystemsAfterDelay() {
        yield return new WaitForSeconds(delayBetweenStartDrillAndPS);
        dirtPS.Play();
        sparksPS.Play();
    }
}
