using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICurrencyManagerVisual : MonoBehaviour
{
    [SerializeField] private bool debugAlwaysShow;
    [SerializeField] private UICurrencyManager uICurrencyManager;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private BackpackFeedbacks backpackFeedbacks;
    [SerializeField] private List<CurrencyUI_AlmostFullColliders> almostFullColliders;

    public float backpackDisplayTime = 2f;   // Durée d'affichage de la barre
    public float backpackDisplayTimer;   // Durée d'affichage de la barre
    public float fadeOutDuration = .2f;  // Durée du fade-out
    public float fadeInDuration = .2f;  // Durée du fade-in

    private bool backpackAlmostFull;
    private float backpackBarDiplayTimer = 0f;
    private bool isFadingOut = true;
    private bool isFadingIn = false;

    private void Start() {
        uICurrencyManager.OnCurrencyDropped += UICurrencyManager_OnCurrencyDropped;
        uICurrencyManager.OnCurrencyTryPay += UICurrencyManager_OnCurrencyTryPay;
        uICurrencyManager.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        uICurrencyManager.OnCurrencyFailedToDrop += UICurrencyManager_OnCurrencyFailedToDrop;

        PlayerShoot.Instance.OnPlayerShotProjectile += PlayerShoot_OnPlayerShotProjectile;
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
    }

    private void Update() {
        if (debugAlwaysShow) return;

        if (isFadingIn) {
            HandleFadeIn();
            return;
        }

        HandleFadeOut();
    }

    private void UICurrencyManager_OnCurrencyFailedToDrop(object sender, System.EventArgs e) {
        ShowBackpack(2f);
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        ShowBackpack(2f);
    }

    private void UICurrencyManager_OnCurrencyTryPay(object sender, UICurrencyManager.OnCurrencyTryPayEventArgs e) {
        ShowBackpack(2f);
    }

    private void UICurrencyManager_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        ShowBackpack(2f);
    }

    private void ShowBackpack(float displayTime = 1f) {
        canvasGroup.alpha = 1;
        backpackDisplayTimer = displayTime;
        isFadingOut = false;
        CheckBagIsAlmostFull();
    }

    private void HandleFadeIn() {
        // Réduit le timer pour le fade-in
        backpackDisplayTimer -= Time.deltaTime;

        float alpha = Mathf.Clamp01(1 - (backpackDisplayTimer / fadeInDuration));
        canvasGroup.alpha = alpha;

        // Quand le fade-in est terminé
        if (backpackDisplayTimer <= 0f) {
            canvasGroup.alpha = 1f;
            isFadingIn = false;
            backpackDisplayTimer = backpackDisplayTime; // Initialise le timer pour maintenir la barre visible
        }
    }

    private void HandleFadeOut() {

        // Si le timer est en cours et que le fade-out n'a pas commencé
        if (backpackDisplayTimer > 0f && !isFadingOut) {
            backpackDisplayTimer -= Time.deltaTime;

            // Démarre le fade-out lorsque le timer atteint 0
            if (backpackDisplayTimer <= 0f) {
                isFadingOut = true;
                backpackDisplayTimer = fadeOutDuration; // Initialise le timer pour le fade
            }
        }

        // Gestion du fade-out
        if (isFadingOut) {
            float alpha = Mathf.Clamp01(backpackDisplayTimer / fadeOutDuration);
            canvasGroup.alpha = alpha;

            // Réduit le timer pour le fade-out
            backpackDisplayTimer -= Time.deltaTime;

            // Quand le fade-out est terminé
            if (backpackDisplayTimer <= 0f) {
                canvasGroup.alpha = 0;
                isFadingOut = false;
            }
        }
    }

    private void CheckBagIsAlmostFull() {
        bool isAlmostFull = false;

        foreach(CurrencyUI_AlmostFullColliders almostFullCollider in almostFullColliders) {
            if(almostFullCollider.GetBagIsAlmostFull()) {
                isAlmostFull = true;
            }
        }
        backpackAlmostFull = isAlmostFull;
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        if (backpackAlmostFull) {
            //ShowBackpack(2f);
        }
    }

    private void PlayerShoot_OnPlayerShotProjectile(object sender, System.EventArgs e) {
        if (backpackAlmostFull) {
            //ShowBackpack(2f);
        }
    }

}
