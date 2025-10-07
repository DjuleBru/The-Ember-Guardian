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
    [SerializeField] private Transform ejectedClipsParentTransform;

    [SerializeField] private Transform ammoTickTemplateBackground;
    [SerializeField] private Transform ammoTickContainerBackground;

    [SerializeField] private Image ammoTickTemplateBackgroundRenderer;
    [SerializeField] private Sprite ammoSprite;
    [SerializeField] private Sprite ammoSpecialSprite;

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
    private bool tabMenuOpen = false;
    private bool alwaysDisplay;

    public event EventHandler OnAmmoTickAdded;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        PlayerShoot.Instance.OnPlayerReloadHandEnded += PlayerSHoot_OnPlayerReloadHandEnded;
        PlayerShoot.Instance.OnPlayerAmmoRefilled += PlayerShoot_OnPlayerAmmoRefilled;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
        PlayerShoot.Instance.OnPlayerTryReloadAmmoBelt_NoAmmoInBag += PlayerShoot_OnPlayerTryReloadAmmoBelt_NoAmmoInBag;
        PlayerShoot.Instance.OnPlayerTryReload_FullAmmoBelt += PlayerShoot_OnPlayerTryReload_FullAmmoBelt;
        Player.Instance.OnPlayerEnteredCamp += Player_OnPlayerEnteredCamp;
        Player.Instance.OnPlayerExitedCamp += Player_OnPlayerExitedCamp;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Portal.OnAnyPlayerMovedOnTeleporter += Portal_OnAnyPlayerMovedOnTeleporter;
        Portal.OnAnyPlayerTeleported += Portal_OnAnyPlayerTeleported;
        Portal.OnAnyTeleporterTeleportedPlayerOut += Portal_OnAnyTeleporterTeleportedPlayerOut;

        Structure.OnAnyPlayerTriggeredIn += Structure_OnAnyPlayerTriggeredIn;
        Structure.OnAnyPlayerTriggeredOut += Structure_OnAnyPlayerTriggeredOut;

        Gun.OnAnyGunMaxAmmoChanged += Gun_OnAnyGunMaxAmmoChanged;

        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Tutorial) {
            PlayerTabMenuUI.Instance.OnPlayerTabClosed += PlayerTabMenuUI_OnPlayerTabClosed;
            PlayerTabMenuUI.Instance.OnPlayerTabOpened += PLayerTabMenuUI_OnPlayerTabOpened;
        }

        RefreshAmmoBar();
        RefreshAmmoBarBackground();
        ammoBarGameObject.SetActive(false);
        ammoBarBackgroundGameObject.SetActive(false);
        ammoBarCanvasGroup = ammoBarGameObject.GetComponent<CanvasGroup>();

        RefreshAlwaysDisplay();
        SettingsManager.Instance.OnUIDisplayChanged += SettingsManager_OnUIDisplayChanged;
    }

    private void Portal_OnAnyTeleporterTeleportedPlayerOut(object sender, EventArgs e) {
        if (alwaysDisplay) {
            ammoBarCanvasGroup.alpha = 1f;
            ammoBarGameObject.SetActive(true);
            RefreshAmmoBar();
            RefreshAmmoBarBackground();
        }
        else {
            ammoBarCanvasGroup.alpha = 0f;
        }
    }

    private void SettingsManager_OnUIDisplayChanged(object sender, System.EventArgs e) {
        RefreshAlwaysDisplay();
    }

    private void RefreshAlwaysDisplay() {
        alwaysDisplay = SettingsManager.Instance.GetCurrentUIDisplayType() == SettingsManager.UIDisplayType.Persistent;

    }


    private void PlayerShoot_OnPlayerTryReload_FullAmmoBelt(object sender, EventArgs e) {
        ForceShowAmmoBar(ammoBarReloadDisplayTime);
    }

    private void PlayerShoot_OnPlayerTryReloadAmmoBelt_NoAmmoInBag(object sender, EventArgs e) {
        ForceShowAmmoBar(ammoBarReloadDisplayTime);
    }

    private void PLayerTabMenuUI_OnPlayerTabOpened(object sender, System.EventArgs e) {
        tabMenuOpen = true;
        isFadingIn = true;
    }

    private void PlayerTabMenuUI_OnPlayerTabClosed(object sender, System.EventArgs e) {
        tabMenuOpen = false;
        isFadingOut = true;
        ammoBarDisplayTimer = fadeOutDuration; // Initialise le timer pour le fade
    }

    private void Player_OnPlayerDied(object sender, EventArgs e) {
        ammoBarCanvasGroup.alpha = 0;
    }

    private void Portal_OnAnyPlayerTeleported(object sender, EventArgs e) {
        ammoBarCanvasGroup.alpha = 0;
    }

    private void Portal_OnAnyPlayerMovedOnTeleporter(object sender, EventArgs e) {
        ammoBarCanvasGroup.alpha = 0;
    }
    private void Gun_OnAnyGunMaxAmmoChanged(object sender, EventArgs e) {
        RefreshAmmoBar();
        RefreshAmmoBarBackground();
    }

    private void Update() {
        if (Player.Instance.GetDead()) return;
        if (alwaysDisplay) return;

        if (isFadingIn) {
            HandleFadeIn();
            return;
        }

        if (ammoBarCritical) return;
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
        ForceShowAmmoBar(ammoBarReloadDisplayTime);
        StartCoroutine(RefillAmmoBar(e.ammoAmount));

        if (PlayerShoot.Instance.GetCurrentAmmoClip() <= PlayerShoot.Instance.GetMaxAmmoClips() / 3) {
            ammoBarCritical = true;
            ammoBarCanvasGroup.alpha = 1f;
        } else {
            ammoBarCritical = false;
        }
    }


    private void PlayerShoot_OnPlayerReload(object sender, PlayerShoot.OnPlayerReloadEventArgs e) {
        if (PlayerShoot.Instance.GetCurrentAmmoClip() < 0) return;

        // Affiche directement la barre (ignore tout fade-out en cours)
        ForceShowAmmoBar(ammoBarReloadDisplayTime);

        // Gestion du mode critique si faible en munitions
        if (PlayerShoot.Instance.GetCurrentAmmoClip() <= PlayerShoot.Instance.GetMaxAmmoClips() / 3) {
            ammoBarCritical = true;
            ammoBarCanvasGroup.alpha = 1f;
        }
        else {
            ammoBarCritical = false;
        }

        // Retrait du tick (correspond à la munition utilisée dans la main)
        PlayerUI_TickTemplate[] ammoTickArray = ammoTickContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
        if (ammoTickArray.Length > 0) {
            ammoTickArray[0].GetComponent<RectTransform>().SetParent(transform);

            float tickForceYMultiplier = 1f;
            if (e.surgeReload) {
                tickForceYMultiplier = 1.3f;
            }
            ammoTickArray[0].RemoveTick(tickForceYMultiplier, true, 1, 0, e.surgeReload, false, 1.5f, false);
            ammoTickArray[0].transform.SetParent(ejectedClipsParentTransform);
        }

        // Rafraîchit l’affichage des ticks
        RefreshAmmoBar();
    }

    private void PlayerSHoot_OnPlayerReloadHandEnded(object sender, System.EventArgs e) {
        
    }

    private IEnumerator RefillAmmoBar(int ammoCount) {
        for (int i = 0; i < ammoCount; i++) {

            
            PlayerUI_TickTemplate ammoTick = Instantiate(ammoTickTemplate, ammoTickContainer).GetComponent<PlayerUI_TickTemplate>();

            PlayerCurrencies.CurrencyType ammoType = PlayerShoot.Instance.GetCurrentAmmoType();
            RectTransform rt = ammoTick.GetComponent<RectTransform>();
            if (ammoType == PlayerCurrencies.CurrencyType.ammo) {
                ammoTick.SetImageSprite(ammoSprite);
                rt.sizeDelta = new Vector2(.3f, .1f);
            }
            if (ammoType == PlayerCurrencies.CurrencyType.ammo_special) {
                ammoTick.SetImageSprite(ammoSpecialSprite);
                rt.sizeDelta = new Vector2(.3f, .15f);
            }

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
        PlayerCurrencies.CurrencyType ammoType = PlayerShoot.Instance.GetCurrentAmmoType();

        for (int i = 0; i < playerAmmo; i++) {
            PlayerUI_TickTemplate ammoTick = Instantiate(ammoTickTemplate, ammoTickContainer).GetComponent<PlayerUI_TickTemplate>();
            RectTransform rt = ammoTick.GetComponent<RectTransform>();
            ammoTick.SetImageAlphaFull();
            if (ammoType == PlayerCurrencies.CurrencyType.ammo) {
                ammoTick.SetImageSprite(ammoSprite);
                rt.sizeDelta = new Vector2(.3f, .1f);
            }
            if (ammoType == PlayerCurrencies.CurrencyType.ammo_special) {
                ammoTick.SetImageSprite(ammoSpecialSprite);
                rt.sizeDelta = new Vector2(.3f, .15f);
            }
        }

        
        ammoTickTemplate.gameObject.SetActive(false);
    }

    private void RefreshAmmoBarBackground() {
        ammoTickTemplateBackground.gameObject.SetActive(true);

        PlayerCurrencies.CurrencyType ammoType = PlayerShoot.Instance.GetCurrentAmmoType();
        RectTransform rt = ammoTickTemplateBackground.GetComponent<RectTransform>();

        if (ammoType == PlayerCurrencies.CurrencyType.ammo) {
            ammoTickTemplateBackgroundRenderer.sprite = ammoSprite;
            rt.sizeDelta = new Vector2(.3f, .1f);
        }
        if (ammoType == PlayerCurrencies.CurrencyType.ammo_special) {
            ammoTickTemplateBackgroundRenderer.sprite = ammoSpecialSprite;
            rt.sizeDelta = new Vector2(.3f, .15f);
        }

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
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        FadeInAmmoBar();
    }

    private void Player_OnPlayerEnteredCamp(object sender, System.EventArgs e) {
        if (!Player.Instance.GetPlayerControlInputsEnabled()) return;
        if (Player.Instance.GetDead()) return;
        FadeInAmmoBar();
    }

    private void Structure_OnAnyPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (sender is CurrencyCrafter) {
            inAmmoCrafterArea = false;
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
    }
    private void ForceShowAmmoBar(float displayDuration = 2f) {
        ammoBarGameObject.SetActive(true);
        ammoBarBackgroundGameObject.SetActive(true);
        ammoBarCanvasGroup.alpha = 1f;
        isFadingIn = false;
        isFadingOut = false;
        ammoBarDisplayTime = displayDuration;
        ammoBarDisplayTimer = ammoBarDisplayTime;
    }

    private void OnDestroy() {
        PlayerShoot.Instance.OnPlayerReloadHandEnded -= PlayerSHoot_OnPlayerReloadHandEnded;
        PlayerShoot.Instance.OnPlayerAmmoRefilled -= PlayerShoot_OnPlayerAmmoRefilled;
        PlayerShoot.Instance.OnPlayerSwappedGun -= PlayerShoot_OnPlayerSwappedGun;
        Player.Instance.OnPlayerEnteredCamp -= Player_OnPlayerEnteredCamp;
        Player.Instance.OnPlayerExitedCamp -= Player_OnPlayerExitedCamp;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;

        Portal.OnAnyPlayerMovedOnTeleporter -= Portal_OnAnyPlayerMovedOnTeleporter;
        Portal.OnAnyPlayerTeleported -= Portal_OnAnyPlayerTeleported;
        Portal.OnAnyTeleporterTeleportedPlayerOut -= Portal_OnAnyTeleporterTeleportedPlayerOut;

        Structure.OnAnyPlayerTriggeredIn -= Structure_OnAnyPlayerTriggeredIn;
        Structure.OnAnyPlayerTriggeredOut -= Structure_OnAnyPlayerTriggeredOut;

        Gun.OnAnyGunMaxAmmoChanged -= Gun_OnAnyGunMaxAmmoChanged;


        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Tutorial) {
            PlayerTabMenuUI.Instance.OnPlayerTabClosed -= PlayerTabMenuUI_OnPlayerTabClosed;
            PlayerTabMenuUI.Instance.OnPlayerTabOpened -= PLayerTabMenuUI_OnPlayerTabOpened;
        }
    }
}
