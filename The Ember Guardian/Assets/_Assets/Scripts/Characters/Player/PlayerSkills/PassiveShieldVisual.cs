using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveShieldVisual : MonoBehaviour
{
    [SerializeField] private Animator shieldAnimator;
    [SerializeField] private SpriteRenderer shieldBodySpriteRenderer;
    [SerializeField] private Sprite level1ShieldSprite;
    [SerializeField] private Sprite level2ShieldSprite;
    [SerializeField] private Sprite level3ShieldSprite;
    [SerializeField] private Sprite level4ShieldSprite;
    [SerializeField] private Sprite level5ShieldSprite;

    private PassiveShield passiveShield;

    private void Awake() {
        passiveShield = GetComponentInParent<PassiveShield>();
        shieldBodySpriteRenderer.enabled = false;
    }

    private void Start() {
        passiveShield.OnShieldActivated += PassiveShield_OnShieldActivated;
        passiveShield.OnShieldDied += PassiveShield_OnShieldDied;
        FastTravelTP.OnAnyPlayerWarped += FastTravelTP_OnAnyPlayerWarped;
        FastTravelTP.OnAnyPlayerWarpedOut += FastTravelTP_OnAnyPlayerWarpedOut;
        Portal.OnAnyPlayerTeleported += Portal_OnAnyPlayerTeleported;
    }

    private void Portal_OnAnyPlayerTeleported(object sender, System.EventArgs e) {
        if(passiveShield.GetShieldActive()) {
            shieldBodySpriteRenderer.enabled = false;
        }
    }

    private void FastTravelTP_OnAnyPlayerWarpedOut(object sender, System.EventArgs e) {
        if (passiveShield.GetShieldActive()) {
            shieldBodySpriteRenderer.enabled = true;
        }
    }

    private void FastTravelTP_OnAnyPlayerWarped(object sender, System.EventArgs e) {
        if (passiveShield.GetShieldActive()) {
            shieldBodySpriteRenderer.enabled = false;
        }
    }

    private void PassiveShield_OnShieldDied(object sender, System.EventArgs e) {
        shieldAnimator.ResetTrigger("Activate");
        shieldAnimator.SetTrigger("Die");
    }

    private void PassiveShield_OnShieldActivated(object sender, System.EventArgs e) {
        shieldBodySpriteRenderer.enabled = true;
        shieldAnimator.ResetTrigger("Die");
        shieldAnimator.SetTrigger("Activate");
    }

    private void OnDestroy() {
        FastTravelTP.OnAnyPlayerWarped -= FastTravelTP_OnAnyPlayerWarped;
        FastTravelTP.OnAnyPlayerWarpedOut -= FastTravelTP_OnAnyPlayerWarpedOut;
        Portal.OnAnyPlayerTeleported -= Portal_OnAnyPlayerTeleported;
    }
}
