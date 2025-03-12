using Lofelt.NiceVibrations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamepadVibrationsManager : MonoBehaviour
{
    public HapticClip teleporterAppearHapticClip;
    public HapticClip teleporterDisappearHapticClip;

    public float teleporterDuration = 5f;
    public float etractEmberDuration = 5f;
    private bool isLerping;
    private float timeElapsed;
    private float lerpDuration;
    private float lerpStartValue;
    private float lerpEndValue;

    private float currentLerpValue;

    private void Start() {
        if(SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB || SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level || SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {

            PlayerShoot.Instance.OnPlayerShot += PlayerShoot_OnPlayerShotProjectile;
            PlayerShoot.Instance.OnPlayerAmmoRefilled += PlayerSHoot_OnPlayerAmmoRefilled;
            PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;

            Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
            Player.Instance.OnPlayerDied += Player_OnPlayerDied;
            Player.Instance.OnPlayerHealed += Player_OnPlayerHealed;

            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
            UICurrencyManager.PlayerInventoryUI.OnCurrencyDropped += UICurrencyManager_OnCurrencyDropped;

            HubMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_OnAnyHubMerchantItemBought;

            CreatureAI.OnAnyCreatureAggro += CreatureAI_OnAnyCreatureAggro;

            Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
            Portal.OnAnyPlayerTeleported += Portal_OnAnyPlayerTeleported;
            Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;
            Portal.OnAnyTeleporterActivatedOut += Portal_OnAnyTeleporterActivatedOut;
            Portal.OnAnyTeleporterActivated += Portal_OnAnyTeleporterActivated;
            Portal.OnAnyPortalAppeared += Portal_OnAnyPortalAppeared;
            Portal.OnAnyPortalDisappeared += Portal_OnAnyPortalDisappeared;

            Fire.OnAnyFireEmberExtractionStarted += Fire_OnFireEmberExtractionStarted;
            Fire.OnAnyFireEmberExtractionStopped += Fire_OnFireEmberExtractionStopped;
            Fire.OnAnyFireFuelled += Fire_OnAnyFireFuelled;

            Collectible.OnAnyCollectibleEnteredSlot += Collectible_OnAnyCollectibleEnteredSlot;
            ItemButtonUI_Visual.OnAnyGemPSTriggered += ItemButtonUI_Visual_OnAnyGemPSTriggered;
            ItemButtonUI.OnAnyHubMerchantItemFailedBuy += ItemButtonUI_OnAnyHubMerchantItemFailedBuy;
            ItemButtonUI.OnAnyButtonSelected += ItemButtonUI_OnAnyButtonSelected;
        }


    }

    private void Update() {
        if (isLerping) {
            // Calculer la progression du lerp (0 à 1) basé sur le temps écoulé
            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / lerpDuration); // Limite entre 0 et 1

            // Lerp entre les deux valeurs
            currentLerpValue = Mathf.Lerp(lerpStartValue, lerpEndValue, t);

            // Si la durée est écoulée, arrêter l'interpolation
            if (t >= 1f) {
                isLerping = false;
            }

            HapticController.clipLevel = currentLerpValue;
            HapticController.clipFrequencyShift = currentLerpValue;
        }
    }

    private void Fire_OnFireEmberExtractionStopped(object sender, System.EventArgs e) {
        isLerping = false;
        HapticController.Stop();
    }

    private void Fire_OnFireEmberExtractionStarted(object sender, System.EventArgs e) {
        StartIncreasingVibration(0, 1f, etractEmberDuration);
    }

    #region CURRENCIES
    private void Collectible_OnAnyCollectibleEnteredSlot(object sender, System.EventArgs e) {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
    }

    private void UICurrencyManager_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        HapticPatterns.PlayEmphasis(.03f, 0.0f);
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.bigBlueOrb || e.currencyUIDropped.GetCurrencyType() == PlayerCurrencies.CurrencyType.bigRedOrb) {
            HapticPatterns.PlayEmphasis(.05f, 0.0f);
        }
        else {
            HapticPatterns.PlayEmphasis(.02f, 0.0f);
        }
    }

    #endregion

    #region PORTALS
    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, System.EventArgs e) {
        HapticPatterns.PlayConstant(.5f, 0f, .5f);
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, System.EventArgs e) {
        HapticPatterns.PlayEmphasis(1f, 0.0f);
    }

    private void Portal_OnAnyPortalDisappeared(object sender, System.EventArgs e) {
        HapticController.Play(teleporterAppearHapticClip);
    }

    private void Portal_OnAnyPortalAppeared(object sender, System.EventArgs e) {
        HapticController.Play(teleporterDisappearHapticClip);
    }

    private void Portal_OnAnyPlayerTeleported(object sender, System.EventArgs e) {
        HapticPatterns.PlayEmphasis(1f, 0.0f);
    }

    private void Portal_OnAnyTeleporterActivatedOut(object sender, System.EventArgs e) {
        HapticPatterns.PlayEmphasis(1f, 1.0f);
    }
    private void Portal_OnAnyTeleporterActivated(object sender, System.EventArgs e) {
        StartIncreasingVibration(0, 1f, teleporterDuration);
    }
    #endregion

    #region PLAYER
    private void Player_OnPlayerHealed(object sender, Player.OnPlayerChangedHealthEventArgs e) {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {

    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        HapticPatterns.PlayConstant(.7f, 0.0f, .5f);
    }

    #endregion

    #region GUN
    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        HapticPatterns.PlayEmphasis(.7f, 0.0f);
    }

    private void PlayerSHoot_OnPlayerAmmoRefilled(object sender, PlayerShoot.OnAmmoRefilledEventArgs e) {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
    }
    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
    }


    #endregion

    #region UI
    private void ItemButtonUI_OnAnyHubMerchantItemFailedBuy(object sender, System.EventArgs e) {
        //HapticPatterns.PlayPreset(HapticPatterns.PresetType.Failure);
    }

    private void ItemButtonUI_Visual_OnAnyGemPSTriggered(object sender, System.EventArgs e) {
        HapticPatterns.PlayEmphasis(.7f, 0.0f);
    }
    private void HubMerchantItem_OnAnyHubMerchantItemBought(object sender, System.EventArgs e) {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Success);
    }
    private void ItemButtonUI_OnAnyButtonSelected(object sender, System.EventArgs e) {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
    }

    #endregion

    #region OTHER


    private void CreatureAI_OnAnyCreatureAggro(object sender, System.EventArgs e) {
        HapticPatterns.PlayPreset(HapticPatterns.PresetType.Warning);
    }

    private void Fire_OnAnyFireFuelled(object sender, System.EventArgs e) {
        HapticPatterns.PlayEmphasis(.2f, 0.0f);
    }
    #endregion

    public void StartIncreasingVibration(float start, float end, float duration) {
        HapticPatterns.PlayConstant(currentLerpValue, currentLerpValue, etractEmberDuration);
        lerpStartValue = start;
        lerpEndValue = end;
        this.lerpDuration = duration;
        timeElapsed = 0f;
        isLerping = true;
    }

    private void OnDestroy() {
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB || SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level || SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {

            PlayerShoot.Instance.OnPlayerShot -= PlayerShoot_OnPlayerShotProjectile;
            PlayerShoot.Instance.OnPlayerAmmoRefilled -= PlayerSHoot_OnPlayerAmmoRefilled;
            PlayerShoot.Instance.OnPlayerReload -= PlayerShoot_OnPlayerReload;

            Player.Instance.OnPlayerDamaged -= Player_OnPlayerDamaged;
            Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
            Player.Instance.OnPlayerHealed -= Player_OnPlayerHealed;

            UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected -= UICurrencyManager_OnCurrencyCollected;
            UICurrencyManager.PlayerInventoryUI.OnCurrencyDropped -= UICurrencyManager_OnCurrencyDropped;

            HubMerchantItem.OnAnyHubMerchantItemBought -= HubMerchantItem_OnAnyHubMerchantItemBought;

            CreatureAI.OnAnyCreatureAggro -= CreatureAI_OnAnyCreatureAggro;

            Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
            Portal.OnAnyPlayerTeleported -= Portal_OnAnyPlayerTeleported;
            Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;
            Portal.OnAnyTeleporterActivatedOut -= Portal_OnAnyTeleporterActivatedOut;
            Portal.OnAnyTeleporterActivated -= Portal_OnAnyTeleporterActivated;
            Portal.OnAnyPortalAppeared -= Portal_OnAnyPortalAppeared;
            Portal.OnAnyPortalDisappeared -= Portal_OnAnyPortalDisappeared;

            Fire.OnAnyFireEmberExtractionStarted -= Fire_OnFireEmberExtractionStarted;
            Fire.OnAnyFireEmberExtractionStopped -= Fire_OnFireEmberExtractionStopped;
            Fire.OnAnyFireFuelled -= Fire_OnAnyFireFuelled;

            Collectible.OnAnyCollectibleEnteredSlot -= Collectible_OnAnyCollectibleEnteredSlot;
            ItemButtonUI_Visual.OnAnyGemPSTriggered -= ItemButtonUI_Visual_OnAnyGemPSTriggered;
            ItemButtonUI.OnAnyHubMerchantItemFailedBuy -= ItemButtonUI_OnAnyHubMerchantItemFailedBuy;
            ItemButtonUI.OnAnyButtonSelected -= ItemButtonUI_OnAnyButtonSelected;
        }
    }
}
