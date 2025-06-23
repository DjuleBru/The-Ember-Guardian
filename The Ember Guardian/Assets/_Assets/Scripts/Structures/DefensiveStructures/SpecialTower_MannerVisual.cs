using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTower_MannerVisual : MonoBehaviour {

    [SerializeField] private SpecialTower specialTower;
    [SerializeField] private SpecialTower_Manner manner;
    [SerializeField] private Animator mannerAnimator;
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] protected SpriteRenderer weaponLightsSpriteRenderer;
    [SerializeField] protected SpriteRenderer mannerSpriteRenderer;
    [SerializeField] protected List<Sprite> lightsSpriteList;
    [SerializeField] protected Color outOfAmmoLightColor;
    [SerializeField] protected Color initialAmmoLightColor;
    [SerializeField] protected float shellOutPSDelay;
    [SerializeField] protected ParticleSystem shellOutPS;

    [SerializeField] protected float reloadLightsTime;

    private bool outOfAmmo;
    private bool lastClipStartedEmptying;
    private bool aiming;
    private bool cooldownAnimationDone;
    private bool hasCooldown;

    protected int lightSpriteIndex;
    private void Awake() {
        mannerSpriteRenderer.gameObject.SetActive(false);
        lightSpriteIndex = lightsSpriteList.Count;
    }

    private void Start() {
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

        hasCooldown = manner.GetHasCooldown();
    }


    private void Manner_OnMannerReloadingHandsEnded(object sender, System.EventArgs e) {
        float delayBetweenSprites = reloadLightsTime / lightsSpriteList.Count;

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

    private void Manner_OnMannerCooldownEventTriggered(object sender, System.EventArgs e) {

        if(hasCooldown) {
            mannerAnimator.SetTrigger("Cooldown");
        }

        cooldownAnimationDone = true;
        StartCoroutine(TriggerShellOutAfterDelay());
    }

    private IEnumerator TriggerShellOutAfterDelay() {
        yield return new WaitForSeconds(shellOutPSDelay);
        shellOutPS.Emit(1);
    }

    private void Manner_OnMannerReloadingStarted(object sender, System.EventArgs e) {
        mannerAnimator.SetTrigger("Reload");

        if(outOfAmmo) {
            weaponLightsSpriteRenderer.sprite = lightsSpriteList[0];
            weaponLightsSpriteRenderer.color = initialAmmoLightColor;
        }
    }

    private void Manner_OnMannerShot(object sender, System.EventArgs e) {
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

    private void Manner_OnNoCreatureFound(object sender, System.EventArgs e) {
        aiming = false;

        if (outOfAmmo) return;
        if (hasCooldown && !cooldownAnimationDone) return;
        mannerAnimator.SetBool("Aiming", false);
    }

    private void Manner_OnCreatureTargeted(object sender, System.EventArgs e) {
        aiming = true;

        if (outOfAmmo) return;
        mannerAnimator.SetBool("Aiming", true);
    }

    private void Manner_OnEngineerStoppedManning(object sender, System.EventArgs e) {
        mannerAnimator.SetBool("Manned", false);
        mannerAnimator.SetBool("Aiming", false);
        aiming = false;

        mannerSpriteRenderer.gameObject.SetActive(false);
    }

    private void Manner_OnEngineerStartedManning(object sender, System.EventArgs e) {
        mannerAnimator.SetBool("Manned", true);
        
        mannerSpriteRenderer.gameObject.SetActive(true);
    }

    private void SpecialTower_OnAmmoClipAdded(object sender, System.EventArgs e) {
        if (aiming) {
            mannerAnimator.SetBool("Aiming", true);
        }

        weaponLightsSpriteRenderer.color = initialAmmoLightColor;
        outOfAmmo = false;
    }

    private void SpecialTower_OnAmmoClipRemoved(object sender, System.EventArgs e) {
        if(specialTower.GetCurrentAmmoClip() == 0) {
            lastClipStartedEmptying = true;
        }
    }
}
