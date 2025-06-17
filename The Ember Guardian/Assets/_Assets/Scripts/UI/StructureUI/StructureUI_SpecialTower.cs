using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureUI_SpecialTower : StructureUI
{
    [SerializeField] private SpecialTower specialTower;

    [SerializeField] private Transform ammoTickContainer;
    [SerializeField] private Transform ammoTickTemplate;
    [SerializeField] private CanvasGroup ammoBarCanvasGroup;

    [SerializeField] private Transform ammoTickContainerBackground;
    [SerializeField] private Transform ammoTickTemplateBackground;
    [SerializeField] private Image ammoTickTemplateBackgroundRenderer;

    [SerializeField] private Sprite ammoSprite;
    [SerializeField] private Sprite ammoSpecialSprite;

    private float ammoBarDisplayTime;   // Durée d'affichage de la barre
    private float ammoBarReloadDisplayTime = 2f;   // Durée d'affichage de la barre
    private float fadeOutDuration = .2f;  // Durée du fade-out
    private float fadeInDuration = .1f;  // Durée du fade-in

    private float ammoBarDisplayTimer = 0f;
    private bool isFadingOut = false;
    private bool isFadingIn = false;

    protected override void Awake() {
        base.Awake();
        specialTower.OnAmmoClipAdded += SpecialTower_OnAmmoClipAdded;
        specialTower.OnAmmoClipRemoved += SpecialTower_OnAmmoClipRemoved;
        payOrbsUI.SetOrbTemplateUIList(RecomposePayOrbsUIList(functionPayOrbsUIList));
    }

    protected override void Start() {
        base.Start();

        ammoTickTemplate.gameObject.SetActive(false);
        RefreshAmmoBarBackground();
    }

    private void Update() {

        if (isFadingIn) {
            HandleFadeIn();
            return;
        }

        if (uiActive) return;
        HandleFadeOut();
    }

    private void SpecialTower_OnAmmoClipRemoved(object sender, System.EventArgs e) {
        RemoveAmmoTick();
    }

    private void SpecialTower_OnAmmoClipAdded(object sender, System.EventArgs e) {
        FadeInAmmoBar();
        AddAmmoTick();
    }

    protected override void SetUIActive(bool active) {
        base.SetUIActive(active);
        if(active) {
            FadeInAmmoBar();
        }
    }

    private void AddAmmoTick() {
        PlayerUI_TickTemplate ammoTick = Instantiate(ammoTickTemplate, ammoTickContainer).GetComponent<PlayerUI_TickTemplate>();
        ammoTick.gameObject.SetActive(true);
        PlayerCurrencies.CurrencyType ammoType = PlayerShoot.Instance.GetCurrentAmmoType();
        RectTransform rt = ammoTick.GetComponent<RectTransform>();
        if (ammoType == PlayerCurrencies.CurrencyType.ammo) {
            ammoTick.SetImageSprite(ammoSprite);
            rt.sizeDelta = new Vector2(.2f, .5f);
        }
        if (ammoType == PlayerCurrencies.CurrencyType.ammo_special) {
            ammoTick.SetImageSprite(ammoSpecialSprite);
            rt.sizeDelta = new Vector2(.25f, .5f);
        }

        ammoTick.gameObject.SetActive(true);
        PlayerUI_TickTemplate[] ammoTickArray = ammoTickContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
        ammoTickArray[0].AddTick();
    }

    [Button]
    public void RemoveAmmoTick() {
        FadeInAmmoBar();

        if (specialTower.GetCurrentAmmoClip() < 0) return;

        PlayerUI_TickTemplate[] ammoTickArray = ammoTickContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
        ammoTickArray[0].GetComponent<RectTransform>().SetParent(transform);
        ammoTickArray[0].RemoveTick();
    }
    private void RefreshAmmoBarBackground() {
        ammoTickTemplateBackground.gameObject.SetActive(true);

        PlayerCurrencies.CurrencyType ammoType = specialTower.GetRefillCurrencyTypeNeeded();
        RectTransform rt = ammoTickTemplateBackground.GetComponent<RectTransform>();

        if (ammoType == PlayerCurrencies.CurrencyType.ammo) {
            ammoTickTemplateBackgroundRenderer.sprite = ammoSprite;
            rt.sizeDelta = new Vector2(.2f, .5f);
        }
        if (ammoType == PlayerCurrencies.CurrencyType.ammo_special) {
            ammoTickTemplateBackgroundRenderer.sprite = ammoSpecialSprite;
            rt.sizeDelta = new Vector2(.25f, .5f);
        }

        foreach (Transform child in ammoTickContainerBackground) {
            if (child == ammoTickTemplateBackground) continue;
            Destroy(child.gameObject);
        }

        int towerMaxAmmo = specialTower.GetMaxAmmoClips();

        for (int i = 0; i < towerMaxAmmo; i++) {
            Instantiate(ammoTickTemplateBackground, ammoTickContainerBackground);
        }

        ammoTickTemplateBackground.gameObject.SetActive(false);
    }
    private void FadeInAmmoBar() {
        ammoBarDisplayTime = ammoBarReloadDisplayTime;

        if (ammoBarDisplayTimer <= 0) {
            isFadingIn = true;
            ammoBarDisplayTimer = fadeInDuration;
        }
        else {
            ammoBarDisplayTimer = ammoBarDisplayTime;
        }
    }
    private void HandleFadeIn() {
        // Réduit le timer pour le fade-in
        ammoBarDisplayTimer -= Time.deltaTime;

        float alpha = Mathf.Clamp01(1 - (ammoBarDisplayTimer / fadeInDuration));
        ammoBarCanvasGroup.alpha = alpha;
        //Debug.Log(1 - (ammoBarDisplayTimer / fadeInDuration));

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
}
