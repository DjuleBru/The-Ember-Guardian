using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower_MannerVisual : MonoBehaviour {

    [SerializeField] protected SpecialTower specialTower;
    [SerializeField] protected SpecialTower_Manner manner;
    [SerializeField] protected Animator mannerAnimator;
    [SerializeField] protected SpriteRenderer weaponSpriteRenderer;
    [SerializeField] protected SpriteRenderer weaponLightsSpriteRenderer;
    [SerializeField] protected SpriteRenderer mannerSpriteRenderer;
    [SerializeField] protected List<Sprite> lightsSpriteList;
    [SerializeField] protected Color outOfAmmoLightColor;
    [SerializeField] protected Color initialAmmoLightColor;
    [SerializeField] protected float shellOutPSDelay;
    [SerializeField] protected ParticleSystem shellOutPS;

    [SerializeField] protected float reloadLightsTime;
    [SerializeField] protected bool reloadsLights = true;

    protected bool outOfAmmo;
    protected bool lastClipStartedEmptying;
    protected bool aiming;
    protected bool cooldownAnimationDone;
    protected bool hasCooldown;

    protected int lightSpriteIndex;
    protected virtual void Awake() {
        mannerSpriteRenderer.gameObject.SetActive(false);
        lightSpriteIndex = lightsSpriteList.Count;
    }

    protected void Start() {
        manner.OnEngineerStartedManning += Manner_OnEngineerStartedManning;
        manner.OnEngineerStoppedManning += Manner_OnEngineerStoppedManning;
        manner.OnCreatureTargeted += Manner_OnCreatureTargeted;
        manner.OnNoCreatureFound += Manner_OnNoCreatureFound;
        manner.OnMannerShot += Manner_OnMannerShot;
        manner.OnMannerReloadingStarted += Manner_OnMannerReloadingStarted;
        manner.OnMannerReloadingHandsEnded += Manner_OnMannerReloadingHandsEnded;
        manner.OnMannerCooldownEventTriggered += Manner_OnMannerCooldownEventTriggered;
        specialTower.OnAmmoClipRemoved += SpecialTower_OnAmmoClipRemoved;
        specialTower.OnAmmoClipAdded += SpecialTower_OnAmmoClipAdded;

        hasCooldown = manner.GetHasCooldownAnimation();
    }


    protected void Manner_OnMannerReloadingHandsEnded(object sender, System.EventArgs e) {
        float delayBetweenSprites = reloadLightsTime / lightsSpriteList.Count;
        if (!reloadsLights) return;

        StartCoroutine(ChangeRemainingBulletsVisuals(delayBetweenSprites, 0, lightsSpriteList.Count-1));
    }

    protected IEnumerator ChangeRemainingBulletsVisuals(float delayBetweenSprites, int initialSpriteIndex, int finalSpriteIndex) {
        weaponLightsSpriteRenderer.sprite = lightsSpriteList[initialSpriteIndex];

        if (finalSpriteIndex < initialSpriteIndex) {
            // Shooting: on descend les index
            for (int i = initialSpriteIndex - 1; i >= finalSpriteIndex; i--) {
                weaponLightsSpriteRenderer.sprite = lightsSpriteList[i];
                yield return new WaitForSeconds(delayBetweenSprites);
                lightSpriteIndex = i;
            }
        }
        else {
            // Reloading: on monte les index
            for (int i = initialSpriteIndex + 1; i <= finalSpriteIndex; i++) {
                weaponLightsSpriteRenderer.sprite = lightsSpriteList[i];
                yield return new WaitForSeconds(delayBetweenSprites);
                lightSpriteIndex = i;
            }
        }
    }

    protected void Manner_OnMannerCooldownEventTriggered(object sender, System.EventArgs e) {

        if(hasCooldown) {
            mannerAnimator.SetTrigger("Cooldown");
        }

        cooldownAnimationDone = true;
        StartCoroutine(TriggerShellOutAfterDelay());
    }

    protected IEnumerator TriggerShellOutAfterDelay() {
        yield return new WaitForSeconds(shellOutPSDelay);
        shellOutPS.Emit(1);
    }

    protected virtual void Manner_OnMannerReloadingStarted(object sender, System.EventArgs e) {
        mannerAnimator.SetTrigger("Reload");

        if(outOfAmmo) {
            weaponLightsSpriteRenderer.sprite = lightsSpriteList[0];
            weaponLightsSpriteRenderer.color = initialAmmoLightColor;
        }
    }

    protected virtual void Manner_OnMannerShot(object sender, System.EventArgs e) {
        mannerAnimator.SetTrigger("Shoot");
        cooldownAnimationDone = false;

        float bulletsAmountNormalized = (float)manner.GetCurrentShotIndex() / (float)manner.GetShotsPerAmmoClip();

        if (bulletsAmountNormalized == .5f) {
            bulletsAmountNormalized = .49f;
        }

        lightSpriteIndex = Mathf.FloorToInt(bulletsAmountNormalized * (lightsSpriteList.Count));
        lightSpriteIndex = Mathf.Clamp(lightSpriteIndex, 0, lightsSpriteList.Count - 1);

        weaponLightsSpriteRenderer.sprite = lightsSpriteList[lightSpriteIndex];

        if(outOfAmmo && manner.GetCurrentShotIndex() == 0) {
            mannerAnimator.SetBool("Aiming", false);
            weaponLightsSpriteRenderer.color = outOfAmmoLightColor;
            weaponLightsSpriteRenderer.sprite = lightsSpriteList[lightsSpriteList.Count-1];
        }

        if(lastClipStartedEmptying) {
            outOfAmmo = true;
        }
    }

    protected void Manner_OnNoCreatureFound(object sender, System.EventArgs e) {
        aiming = false;

        if (outOfAmmo) return;
        if (hasCooldown && !cooldownAnimationDone) return;
        mannerAnimator.SetBool("Aiming", false);
    }

    protected void Manner_OnCreatureTargeted(object sender, System.EventArgs e) {
        aiming = true;

        if (outOfAmmo) return;
        mannerAnimator.SetBool("Aiming", true);
    }

    protected virtual void Manner_OnEngineerStoppedManning(object sender, System.EventArgs e) {
        mannerAnimator.SetBool("Aiming", false);
        mannerAnimator.SetInteger("EngineersManning", manner.GetEngineersManning());

        aiming = false;

        mannerSpriteRenderer.gameObject.SetActive(false);
    }

    protected virtual void Manner_OnEngineerStartedManning(object sender, System.EventArgs e) {
        mannerAnimator.SetInteger("EngineersManning", manner.GetEngineersManning());
        
        mannerSpriteRenderer.gameObject.SetActive(true);
    }

    protected void SpecialTower_OnAmmoClipAdded(object sender, System.EventArgs e) {
        if (aiming) {
            mannerAnimator.SetBool("Aiming", true);
        }

        weaponLightsSpriteRenderer.color = initialAmmoLightColor;
        outOfAmmo = false;
    }

    protected void SpecialTower_OnAmmoClipRemoved(object sender, System.EventArgs e) {
        if(specialTower.GetCurrentAmmoClip() == 0) {
            lastClipStartedEmptying = true;
        }
    }
}
