using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemButtonUI_Visual : MonoBehaviour
{
    [SerializeField] private ParticleSystem greenGemPS;
    [SerializeField] private ParticleSystem redGemPS;

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

    public void StartBuyAnimation(int redCost, int greenCost) {
        StartCoroutine(StartBuyAnimationCoroutine(redCost, greenCost));
    }

    private IEnumerator StartBuyAnimationCoroutine(int redCost, int greenCost) {
        int greenParticlesToEmit = greenCost * 5;
        int redParticlesToEmit = redCost * 5;


        animator.enabled = true;
        animator.SetTrigger("Buy");

        //yield return new WaitForSeconds(delayToBurstPS/2);

        
        yield return new WaitForSeconds(delayToBurstPS-.2f);

        OnAnyGemPSTriggered?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(.2f);

        greenGemPS.Emit(greenParticlesToEmit);
        redGemPS.Emit(redParticlesToEmit);
    }
}
