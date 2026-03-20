using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunVisual_Pistol : GunVisual
{

    [SerializeField] protected SpriteRenderer silencerSpriteRenderer;
    protected override void Awake() {
        base.Awake();
        silencerSpriteRenderer.enabled = false;
    }

    protected override void PlayerShoot_OnPlayerSwitchedFireMode(object sender, System.EventArgs e) {
        if (!gun.GetGunActive()) return;

        if (gunSecondaryAbilityActiveSpriteRenderer != null) {
            gunSecondaryFireModeActive = !gunSecondaryFireModeActive;

            if(PlayerShoot.Instance.GetFirstSecondaryAbilityEquipped()) {
                if (PlayerShoot.Instance.GetSilencerActive()) {
                    silencerSpriteRenderer.enabled = gunSecondaryFireModeActive;
                }
                else {
                    silencerSpriteRenderer.enabled = gunSecondaryFireModeActive;
                }
            }

            if(PlayerShoot.Instance.GetSecondSecondaryAbilityEquipped()) {
                if (PlayerShoot.Instance.GetPistolExplosiveBulletsActive()) {

                    gunSecondaryAbilityActiveSpriteRenderer.enabled = gunSecondaryFireModeActive;

                }
                else {
                    gunSecondaryAbilityActiveSpriteRenderer.enabled = false;
                }
            }
           

          
        }

        if (gunCooldownLightsSpriteRenderer != null) {
            if (PlayerShoot.Instance.GetSniperPiercingRoundsActive() || PlayerShoot.Instance.GetRevolverBouncingBulletsActive()) {
                gunCooldownLightsSpriteRenderer.color = orangeColor;
            }
            else {
                gunCooldownLightsSpriteRenderer.color = cooldownLightsColor;
            }
        }

    }


}
