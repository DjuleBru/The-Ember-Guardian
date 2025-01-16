using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUI_Fire : MonoBehaviour
{
    [SerializeField] private GameObject fireUIGameObject;

    [SerializeField] private Animator fireAnimator;
    [SerializeField] private RectTransform progressBarContainer;
    [SerializeField] private RectTransform progressBarTemplate;
    [SerializeField] private RectTransform progressBarBackgroundContainer;
    [SerializeField] private RectTransform progressBarBackgroundTemplate;

    private CanvasGroup fireUICanvasGroup;

    private float displayDuration = 3f; // Durée pendant laquelle le progressBar est visible avant le fade out
    private float fadeOutDuration = 1f; // Durée de la transition de fade out
    private float displayTimer;
    private bool isDisplaying;
    private bool isFadingOut;
    private float fadeOutTimer;
    private float minDistanceToFireToShowUI = 18f;

    private void Awake() {
        fireUICanvasGroup = fireUIGameObject.GetComponent<CanvasGroup>();
    }

    private void Start() {
        StructureUI_Fire.OnFireMaxBarAmountChanged += StructureUI_Fire_OnFireMaxBarAmountChanged;
        StructureUI_Fire.OnFireTickRemoved += StructureUI_Fire_OnFireTickRemoved;
        Fire.Instance.OnFireFuelled += Fire_OnFireFuelled;
        Fire.Instance.OnFireChangedState += Fire_OnFireChangedState;

        fireUIGameObject.SetActive(false);
    }

    private void Update() {
        if (isDisplaying) {
            displayTimer += Time.deltaTime;

            if (displayTimer >= displayDuration) {
                // Fin de l'affichage, commencer le fade-out

                isDisplaying = false;
                isFadingOut = true;
                fadeOutTimer = 0f; // Réinitialiser le timer pour le fade-out
            }
        }
        else if (isFadingOut) {

            fadeOutTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeOutTimer / fadeOutDuration);
            fireUICanvasGroup.alpha = alpha;

            // Vérifier si le fade-out est terminé
            if (fadeOutTimer >= fadeOutDuration) {
                isFadingOut = false;
                fireUIGameObject.SetActive(false); // Masquer l'objet après le fade-out
            }
        }
    }

    private void Fire_OnFireFuelled(object sender, System.EventArgs e) {
        if (Mathf.Abs(Player.Instance.transform.position.x - Fire.Instance.transform.position.x) < minDistanceToFireToShowUI) return;

        RefreshProgressBar();
    }

    private void Fire_OnFireChangedState(object sender, Fire.OnFireChangedStateEventArgs e) {
        if (e.newState == Fire.State.extinguished) {
            fireAnimator.ResetTrigger("Calm");
            fireAnimator.SetTrigger("Extinguished");
        }

        if (e.newState == Fire.State.wild) {
            fireAnimator.SetTrigger("Wild");
        }

        if (e.newState == Fire.State.mild) {
            fireAnimator.ResetTrigger("Calm");
            fireAnimator.SetTrigger("Mild");
        }

        if (e.newState == Fire.State.insane) {
            fireAnimator.SetTrigger("Insane");
        }

        if (e.newState == Fire.State.calm) {
            fireAnimator.SetTrigger("Calm");
        }

        RefreshBackgroundProgressBar();
    }

    private void StructureUI_Fire_OnFireTickRemoved(object sender, System.EventArgs e) {
        if (Mathf.Abs(Player.Instance.transform.position.x - Fire.Instance.transform.position.x) < minDistanceToFireToShowUI) return;

        DisplayFireUI();
        RefreshProgressBar();
    }

    private void StructureUI_Fire_OnFireMaxBarAmountChanged(object sender, System.EventArgs e) {
        RefreshBackgroundProgressBar();
    }

    private void RefreshBackgroundProgressBar() {
        progressBarBackgroundTemplate.gameObject.SetActive(true);
        foreach (RectTransform child in progressBarBackgroundContainer) {
            if (child == progressBarBackgroundTemplate) continue;
            Destroy(child.gameObject);
        }
        

        int maxBars = StructureUI_Fire.Instance.GetMaxBarAmount();
        for(int i = 0; i < maxBars; i++) {
            Instantiate(progressBarBackgroundTemplate, progressBarBackgroundContainer);
        }

        progressBarBackgroundTemplate.gameObject.SetActive(false);
    }

    private void RefreshProgressBar() {

        progressBarTemplate.gameObject.SetActive(true);

        foreach (RectTransform child in progressBarContainer) {
            if (child == progressBarTemplate) continue;
            Destroy(child.gameObject);
        }

        int currentBars = StructureUI_Fire.Instance.GetCurrentBarAmount();
        int maxBars = StructureUI_Fire.Instance.GetMaxBarAmount();

        for (int i = 0; i < currentBars; i++) {
           Instantiate(progressBarTemplate, progressBarContainer);
        }

        PlayerUI_TickTemplate[] tickArray = progressBarContainer.GetComponentsInChildren<PlayerUI_TickTemplate>();
        Debug.Log("tickArray.Length " + tickArray.Length);
        Debug.Log("currentBars " + currentBars);
        Debug.Log("maxBars " + maxBars);

        tickArray[tickArray.Length - 1].RemoveTick(1);
        tickArray[tickArray.Length - 1].GetComponent<Rigidbody2D>().gravityScale = 2f;
        tickArray[tickArray.Length - 1].transform.SetParent(fireUIGameObject.transform, true);

        progressBarTemplate.gameObject.SetActive(false);
    }
    private void DisplayFireUI() {
        isDisplaying = true;
        fireUIGameObject.SetActive(true);
        fireUICanvasGroup.alpha = 1.0f;
        displayTimer = 0;
    }
}
