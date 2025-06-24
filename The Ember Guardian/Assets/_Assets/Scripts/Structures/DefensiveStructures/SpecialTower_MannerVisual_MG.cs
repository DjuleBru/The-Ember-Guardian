using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower_MannerVisual_MG : SpecialTower_MannerVisual {

    [SerializeField] protected SpecialTower_Manner_MG mannerMG;
    [SerializeField] protected SpriteRenderer magSpriteRenderer;
    [SerializeField] protected GameObject magSpriteRenderer_Reloading;
    [SerializeField] protected GameObject lightsSpriteRenderer_Reloading;
    [SerializeField] protected SpriteRenderer reloaderSpriteRenderer;
    [SerializeField] protected SpriteRenderer reloaderSpriteRenderer_Reloading;
    [SerializeField] protected SpriteRenderer reloaderSpriteRenderer_Hands;
    [SerializeField] protected List<Sprite> magSpriteList;
    [SerializeField] protected Sprite lightSprite1;
    [SerializeField] protected Sprite lightSprite2;
    [SerializeField] protected Sprite magSprite1;
    [SerializeField] protected Sprite magSprite2;
    [SerializeField] protected RuntimeAnimatorController level2AnimatorController;

    protected bool reloaderActive;
    protected int shotCountWereReloaderGoesIdle = 16;
    protected float reloadAnimationDuration = 1.8f;
    
    protected override void Awake() {
        base.Awake();
        magSpriteRenderer_Reloading.SetActive(false);
        lightsSpriteRenderer_Reloading.SetActive(false);
        reloaderSpriteRenderer.gameObject.SetActive(true);
        reloaderSpriteRenderer_Hands.gameObject.SetActive(true);
        reloaderSpriteRenderer_Reloading.gameObject.SetActive(false);
        specialTower.OnStructureUpgraded += SpecialTower_OnStructureUpgraded;
    }

    private void SpecialTower_OnStructureUpgraded(object sender, System.EventArgs e) {
        int engineersManning = mannerAnimator.GetInteger("EngineersManning");

        mannerAnimator.runtimeAnimatorController = level2AnimatorController;

        mannerAnimator.SetInteger("EngineersManning", engineersManning);
    }

    protected override void Manner_OnEngineerStoppedManning(object sender, System.EventArgs e) {
        base.Manner_OnEngineerStoppedManning(sender, e);
        if(manner.GetEngineersManning() < manner.GetMaxEngineersManning()) {
            reloaderSpriteRenderer.enabled = false;
            reloaderSpriteRenderer_Hands.enabled = false;
            reloaderSpriteRenderer_Reloading.enabled = false;
        } else {
            reloaderSpriteRenderer.enabled = true;
            reloaderSpriteRenderer_Hands.enabled = true;
            reloaderSpriteRenderer_Reloading.enabled = true;
        }
    }

    protected override void Manner_OnEngineerStartedManning(object sender, System.EventArgs e) {
        base.Manner_OnEngineerStartedManning(sender, e);
        if (manner.GetEngineersManning() < manner.GetMaxEngineersManning()) {
            reloaderSpriteRenderer.enabled = false;
            reloaderSpriteRenderer_Hands.enabled = false;
            reloaderSpriteRenderer_Reloading.enabled = false;
        }
        else {
            reloaderSpriteRenderer.enabled = true;
            reloaderSpriteRenderer_Hands.enabled = true;
            reloaderSpriteRenderer_Reloading.enabled = true;
        }
    }

    protected override void Manner_OnMannerShot(object sender, System.EventArgs e) {
        if(mannerMG.GetLevel2MG()) {
            if(mannerMG.GetShootingTop()) {
                mannerAnimator.SetTrigger("Shoot_Top");
            } else {
                mannerAnimator.SetTrigger("Shoot_Bot");
            }
        } else {
            mannerAnimator.SetTrigger("Shoot");
        }

        cooldownAnimationDone = false;

        float bulletsAmountNormalized = (float)manner.GetCurrentShotIndex() / (float)manner.GetShotsPerAmmoClip();

        if (bulletsAmountNormalized == .5f) {
            bulletsAmountNormalized = .49f;
        }

        StartCoroutine(HandleAmmoBeltVisuals());


        if (outOfAmmo && manner.GetCurrentShotIndex() == 0) {
            mannerAnimator.SetBool("Aiming", false);
        }

        if (lastClipStartedEmptying) {
            outOfAmmo = true;
        }
    }

    protected override void Manner_OnMannerReloadingStarted(object sender, System.EventArgs e) {
        mannerAnimator.SetTrigger("Reload");
        StartCoroutine(HandleReloadingAmmoBelt());
    }
    private IEnumerator HandleAmmoBeltVisuals() {
        yield return new WaitForSeconds(.2f);

        int shotIndex = manner.GetCurrentShotIndex();
        // Si l'index dépasse la taille de la liste lights, on utilise un sprite de fallback
        if (shotIndex >= lightsSpriteList.Count) {
            weaponLightsSpriteRenderer.sprite = lightSprite2;
        }
        else {
            weaponLightsSpriteRenderer.sprite = lightsSpriteList[shotIndex];
        }

        // Pareil pour le mag
        if (shotIndex >= magSpriteList.Count) {
            magSpriteRenderer.sprite = magSprite2;
        }
        else {
            magSpriteRenderer.sprite = magSpriteList[shotIndex];
        }

        if (manner.GetCurrentShotIndex() > shotCountWereReloaderGoesIdle) {

            reloaderSpriteRenderer.gameObject.SetActive(true);
            reloaderSpriteRenderer_Hands.gameObject.SetActive(true);
            reloaderSpriteRenderer_Reloading.gameObject.SetActive(false);

        } else {

            reloaderSpriteRenderer.gameObject.SetActive(false);
            reloaderSpriteRenderer_Hands.gameObject.SetActive(false);
            reloaderSpriteRenderer_Reloading.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(.1f);

        if (manner.GetCurrentShotIndex() > lightsSpriteList.Count) {
            weaponLightsSpriteRenderer.sprite = lightSprite1;
            magSpriteRenderer.sprite = magSprite1;
        }
    }

    private IEnumerator HandleReloadingAmmoBelt() {
        reloaderSpriteRenderer.gameObject.SetActive(true);
        reloaderSpriteRenderer_Hands.gameObject.SetActive(true);
        reloaderSpriteRenderer_Reloading.gameObject.SetActive(false);

        magSpriteRenderer_Reloading.SetActive(true);
        lightsSpriteRenderer_Reloading.SetActive(true);

        yield return new WaitForSeconds(reloadAnimationDuration);

        weaponLightsSpriteRenderer.sprite = lightSprite1;
        magSpriteRenderer.sprite = magSprite1;
        magSpriteRenderer_Reloading.SetActive(false);
        lightsSpriteRenderer_Reloading.SetActive(false);
    } 
}
