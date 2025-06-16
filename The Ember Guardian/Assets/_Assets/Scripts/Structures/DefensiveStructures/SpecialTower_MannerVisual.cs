using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower_MannerVisual : MonoBehaviour {

    [SerializeField] private SpecialTower_Manner manner;
    [SerializeField] private Animator mannerAnimator;
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] protected SpriteRenderer weaponLightsSpriteRenderer;
    [SerializeField] protected SpriteRenderer mannerSpriteRenderer;
    [SerializeField] protected List<Sprite> lightsSpriteList;

    private void Awake() {
        weaponLightsSpriteRenderer.gameObject.SetActive(false);
        mannerSpriteRenderer.gameObject.SetActive(false);
    }

    private void Start() {
        manner.OnEngineerStartedManning += Manner_OnEngineerStartedManning;
        manner.OnEngineerStoppedManning += Manner_OnEngineerStoppedManning;
        manner.OnCreatureTargeted += Manner_OnCreatureTargeted;
        manner.OnNoCreatureFound += Manner_OnNoCreatureFound;
        manner.OnMannerShot += Manner_OnMannerShot;
        manner.OnMannerReloadingStarted += Manner_OnMannerReloadingStarted;
    }

    private void Manner_OnMannerReloadingStarted(object sender, System.EventArgs e) {

    }

    private void Manner_OnMannerShot(object sender, System.EventArgs e) {
        mannerAnimator.SetTrigger("Shoot");
    }

    private void Manner_OnNoCreatureFound(object sender, System.EventArgs e) {
        mannerAnimator.SetBool("Aiming", false);
    }

    private void Manner_OnCreatureTargeted(object sender, System.EventArgs e) {
        mannerAnimator.SetBool("Aiming", true);
    }

    private void Manner_OnEngineerStoppedManning(object sender, System.EventArgs e) {
        mannerAnimator.SetBool("Manned", false);
        mannerAnimator.SetBool("Aiming", false);

        weaponLightsSpriteRenderer.gameObject.SetActive(false);
        mannerSpriteRenderer.gameObject.SetActive(false);
    }

    private void Manner_OnEngineerStartedManning(object sender, System.EventArgs e) {
        mannerAnimator.SetBool("Manned", true);

        weaponLightsSpriteRenderer.gameObject.SetActive(true);
        mannerSpriteRenderer.gameObject.SetActive(true);
    }
}
