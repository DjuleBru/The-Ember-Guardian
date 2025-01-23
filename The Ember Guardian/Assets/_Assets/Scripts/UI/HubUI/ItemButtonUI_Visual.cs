using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemButtonUI_Visual : MonoBehaviour
{
    [SerializeField] private ParticleSystem greenGemPS;
    [SerializeField] private ParticleSystem redGemPS;
    [SerializeField] private ParticleSystem blueGemPS;
    [SerializeField] private ParticleSystem yellowGemPS;
    [SerializeField] private ParticleSystem purpleGemPS;

    private Animator animator;

    private float delayToBurstPS = .66f;
    public static event EventHandler OnAnyGemPSTriggered;

    private void Awake() {
        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    public void SetItemLoadedBought() {
        animator.enabled = true;
    }

    public void StartBuyAnimation(int redCost, int greenCost, int blueCost, int yellowCost, int purpleCost) {
        StartCoroutine(StartBuyAnimationCoroutine(redCost, greenCost, blueCost, yellowCost, purpleCost));
    }

    private IEnumerator StartBuyAnimationCoroutine(int redCost, int greenCost, int blueCost, int yellowCost, int purpleCost) {
        int greenParticlesToEmit = greenCost * 5;
        int redParticlesToEmit = redCost * 5;
        int blueParticlesToEmit = blueCost * 5;
        int yellowParticlesToEmit = yellowCost * 5;
        int purpleParticlesToEmit = purpleCost * 5;

        animator.enabled = true;
        animator.SetTrigger("Buy");

        //yield return new WaitForSeconds(delayToBurstPS/2);

        
        yield return new WaitForSeconds(delayToBurstPS-.2f);

        OnAnyGemPSTriggered?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(.2f);

        greenGemPS.Emit(greenParticlesToEmit);
        redGemPS.Emit(redParticlesToEmit);
        blueGemPS.Emit(blueParticlesToEmit);
        yellowGemPS.Emit(yellowParticlesToEmit);
        purpleGemPS.Emit(purpleParticlesToEmit);
    }
}
