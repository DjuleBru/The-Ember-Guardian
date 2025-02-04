using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerUI_HPBar : MonoBehaviour
{
    public static PlayerUI_HPBar Instance;

    [SerializeField] private GameObject hpBarGameObject;
    [SerializeField] private Transform hpTickTemplate;
    [SerializeField] private Transform hpTickContainer;
    private float tickWidth = .15f;
    private float sidesWidth = .5f;

    private CanvasGroup hpBarCanvasGroup;
    public float hpBarDisplayTime = 2f;   // Durée d'affichage de la barre
    public float fadeOutDuration = .2f;  // Durée du fade-out
    public float fadeInDuration = .2f;  // Durée du fade-in

    private bool hpBarCritical;
    private float hpBarDiplayTimer = 0f;
    private bool isFadingOut = false;
    private bool isFadingIn = false;
    private bool inTentArea = false;

    public event EventHandler OnHPTickAdded;

    private void Awake() {
        Instance = this;

        hpBarCanvasGroup = hpBarGameObject.GetComponent<CanvasGroup>(); 
    }

    private void Start() {
        if (SceneLoader.Instance == null) return;
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB) {
            hpBarGameObject.SetActive(false);
            return;
        }

        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        Player.Instance.OnPlayerHealed += Player_OnPlayerHealed;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
        Player.Instance.OnPlayerEnteredCamp += Player_OnPlayerEnteredCamp;
        Player.Instance.OnPlayerExitedCamp += Player_OnPlayerExitedCamp;
        PlayerStats.Instance.OnPlayerMaxHPChanged += PlayerStats_OnPlayerMaxHPChanged;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        RefreshHPBar();

        hpBarGameObject.SetActive(false);
    }

    private void Fire_OnInitialFireActivated(object sender, EventArgs e) {

        Tent.Instance.OnPlayerTriggeredIn += Tent_OnPlayerTriggeredIn;
        Tent.Instance.OnPlayerTriggeredOut += Tent_OnPlayerTriggeredOut;

    }

    private void Update() {
        if (hpBarCritical) return;
        if (Player.Instance.GetDead()) return;

        if (isFadingIn) {
            HandleFadeIn();
            return;
        }

        if (inTentArea) return;
        HandleFadeOut();

    }

    private void HandleFadeIn() {
        // Réduit le timer pour le fade-in
        hpBarDiplayTimer -= Time.deltaTime;

        float alpha = Mathf.Clamp01(1 - (hpBarDiplayTimer / fadeInDuration)); 
        hpBarCanvasGroup.alpha = alpha;

        // Quand le fade-in est terminé
        if (hpBarDiplayTimer <= 0f) {
            hpBarCanvasGroup.alpha = 1f;
            isFadingIn = false;
            hpBarDiplayTimer = hpBarDisplayTime; // Initialise le timer pour maintenir la barre visible
        }
    }

    private void HandleFadeOut() {

        // Si le timer est en cours et que le fade-out n'a pas commencé
        if (hpBarDiplayTimer > 0f && !isFadingOut) {
            hpBarDiplayTimer -= Time.deltaTime;

            // Démarre le fade-out lorsque le timer atteint 0
            if (hpBarDiplayTimer <= 0f) {
                isFadingOut = true;
                hpBarDiplayTimer = fadeOutDuration; // Initialise le timer pour le fade
            }
        }

        // Gestion du fade-out
        if (isFadingOut) {
            float alpha = Mathf.Clamp01(hpBarDiplayTimer / fadeOutDuration);
            hpBarCanvasGroup.alpha = alpha;

            // Réduit le timer pour le fade-out
            hpBarDiplayTimer -= Time.deltaTime;

            // Quand le fade-out est terminé
            if (hpBarDiplayTimer <= 0f) {
                hpBarCanvasGroup.alpha = 0;
                isFadingOut = false;
            }
        }
    }

    private void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        StartCoroutine(ShowHPBarAfterDelay(3f));
        hpBarCritical = false;
        RefreshHPBar();
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        hpBarGameObject.SetActive(false);
    }

    private void Player_OnPlayerDamaged(object sender, Player.OnPlayerChangedHealthEventArgs e) {
        if (Player.Instance.GetHP() < 0) return;

        if(Player.Instance.GetHP() != 0) {
            hpBarGameObject.SetActive(true);
        }

        if (Player.Instance.GetHP() <= PlayerStats.Instance.GetMaxHP() / 3) {
            hpBarCritical = true;
        }

        PlayerUI_TickTemplate[] hpTickArray = hpTickContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
        
        int damageTaken = e.hpChangeAmount;

        for(int  i = 1; i <= damageTaken; i++) {
            hpTickArray[hpTickArray.Length - i].GetComponent<RectTransform>().SetParent(transform);
            hpTickArray[hpTickArray.Length - i].RemoveTick();
        }
        
        ShowHPBar();
        RefreshHPBar();
    }

    private void Player_OnPlayerHealed(object sender, Player.OnPlayerChangedHealthEventArgs e) {
        isFadingIn = true;
        hpBarDiplayTimer = 0;
        StartCoroutine(RefillHPBar(e.hpChangeAmount));
    }

    private void PlayerStats_OnPlayerMaxHPChanged(object sender, EventArgs e) {
        RefreshHPBar();
    }

    private IEnumerator RefillHPBar(int hpCount) {

        for (int i = 0; i < hpCount; i++) {

            PlayerUI_TickTemplate hpTick = Instantiate(hpTickTemplate, hpTickContainer).GetComponent<PlayerUI_TickTemplate>();

            hpTick.gameObject.SetActive(true);
            PlayerUI_TickTemplate[] hpTickArray = hpTickContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();

            hpTickArray[hpTickArray.Length - 1].AddTick();
            OnHPTickAdded?.Invoke(this, EventArgs.Empty);

            yield return new WaitForSeconds(.25f);
        }

    }

    private void RefreshHPBar() {
        RefreshHPBarSize();
        hpTickTemplate.gameObject.SetActive(true);

        foreach (Transform child in hpTickContainer) {
            if (child == hpTickTemplate) continue;
            Destroy(child.gameObject);
        }

        int playerHP = Player.Instance.GetHP();
        int playerMaxHP = PlayerStats.Instance.GetMaxHP();

        for(int i = 0; i < playerHP; i++) {
            Instantiate(hpTickTemplate, hpTickContainer);
        }

        hpTickTemplate.gameObject.SetActive(false);
    }

    private void RefreshHPBarSize() {
        RectTransform rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(sidesWidth + tickWidth * PlayerStats.Instance.GetMaxHP(), .4f);
    }

    private void Player_OnPlayerExitedCamp(object sender, System.EventArgs e) {
        FadeInHPBar();
    }

    private void Player_OnPlayerEnteredCamp(object sender, System.EventArgs e) {
        FadeInHPBar();
    }

    private void Tent_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        inTentArea = true;
        FadeInHPBar();
    }
    private void Tent_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        inTentArea = false;
        FadeInHPBar();
    }

    private void FadeInHPBar() {
        if(hpBarDiplayTimer <= 0) {
            isFadingIn = true;
        }

        hpBarGameObject.SetActive(true);
        hpBarDiplayTimer = fadeInDuration;
        hpBarDisplayTime = 3f;
    }

    private void ShowHPBar(float displayTime = 1f) {
        hpBarCanvasGroup.alpha = 1;
        hpBarDiplayTimer = displayTime;
        isFadingOut = false;
    }

    private IEnumerator ShowHPBarAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        ShowHPBar(2f);
    }
}
