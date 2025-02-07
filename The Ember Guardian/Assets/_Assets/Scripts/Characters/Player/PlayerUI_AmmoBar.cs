using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_AmmoBar : MonoBehaviour
{

    public static PlayerUI_AmmoBar Instance;

    [SerializeField] private GameObject ammoBarGameObject;
    [SerializeField] private GameObject ammoBarBackgroundGameObject;
    [SerializeField] private RectTransform ammoBarRightPosition;
    [SerializeField] private RectTransform ammoBarLeftPosition;

    [SerializeField] private Transform ammoTickTemplate;
    [SerializeField] private Transform ammoTickContainer;

    [SerializeField] private Transform ammoTickTemplateBackground;
    [SerializeField] private Transform ammoTickContainerBackground;

    private CanvasGroup ammoBarCanvasGroup;
    private float ammoBarDisplayTime;   // Durée d'affichage de la barre
    private float ammoBarReloadDisplayTime = 2f;   // Durée d'affichage de la barre
    private float ammoBarExitCampDisplayTime = 3f;   // Durée d'affichage de la barre
    private float fadeOutDuration = .2f;  // Durée du fade-out
    private float fadeInDuration = .2f;  // Durée du fade-in

    private float ammoBarDisplayTimer = 0f;
    private bool isFadingOut = false;
    private bool isFadingIn = false;
    private bool ammoBarCritical = false;
    private bool inAmmoCrafterArea = false;

    public event EventHandler OnAmmoTickAdded;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        PlayerShoot.Instance.OnPlayerReloadHandEnded += PlayerSHoot_OnPlayerReloadHandEnded;
        PlayerShoot.Instance.OnPlayerAmmoRefilled += PlayerShoot_OnPlayerAmmoRefilled;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        Player.Instance.OnPlayerEnteredCamp += Player_OnPlayerEnteredCamp;
        Player.Instance.OnPlayerExitedCamp += Player_OnPlayerExitedCamp;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;

        Structure.OnAnyPlayerTriggeredIn += Structure_OnAnyPlayerTriggeredIn;
        Structure.OnAnyPlayerTriggeredOut += Structure_OnAnyPlayerTriggeredOut;

        Gun.OnAnyGunMaxAmmoChanged += Gun_OnAnyGunMaxAmmoChanged;

        RefreshAmmoBar();
        RefreshAmmoBarBackground();
        ammoBarGameObject.SetActive(false);
        ammoBarBackgroundGameObject.SetActive(false);
        ammoBarCanvasGroup = ammoBarGameObject.GetComponent<CanvasGroup>();
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        ammoBarCanvasGroup.alpha = 0;
    }

    private void Gun_OnAnyGunMaxAmmoChanged(object sender, EventArgs e) {
        RefreshAmmoBar();
        RefreshAmmoBarBackground();
    }

    private void Update() {
        if (ammoBarCritical) return;
        if (Player.Instance.GetDead()) return;

        if (isFadingIn) {
            HandleFadeIn();
            return;
        }

        if (inAmmoCrafterArea) return;

        HandleFadeOut();
    }

    private void HandleFadeIn() {
        // Réduit le timer pour le fade-in
        ammoBarDisplayTimer -= Time.deltaTime;

        float alpha = Mathf.Clamp01(1 - (ammoBarDisplayTimer / fadeInDuration));
        ammoBarCanvasGroup.alpha = alpha;

        // Quand le fade-in est terminé
        if (ammoBarDisplayTimer <= 0f) {
            ammoBarCanvasGroup.alpha = 1f;
            isFadingIn = false;
            ammoBarDisplayTimer = ammoBarDisplayTime; // Initialise le timer pour maintenir la barre visible
        }
    }

    private void HandleFadeOut() {

        // Si le timer est en cours et que le fade-out n'a pas commencé
        if (ammoBarDisplayTimer > 0f && !isFadingOut) {
            ammoBarDisplayTimer -= Time.deltaTime;

            // Démarre le fade-out lorsque le timer atteint 0
            if (ammoBarDisplayTimer <= 0f) {
                isFadingOut = true;
                ammoBarDisplayTimer = fadeOutDuration; // Initialise le timer pour le fade
            }
        }

        // Gestion du fade-out
        if (isFadingOut) {
            float alpha = Mathf.Clamp01(ammoBarDisplayTimer / fadeOutDuration);
            ammoBarCanvasGroup.alpha = alpha;

            // Réduit le timer pour le fade-out
            ammoBarDisplayTimer -= Time.deltaTime;

            // Quand le fade-out est terminé
            if (ammoBarDisplayTimer <= 0f) {
                ammoBarCanvasGroup.alpha = 0;
                isFadingOut = false;
            }
        }
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, EventArgs e) {
        RefreshAmmoBar();
        RefreshAmmoBarBackground();
        FadeInAmmoBar();
    }

    private void PlayerShoot_OnPlayerAmmoRefilled(object sender, PlayerShoot.OnAmmoRefilledEventArgs e) {
        FadeInAmmoBar();
        StartCoroutine(RefillAmmoBar(e.ammoAmount));

        if (PlayerShoot.Instance.GetCurrentAmmoClip() <= PlayerShoot.Instance.GetMaxAmmoClips() / 3) {
            ammoBarCritical = true;
            ammoBarCanvasGroup.alpha = 1f;
        } else {
            ammoBarCritical = false;
        }
    }

    private void PlayerSHoot_OnPlayerReloadHandEnded(object sender, EventArgs e) {
        if (PlayerShoot.Instance.GetCurrentAmmoClip() < 0) return;

        if (PlayerShoot.Instance.GetCurrentAmmoClip() != 0) {
            ammoBarGameObject.SetActive(true);
            ammoBarBackgroundGameObject.SetActive(true);
            ammoBarCanvasGroup.alpha = 1f;
            ammoBarDisplayTime = ammoBarReloadDisplayTime;
            ammoBarDisplayTimer = ammoBarDisplayTime;
        }

        if (PlayerShoot.Instance.GetCurrentAmmoClip() <= PlayerShoot.Instance.GetMaxAmmoClips() / 3) {
            ammoBarCritical = true;
            ammoBarCanvasGroup.alpha = 1f;
        }
        else {
            ammoBarCritical = false;
        }

        PlayerUI_TickTemplate[] ammoTickArray = ammoTickContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
        ammoTickArray[0].GetComponent<RectTransform>().SetParent(transform);
        ammoTickArray[0].RemoveTick();

        RefreshAmmoBar();
    }


    private IEnumerator RefillAmmoBar(int ammoCount) {
        for (int i = 0; i < ammoCount; i++) {

            PlayerUI_TickTemplate ammoTick = Instantiate(ammoTickTemplate, ammoTickContainer).GetComponent<PlayerUI_TickTemplate>();

            ammoTick.gameObject.SetActive(true);
            PlayerUI_TickTemplate[] ammoTickArray = ammoTickContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
            ammoTickArray[0].AddTick();
            OnAmmoTickAdded?.Invoke(this, EventArgs.Empty);

            yield return new WaitForSeconds(.25f);
        }

    }

    public void RefreshAmmoBar() {
        ammoTickTemplate.gameObject.SetActive(true);

        foreach (Transform child in ammoTickContainer) {
            if (child == ammoTickTemplate) continue;
            Destroy(child.gameObject);
        }

        int playerAmmo = PlayerShoot.Instance.GetCurrentAmmoClip();
        for (int i = 0; i < playerAmmo; i++) {
            PlayerUI_TickTemplate ammoTick = Instantiate(ammoTickTemplate, ammoTickContainer).GetComponent<PlayerUI_TickTemplate>();
            ammoTick.SetImageAlphaFull();
        }

        ammoTickTemplate.gameObject.SetActive(false);
    }

    private void RefreshAmmoBarBackground() {
        ammoTickTemplateBackground.gameObject.SetActive(true);

        foreach (Transform child in ammoTickContainerBackground) {
            if (child == ammoTickTemplateBackground) continue;
            Destroy(child.gameObject);
        }

        int playerAmmo = PlayerShoot.Instance.GetMaxAmmoClips();

        for (int i = 0; i < playerAmmo; i++) {
            Instantiate(ammoTickTemplateBackground, ammoTickContainerBackground);
        }

        ammoTickTemplateBackground.gameObject.SetActive(false);
    }

    private void Player_OnPlayerExitedCamp(object sender, System.EventArgs e) {
        FadeInAmmoBar();
    }

    private void Player_OnPlayerEnteredCamp(object sender, System.EventArgs e) {
        if (Player.Instance.GetDead()) return;
        FadeInAmmoBar();
    }

    private void Structure_OnAnyPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (sender is CurrencyCrafter) {
            inAmmoCrafterArea = false;
            FadeInAmmoBar();
        }
    }

    private void Structure_OnAnyPlayerTriggeredIn(object sender, System.EventArgs e) {
        if (Player.Instance.GetDead()) return;

        if (sender is CurrencyCrafter) {
            if ((sender as CurrencyCrafter).GetCurrencyTypeCrafted() != PlayerCurrencies.CurrencyType.ammo) return;
            inAmmoCrafterArea = true;
            FadeInAmmoBar();
        }
    }

    private void FadeInAmmoBar() {

        ammoBarDisplayTime = ammoBarExitCampDisplayTime;

        if (ammoBarDisplayTimer <= 0) {
            isFadingIn = true;
            ammoBarDisplayTimer = fadeInDuration;
        } else {
            ammoBarDisplayTimer = ammoBarDisplayTime;
        }

        ammoBarGameObject.SetActive(true);
        ammoBarBackgroundGameObject.SetActive(true);

        Debug.Log(ammoBarDisplayTimer);
    }

    private void OnDestroy() {
        PlayerShoot.Instance.OnPlayerReloadHandEnded -= PlayerSHoot_OnPlayerReloadHandEnded;
        PlayerShoot.Instance.OnPlayerAmmoRefilled -= PlayerShoot_OnPlayerAmmoRefilled;
        PlayerShoot.Instance.OnPlayerSwappedGun -= PlayerShoot_OnPlayerSwappedGun;
        Player.Instance.OnPlayerEnteredCamp -= Player_OnPlayerEnteredCamp;
        Player.Instance.OnPlayerExitedCamp -= Player_OnPlayerExitedCamp;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;

        Structure.OnAnyPlayerTriggeredIn -= Structure_OnAnyPlayerTriggeredIn;
        Structure.OnAnyPlayerTriggeredOut -= Structure_OnAnyPlayerTriggeredOut;

        Gun.OnAnyGunMaxAmmoChanged -= Gun_OnAnyGunMaxAmmoChanged;

    }
}
