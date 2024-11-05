using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUI_AmmoBar : MonoBehaviour
{
    [SerializeField] private GameObject ammoBarGameObject;
    [SerializeField] private GameObject ammoBarBackgroundGameObject;

    [SerializeField] private Transform ammoTickTemplate;
    [SerializeField] private Transform ammoTickContainer;

    [SerializeField] private Transform ammoTickTemplateBackground;
    [SerializeField] private Transform ammoTickContainerBackground;

    private CanvasGroup ammoBarCanvasGroup;
    public float ammoBarDisplayTime = 2f;   // Durée d'affichage de la barre
    public float fadeOutDuration = .2f;  // Durée du fade-out
    public float fadeInDuration = .2f;  // Durée du fade-in

    private float timer = 0f;
    private bool isFadingOut = false;
    private bool isFadingIn = false;
    private bool ammoBarCritical = false;

    private void Start() {
        PlayerShoot.Instance.OnPlayerReload += PlayerShoot_OnPlayerReload;
        RefreshAmmoBar();
        RefreshAmmoBarBackground();
        ammoBarGameObject.SetActive(false);
        ammoBarBackgroundGameObject.SetActive(false);
    }

    private void Update() {

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
        ammoBarCanvasGroup.alpha = alpha;

        // Quand le fade-in est terminé
        if (timer <= 0f) {
            ammoBarCanvasGroup.alpha = 1f;
            isFadingIn = false;
            timer = ammoBarDisplayTime; // Initialise le timer pour maintenir la barre visible
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
            ammoBarCanvasGroup.alpha = alpha;

            // Réduit le timer pour le fade-out
            timer -= Time.deltaTime;

            // Quand le fade-out est terminé
            if (timer <= 0f) {
                ammoBarCanvasGroup.alpha = 0;
                isFadingOut = false;
            }
        }
    }

    private void PlayerShoot_OnPlayerReload(object sender, System.EventArgs e) {
        if (PlayerShoot.Instance.GetCurrentAmmo() < 0) return;

        if (PlayerShoot.Instance.GetCurrentAmmo() != 0) {
            ammoBarGameObject.SetActive(true);
            ammoBarBackgroundGameObject.SetActive(true);
        }

        if (PlayerShoot.Instance.GetCurrentAmmo() <= PlayerShoot.Instance.GetMaxAmmo() / 3) {
            ammoBarCritical = true;
        }

        RectTransform[] hpTickArray = ammoTickContainer.GetComponentsInChildren<RectTransform>();
        hpTickArray[1].SetParent(transform);
        hpTickArray[1].GetComponent<PlayerUI_TickTemplate>().RemoveTick();


        RefreshAmmoBar();
    }

    private void RefreshAmmoBar() {
        ammoTickTemplate.gameObject.SetActive(true);

        foreach (Transform child in ammoTickContainer) {
            if (child == ammoTickTemplate) continue;
            Destroy(child.gameObject);
        }

        int playerAmmo = PlayerShoot.Instance.GetCurrentAmmo();

        for (int i = 0; i < playerAmmo; i++) {
            Instantiate(ammoTickTemplate, ammoTickContainer);
        }

        ammoTickTemplate.gameObject.SetActive(false);
    }

    private void RefreshAmmoBarBackground() {
        ammoTickTemplateBackground.gameObject.SetActive(true);

        foreach (Transform child in ammoTickContainerBackground) {
            if (child == ammoTickTemplateBackground) continue;
            Destroy(child.gameObject);
        }

        int playerAmmo = PlayerShoot.Instance.GetCurrentAmmo();

        for (int i = 0; i < playerAmmo; i++) {
            Instantiate(ammoTickTemplateBackground, ammoTickContainerBackground);
        }

        ammoTickTemplateBackground.gameObject.SetActive(false);
    }
}
