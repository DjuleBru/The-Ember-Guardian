using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemButtonUI_Visual : MonoBehaviour
{
    [SerializeField] private Transform greenGemPSPrefab;
    [SerializeField] private Transform redGemPSPrefab;
    [SerializeField] private Transform blueGemPSPrefab;
    [SerializeField] private Transform yellowGemPSPrefab;
    [SerializeField] private Transform purpleGemPSPrefab;
    [SerializeField] private Transform cyanGemPSPrefab;

    private Animator animator;

    private float delayToBurstPS = .66f;
    public static event EventHandler OnAnyGemPSTriggered;

    private void Awake() {
        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    public void SetItemLoadedBought() {
        animator = GetComponent<Animator>();
        animator.enabled = true;
    }

    public void StartBuyAnimation(int redCost, int greenCost, int blueCost, int yellowCost, int purpleCost, int cyanCost) {
        StartCoroutine(StartBuyAnimationCoroutine(redCost, greenCost, blueCost, yellowCost, purpleCost, cyanCost));
    }

    public void DisableAnimator() {
        animator.enabled = false;
    }

    public void ResetAnimator() {
        animator.SetTrigger("Reset");
    }

    public void BuyAnimationInstant(int redCost, int greenCost, int blueCost, int yellowCost, int purpleCost, int cyanCost) {
        int greenParticlesToEmit = greenCost * 5;
        int redParticlesToEmit = redCost * 5;
        int blueParticlesToEmit = blueCost * 5;
        int yellowParticlesToEmit = yellowCost * 5;
        int purpleParticlesToEmit = purpleCost * 5;
        int cyanParticlesToEmit = cyanCost * 5;

        animator.enabled = true;
        animator.SetTrigger("Buy");
        OnAnyGemPSTriggered?.Invoke(this, EventArgs.Empty);

        if(greenParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(greenGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(greenParticlesToEmit);
        }

        if (redParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(redGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(redParticlesToEmit);
        }

        if (blueParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(blueGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(blueParticlesToEmit);
        }

        if (yellowParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(yellowGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(yellowParticlesToEmit);
        }

        if (purpleParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(purpleGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(purpleParticlesToEmit);
        }

        if (cyanParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(cyanGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(cyanParticlesToEmit);
        }
    }

    private IEnumerator StartBuyAnimationCoroutine(int redCost, int greenCost, int blueCost, int yellowCost, int purpleCost, int cyanCost) {
        int greenParticlesToEmit = greenCost * 5;
        int redParticlesToEmit = redCost * 5;
        int blueParticlesToEmit = blueCost * 5;
        int yellowParticlesToEmit = yellowCost * 5;
        int purpleParticlesToEmit = purpleCost * 5;
        int cyanParticlesToEmit = cyanCost * 5;

        animator.enabled = true;
        animator.SetTrigger("Buy");
        
        
        yield return new WaitForSeconds(delayToBurstPS-.2f);

        OnAnyGemPSTriggered?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(.2f);

        if (greenParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(greenGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(greenParticlesToEmit);
        }

        if (redParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(redGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(redParticlesToEmit);
        }

        if (blueParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(blueGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(blueParticlesToEmit);
        }

        if (yellowParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(yellowGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(yellowParticlesToEmit);
        }

        if (purpleParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(purpleGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(purpleParticlesToEmit);
        }

        if (cyanParticlesToEmit != 0) {
            ParticleSystem ps = Instantiate(cyanGemPSPrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
            ps.Emit(cyanParticlesToEmit);
        }
    }
}
