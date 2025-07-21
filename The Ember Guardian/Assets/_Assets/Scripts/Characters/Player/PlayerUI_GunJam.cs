using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_GunJam : MonoBehaviour
{
    public static PlayerUI_GunJam PrimaryWeaponGunJamUI;
    public static PlayerUI_GunJam SecondaryWeaponGunJamUI;

    [SerializeField] private bool isPrimaryWeaponJamUI;
    private bool jamUIActive;

    [SerializeField] private Transform inputSequenceIconContainer;
    [SerializeField] private GunJamSingleIconUI inputSequenceIconTemplate;

    [SerializeField] private GameObject spamGameObject;
    [SerializeField] private Transform spamContainer;
    [SerializeField] private PlayerUI_TickTemplate spamTickTemplate;

    [SerializeField] private Transform timingGameObject;
    [SerializeField] private RectTransform tickTransform;
    [SerializeField] private RectTransform backgroundValidZone;
    [SerializeField] private RectTransform failZoneLeft;
    [SerializeField] private RectTransform failZoneRight; 
    [SerializeField] private Animator timerBarOutlineAnimator;

    [SerializeField] private GameObject gunJamTimerGameObject;
    [SerializeField] private Image gunJamTimerFillImage; 
    [SerializeField] private Animator gunJamTimerAnimator; 
    
    private float initialValidFraction = 0.6f; // 60% au départ
    private float finalValidFraction = 0.2f;   // 20% à la fin
    private int maxStages; 
    private int currentStage = 0;
    private float totalWidth;
    private float tickSpeed = 3f; // pixels par seconde
    private float jamHitAnimationDuration = .3f;

    private bool isSequenceQTEActive = false;
    private bool isTimingQTEActive = false;
    private bool isSpamQTEActive = false;

    private float tickDirection = 1f;
    private float tickMinX;
    private float tickMaxX;
    private float timingButtonJustPressedTimer;
    private bool timingButtonJustPressed;

    private List<PlayerUI_TickTemplate> currentActiveTickTemplateList = new List<PlayerUI_TickTemplate>();
    private int currentTickIndex = 0;

    private float currentSpamProgress = 0f;
    private float requiredSpamProgress = 1f;

    private void Awake() {
        if(isPrimaryWeaponJamUI) {
            PrimaryWeaponGunJamUI = this;
        } else {
            SecondaryWeaponGunJamUI = this;
        }

        inputSequenceIconContainer.gameObject.SetActive(false);
        timingGameObject.gameObject.SetActive(false);
        spamGameObject.gameObject.SetActive(false);
        inputSequenceIconTemplate.gameObject.SetActive(false);
        gunJamTimerGameObject.gameObject.SetActive(false);
    }

    private void Start() {
        GunJamHandler.OnAnyJamSequenceGenerated += GunJamHandler_OnJamSequenceGenerated;
        GunJamHandler.OnAnyJamSequenceCompleted += GunJamHandler_OnAnyJamSequenceCompleted;
        GunJamHandler.OnSpamQTEProgressed += GunJamHandler_OnSpamQTEProgressed;
        GunJamHandler.OnAnyTimingButtonPressed += GunJamHandler_OnAnyTimingButtonPressed;
        GunJamHandler.OnAnyJamTimerProgressed += GunJamHandler_OnAnyJamTimerProgressed;
        GunJamHandler.OnAnyJamWrongInput += GunJamHandler_OnAnyJamWrongInput;
        PlayerShoot.Instance.OnPlayerSwappedGun += PlayerShoot_OnPlayerSwappedGun;
    }

    private void Update() {
        if (isTimingQTEActive) {
            if(timingButtonJustPressed) {
                timingButtonJustPressedTimer -= Time.deltaTime;
                if(timingButtonJustPressedTimer <= 0) {
                    timingButtonJustPressed = false;
                    CheckTimingQTEAfterHitDelay();
                }
                return;
            }
            Vector2 pos = tickTransform.anchoredPosition;
            pos.x += tickSpeed * tickDirection * Time.deltaTime;

            if (pos.x > tickMaxX) {
                pos.x = tickMaxX;
                tickDirection = -1f;
            }
            else if (pos.x < tickMinX) {
                pos.x = tickMinX;
                tickDirection = 1f;
            }

            tickTransform.anchoredPosition = pos;
        };

        if (isSpamQTEActive) {
            // Calcul du nombre de ticks complètement remplis
            int newFilledTickIndex = Mathf.FloorToInt(currentSpamProgress);

            // Si on est passé à un nouveau tick rempli, on valide et on le retire
            while (currentTickIndex < newFilledTickIndex && currentTickIndex < currentActiveTickTemplateList.Count) {
                int reverseIndex = currentActiveTickTemplateList.Count - 1 - currentTickIndex;
                currentActiveTickTemplateList[reverseIndex].SetImageFill(1f); // par sécurité
                StartCoroutine(RemoveSpamTickAfterDelay(currentActiveTickTemplateList[reverseIndex], jamHitAnimationDuration));
                currentTickIndex++;
            }

            // Mise à jour du tick en cours de remplissage (non encore retiré)
            if (currentTickIndex < currentActiveTickTemplateList.Count) {
                int reverseIndex = currentActiveTickTemplateList.Count - 1 - currentTickIndex;
                float partialFill = currentSpamProgress - currentTickIndex;
                currentActiveTickTemplateList[reverseIndex].SetImageFill(partialFill);
            }
        };

    }
    private void GunJamHandler_OnAnyJamWrongInput(object sender, GunJamHandler.OnAnyJamSequenceProgressedEventArgs e) {
        gunJamTimerAnimator.SetTrigger("Penalty");
    }

    private void GunJamHandler_OnAnyJamTimerProgressed(object sender, GunJamHandler.OnAnyJamTimerProgressedEventArgs e) {
        gunJamTimerFillImage.fillAmount = 1 - e.jamProgressTimerNormalized;
    }

    private IEnumerator RemoveSpamTickAfterDelay(PlayerUI_TickTemplate tick, float delay) {
        yield return new WaitForSeconds(delay);
        tick.transform.SetParent(transform);
        tick.RemoveTick(1f, true, 2f, .3f);
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        RefreshPrimaryOrSecondaryUI();
    }

    private void GunJamHandler_OnSpamQTEProgressed(object sender, GunJamHandler.OnSpamQTEProgressedEventArgs e) {
        if (!jamUIActive) return;
        currentSpamProgress = e.spamProgress;
    }

    private void GunJamHandler_OnAnyTimingButtonPressed(object sender, System.EventArgs e) {
        if (!jamUIActive) return;
        timingButtonJustPressed = true;
        timingButtonJustPressedTimer = jamHitAnimationDuration;

        if (timerBarOutlineAnimator == null) {
            Debug.LogWarning("timerBarOutlineAnimator n'est pas assigné dans PlayerUI_GunJam !");
            return;
        }

        if (TimingQTEIsRight()) {
            timerBarOutlineAnimator.SetTrigger("Valid");
        } else {
            timerBarOutlineAnimator.SetTrigger("Wrong");
        }
    }

    private void GunJamHandler_OnAnyJamSequenceCompleted(object sender, System.EventArgs e) {
        if (!jamUIActive) return;

        CleanUISequence();

        gunJamTimerGameObject.gameObject.SetActive(false);
        if (isSpamQTEActive) {
            isSpamQTEActive = false;
            spamGameObject.gameObject.SetActive(false);
            currentActiveTickTemplateList.Clear();
            currentTickIndex = 0;
        }

        if(isTimingQTEActive) {
            isTimingQTEActive = false;
            timingGameObject.gameObject.SetActive(false);
            currentStage = 0;
        }

        isSequenceQTEActive = false;
    }

    private void GunJamHandler_OnJamSequenceGenerated(object sender, GunJamHandler.OnJamSequenceGeneratedEventArgs e) {
        RefreshPrimaryOrSecondaryUI();
        if (!jamUIActive) return;

        gunJamTimerGameObject.gameObject.SetActive(true);
        switch (e.qteType) {
            case GunJamHandler.QTEType.InputSequence:
                isSequenceQTEActive = true;
                HandleInputSequenceQTE(e.inputSequence);
                break;

            case GunJamHandler.QTEType.TimingChallenge:
                isTimingQTEActive = true;
                HandleTimingQTE();
                break;

            case GunJamHandler.QTEType.SpamButton:
                isSpamQTEActive = true;
                HandleSpamButtonQTE(e.spamTargetProgress);
                break;
        }
    }
    private void HandleInputSequenceQTE(Queue<GameInput.Binding> inputSequence) {
        inputSequenceIconContainer.gameObject.SetActive(true);
        int i = 0;
        foreach (GameInput.Binding inputBinding in inputSequence) {
            GunJamSingleIconUI inputIcon = Instantiate(inputSequenceIconTemplate.transform, inputSequenceIconContainer).GetComponent<GunJamSingleIconUI>();
            inputIcon.gameObject.SetActive(true);
            inputIcon.SetBinding(inputBinding);
            inputIcon.SetIndex(i++);
        }
    }

    private void HandleTimingQTE() {
        timingGameObject.gameObject.SetActive(true);
        maxStages = PlayerShoot.Instance.GetHeldGun().GetJamRepairHitAmount();

        float halfWidth = ((RectTransform)backgroundValidZone.parent).rect.width * 0.5f;
        tickMinX = -halfWidth;
        tickMaxX = halfWidth;

        tickTransform.anchoredPosition = new Vector2(tickMinX, tickTransform.anchoredPosition.y);
        tickDirection = 1f;

        UpdateFailZones();
    }

    private void HandleSpamButtonQTE(float targetProgress) {
        spamGameObject.gameObject.SetActive(true);
        currentSpamProgress = 0f;
        requiredSpamProgress = targetProgress;

        foreach (Transform child in spamContainer) {
            if (child == spamTickTemplate.transform) continue;
            Destroy(child.gameObject);
        }

        spamTickTemplate.gameObject.SetActive(true);
        currentActiveTickTemplateList.Clear();

        for (int i = 0; i < requiredSpamProgress; i++) {
            var tickTemplate = Instantiate(spamTickTemplate, spamContainer).GetComponent<PlayerUI_TickTemplate>();
            tickTemplate.SetImageFill(0f);
            currentActiveTickTemplateList.Add(tickTemplate);
        }

        spamTickTemplate.gameObject.SetActive(false);
    }

    private void RefreshPrimaryOrSecondaryUI() {
        if ((isPrimaryWeaponJamUI && PlayerShoot.Instance.GetHeldGunSO() == PlayerShoot.Instance.GetPrimaryGunSO()) || (!isPrimaryWeaponJamUI && PlayerShoot.Instance.GetHeldGunSO() == PlayerShoot.Instance.GetSecondaryGunSO())) {

            if (isTimingQTEActive) {
                timingGameObject.gameObject.SetActive(true);
            }
            if(isSpamQTEActive) {
                spamGameObject.gameObject.SetActive(true);
            }
            if(isSequenceQTEActive) {
                inputSequenceIconContainer.gameObject.SetActive(true);
            }

            if(isTimingQTEActive || isSpamQTEActive || isSequenceQTEActive) {
                gunJamTimerGameObject.SetActive(true);
            }

            jamUIActive = true;

        } else {

            inputSequenceIconContainer.gameObject.SetActive(false);
            timingGameObject.gameObject.SetActive(false);
            spamGameObject.gameObject.SetActive(false);
            gunJamTimerGameObject.SetActive(false);
            jamUIActive = false;
        }

    }

    private void CheckTimingQTEAfterHitDelay() {
        if (TimingQTEIsRight()) {
            currentStage++;
            UpdateFailZones();
        }
        else {
            ResetTimingQTE();
        }
    }

    private void UpdateFailZones() {
        totalWidth = backgroundValidZone.rect.width;

        // Fraction actuelle de zone valide
        float t = Mathf.Clamp01((float)currentStage / maxStages);
        float currentValidFraction = Mathf.Lerp(initialValidFraction, finalValidFraction, t);
        float currentValidWidth = totalWidth * currentValidFraction;

        // Répartition aléatoire entre gauche et droite
        float leftFailFraction = Random.Range(0f, 1f);
        float rightFailFraction = 1f - leftFailFraction;

        float remainingWidth = totalWidth - currentValidWidth;
        float leftFailWidth = remainingWidth * leftFailFraction;
        float rightFailWidth = remainingWidth * rightFailFraction;

        failZoneLeft.sizeDelta = new Vector2(leftFailWidth, failZoneLeft.sizeDelta.y);
        failZoneRight.sizeDelta = new Vector2(rightFailWidth, failZoneRight.sizeDelta.y);
    }

    private void ResetTimingQTE() {
        currentStage = 0;
        UpdateFailZones();
    }
    private void CleanUISequence() {
        foreach(Transform child in inputSequenceIconContainer) {
            if (child == inputSequenceIconTemplate.transform) continue;
            Destroy(child.gameObject);
        }
    }

    public bool TimingQTEIsRight() {
        float tickX = tickTransform.anchoredPosition.x;

        float totalWidth = backgroundValidZone.rect.width;
        float halfWidth = totalWidth * 0.5f;

        float leftFailWidth = failZoneLeft.sizeDelta.x;
        float rightFailWidth = failZoneRight.sizeDelta.x;

        // Convertir les bornes absolues en coordonnées centrées
        float leftBound = -halfWidth + leftFailWidth;
        float rightBound = halfWidth - rightFailWidth;

        return tickX >= leftBound && tickX <= rightBound;
    }


    private void OnDestroy() {
        GunJamHandler.OnAnyJamSequenceGenerated -= GunJamHandler_OnJamSequenceGenerated;
        GunJamHandler.OnAnyJamSequenceCompleted -= GunJamHandler_OnAnyJamSequenceCompleted;
        GunJamHandler.OnSpamQTEProgressed -= GunJamHandler_OnSpamQTEProgressed;
        GunJamHandler.OnAnyTimingButtonPressed -= GunJamHandler_OnAnyTimingButtonPressed;
        GunJamHandler.OnAnyJamTimerProgressed -= GunJamHandler_OnAnyJamTimerProgressed;
        GunJamHandler.OnAnyJamWrongInput -= GunJamHandler_OnAnyJamWrongInput;
    }
}
