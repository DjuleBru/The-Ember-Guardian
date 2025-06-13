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
    private float ammoBarExitCampDisplayTime = 3f;   // Durée d'affichage de la barre
    private float fadeOutDuration = .2f;  // Durée du fade-out
    private float fadeInDuration = .2f;  // Durée du fade-in

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

    private void SpecialTower_OnAmmoClipRemoved(object sender, System.EventArgs e) {
        FadeInAmmoBar();
        RemoveAmmoTick();
    }

    private void SpecialTower_OnAmmoClipAdded(object sender, System.EventArgs e) {
        FadeInAmmoBar();
        AddAmmoTick();
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

    private void RemoveAmmoTick() {
        if (specialTower.GetCurrentAmmoClip() < 0) return;

        if (specialTower.GetCurrentAmmoClip() != 0) {
            ammoBarCanvasGroup.alpha = 1f;
            ammoBarDisplayTime = ammoBarReloadDisplayTime;
            ammoBarDisplayTimer = ammoBarDisplayTime;
        }

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

        ammoBarDisplayTime = ammoBarExitCampDisplayTime;

        if (ammoBarDisplayTimer <= 0) {
            isFadingIn = true;
            ammoBarDisplayTimer = fadeInDuration;
        }
        else {
            ammoBarDisplayTimer = ammoBarDisplayTime;
        }
    }

}
