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

    private CanvasGroup hpBarCanvasGroup;
    public float hpBarDisplayTime = 2f;   // Durée d'affichage de la barre
    public float fadeOutDuration = .2f;  // Durée du fade-out
    public float fadeInDuration = .2f;  // Durée du fade-in

    private bool hpBarCritical;
    private float timer = 0f;
    private bool isFadingOut = false;
    private bool isFadingIn = false;

    private void Awake() {
        Instance = this;

        hpBarCanvasGroup = hpBarGameObject.GetComponent<CanvasGroup>(); 
    }

    private void Start() {
        Player.Instance.OnPlayerDamaged += Player_OnPlayerDamaged;
        Player.Instance.OnPlayerHealed += Player_OnPlayerHealed;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
        Player.Instance.OnPlayerEnteredCamp += Player_OnPlayerEnteredCamp;
        Player.Instance.OnPlayerExitedCamp += Player_OnPlayerExitedCamp;

        Tent.Instance.OnPlayerTriggeredIn += Tent_OnPlayerTriggeredIn;

        RefreshHPBar();

        hpBarGameObject.SetActive(false);
    }

    private void Update() {
        if (hpBarCritical) return;
        //if (!showHPBar) return;

        if (isFadingIn) {
            HandleFadeIn();
            return;
        }

        HandleFadeOut();

    }

    private void HandleFadeIn() {
        // Réduit le timer pour le fade-in
        timer -= Time.deltaTime;

        float alpha = Mathf.Clamp01(1 - (timer / fadeInDuration)); 
        hpBarCanvasGroup.alpha = alpha;

        // Quand le fade-in est terminé
        if (timer <= 0f) {
            hpBarCanvasGroup.alpha = 1f;
            isFadingIn = false;
            timer = hpBarDisplayTime; // Initialise le timer pour maintenir la barre visible
        }
    }

    private void HandleFadeOut() {

        // Si le timer est en cours et que le fade-out n'a pas commencé
        if (timer > 0f && !isFadingOut) {
            timer -= Time.deltaTime;

            // Démarre le fade-out lorsque le timer atteint 0
            if (timer <= 0f) {
                isFadingOut = true;
                timer = fadeOutDuration; // Initialise le timer pour le fade
            }
        }

        // Gestion du fade-out
        if (isFadingOut) {
            float alpha = Mathf.Clamp01(timer / fadeOutDuration);
            hpBarCanvasGroup.alpha = alpha;

            // Réduit le timer pour le fade-out
            timer -= Time.deltaTime;

            // Quand le fade-out est terminé
            if (timer <= 0f) {
                hpBarCanvasGroup.alpha = 0;
                isFadingOut = false;
            }
        }
    }

    private void Player_OnPlayerRespawned(object sender, System.EventArgs e) {
        hpBarGameObject.SetActive(true);
        hpBarCritical = false;
        RefreshHPBar();
    }

    private void Player_OnPlayerDied(object sender, System.EventArgs e) {
        hpBarGameObject.SetActive(false);
    }

    private void Player_OnPlayerDamaged(object sender, System.EventArgs e) {
        if (Player.Instance.GetHP() < 0) return;

        if(Player.Instance.GetHP() != 0) {
            hpBarGameObject.SetActive(true);
        }

        if (Player.Instance.GetHP() <= Player.Instance.GetMaxHP() / 3) {
            hpBarCritical = true;
        }

        RectTransform[] hpTickArray = hpTickContainer.GetComponentsInChildren<RectTransform>();
        hpTickArray[hpTickArray.Length - 1].SetParent(transform);
        hpTickArray[hpTickArray.Length - 1].GetComponent<PlayerUI_TickTemplate>().RemoveTick();
        
        ShowHPBar();
        RefreshHPBar();
    }


    private void Player_OnPlayerHealed(object sender, System.EventArgs e) {
        hpBarCritical = true;
        isFadingIn = true;
        ShowHPBar(2f);
        RefreshHPBar();
    }

    private void RefreshHPBar() {
        hpTickTemplate.gameObject.SetActive(true);

        foreach (Transform child in hpTickContainer) {
            if (child == hpTickTemplate) continue;
            Destroy(child.gameObject);
        }

        int playerHP = Player.Instance.GetHP();
        int playerMaxHP = Player.Instance.GetMaxHP();

        for(int i = 0; i < playerHP; i++) {
            Instantiate(hpTickTemplate, hpTickContainer);
        }

        hpTickTemplate.gameObject.SetActive(false);
    }

    private void Player_OnPlayerExitedCamp(object sender, System.EventArgs e) {
        FadeInHPBar();
    }

    private void Player_OnPlayerEnteredCamp(object sender, System.EventArgs e) {
        FadeInHPBar();
    }

    private void Tent_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        FadeInHPBar();
    }

    private void FadeInHPBar() {
        isFadingIn = true;
        hpBarGameObject.SetActive(true);

        timer = fadeInDuration;
        hpBarDisplayTime = 3f;
    }

    private void ShowHPBar(float displayTime = 1f) {
        hpBarCanvasGroup.alpha = 1;
        timer = displayTime;
        isFadingOut = false;
    }
}
