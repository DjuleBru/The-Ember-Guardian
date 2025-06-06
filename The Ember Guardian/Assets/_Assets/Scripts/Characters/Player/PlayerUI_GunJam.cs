using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_GunJam : MonoBehaviour
{
    public static PlayerUI_GunJam Instance;

    [SerializeField] private GunJamSingleIconUI inputIconTemplate;

    [SerializeField] private Transform primaryGunJamObject;
    [SerializeField] private Transform secondaryGunJamObject;

    [SerializeField] private Transform primaryGunInputIconContainer;
    [SerializeField] private Transform secondaryGunInputIconContainer;
    [SerializeField] private Transform primarySpamGameObject;
    [SerializeField] private Transform secondarySpamGameObject;
    [SerializeField] private Transform timingGameObject;

    [SerializeField] private PlayerUI_TickTemplate spamTickTemplate;
    [SerializeField] private Transform primaryGunSpamTickContainer;
    [SerializeField] private Transform secondaryGunSpamTickContainer;

    [SerializeField] private RectTransform tickTransform;
    [SerializeField] private RectTransform backgroundValidZone;
    [SerializeField] private RectTransform failZoneLeft;
    [SerializeField] private RectTransform failZoneRight; 
    [SerializeField] private Animator timerBarOutlineAnimator; 
    
    private float initialValidFraction = 0.6f; // 60% au départ
    private float finalValidFraction = 0.2f;   // 20% à la fin
    private int maxStages; 
    private int currentStage = 0;
    private float totalWidth;

    private float tickSpeed = 3f; // pixels par seconde
    private float jamHitAnimationDuration = .3f;

    private Transform activeGunJamObject;
    private Transform activeInputContainer;
    private Transform activeSpamContainer;
    private Transform activeSpamGameObject;

    private bool isTimingQTEActive = false;
    private float tickDirection = 1f;
    private float tickMinX;
    private float tickMaxX;
    private float timingButtonJustPressedTimer;
    private bool timingButtonJustPressed;

    private List<PlayerUI_TickTemplate> currentActiveTickTemplateList = new List<PlayerUI_TickTemplate>();
    private int currentTickIndex = 0;

    private bool isSpamQTEActive = false;
    private float currentSpamProgress = 0f;
    private float requiredSpamProgress = 1f;

    private void Awake() {
        Instance = this;

        inputIconTemplate.gameObject.SetActive(false);
        primaryGunJamObject.gameObject.SetActive(false);
        secondaryGunJamObject.gameObject.SetActive(false);
        primarySpamGameObject.gameObject.SetActive(false);
        secondarySpamGameObject.gameObject.SetActive(false);
        timingGameObject.gameObject.SetActive(false);
    }

    private void Start() {
        GunJamHandler.OnAnyJamSequenceGenerated += GunJamHandler_OnJamSequenceGenerated;
        GunJamHandler.OnAnyJamSequenceCompleted += GunJamHandler_OnAnyJamSequenceCompleted;
        GunJamHandler.OnSpamQTEProgressed += GunJamHandler_OnSpamQTEProgressed;
        GunJamHandler.OnAnyTimingButtonPressed += GunJamHandler_OnAnyTimingButtonPressed;
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

    private IEnumerator RemoveSpamTickAfterDelay(PlayerUI_TickTemplate tick, float delay) {
        yield return new WaitForSeconds(delay);
        tick.transform.SetParent(activeGunJamObject);
        tick.RemoveTick(.5f);
    }

    private void PlayerShoot_OnPlayerSwappedGun(object sender, System.EventArgs e) {
        RefreshPrimaryOrSecondaryUI();
    }

    private void GunJamHandler_OnSpamQTEProgressed(object sender, GunJamHandler.OnSpamQTEProgressedEventArgs e) {
        currentSpamProgress = e.spamProgress;
    }

    private void GunJamHandler_OnAnyTimingButtonPressed(object sender, System.EventArgs e) {
        timingButtonJustPressed = true;
        timingButtonJustPressedTimer = jamHitAnimationDuration;

        if(TimingQTEIsRight()) {
            timerBarOutlineAnimator.SetTrigger("Valid");
        } else {
            timerBarOutlineAnimator.SetTrigger("Wrong");
        }
    }

    private void GunJamHandler_OnAnyJamSequenceCompleted(object sender, System.EventArgs e) {
        CleanUISequence();

        if(isSpamQTEActive) {
            isSpamQTEActive = false;
            activeSpamGameObject.gameObject.SetActive(false);
            currentActiveTickTemplateList.Clear();
            currentTickIndex = 0;
        }

        if(isTimingQTEActive) {
            isTimingQTEActive = false;
            timingGameObject.gameObject.SetActive(false);
            currentTickIndex = 0;
        }
    }

    private void GunJamHandler_OnJamSequenceGenerated(object sender, GunJamHandler.OnJamSequenceGeneratedEventArgs e) {
        RefreshPrimaryOrSecondaryUI();

        switch (e.qteType) {
            case GunJamHandler.QTEType.InputSequence:
                HandleInputSequenceQTE(e.inputSequence);
                break;

            case GunJamHandler.QTEType.TimingChallenge:
                HandleTimingQTE();
                break;

            case GunJamHandler.QTEType.SpamButton:
                HandleSpamButtonQTE(e.spamTargetProgress);
                break;
        }
    }
    private void HandleInputSequenceQTE(Queue<GameInput.Binding> inputSequence) {
        activeInputContainer.gameObject.SetActive(true);
        int i = 0;
        foreach (GameInput.Binding inputBinding in inputSequence) {
            GunJamSingleIconUI inputIcon = Instantiate(inputIconTemplate.transform, activeInputContainer).GetComponent<GunJamSingleIconUI>();
            inputIcon.gameObject.SetActive(true);
            inputIcon.SetBinding(inputBinding);
            inputIcon.SetIndex(i++);
        }
    }

    private void HandleTimingQTE() {
        timingGameObject.gameObject.SetActive(true);
        maxStages = PlayerShoot.Instance.GetHeldGunSO().jamRepairHitAmount;
        isTimingQTEActive = true;

        float halfWidth = ((RectTransform)backgroundValidZone.parent).rect.width * 0.5f;
        tickMinX = -halfWidth;
        tickMaxX = halfWidth;

        tickTransform.anchoredPosition = new Vector2(tickMinX, tickTransform.anchoredPosition.y);
        tickDirection = 1f;

        UpdateFailZones();
    }

    private void HandleSpamButtonQTE(float targetProgress) {
        activeSpamGameObject.gameObject.SetActive(true);
        isSpamQTEActive = true;
        currentSpamProgress = 0f;
        requiredSpamProgress = targetProgress;

        foreach (Transform child in activeSpamContainer) {
            if (child == spamTickTemplate.transform) continue;
            Destroy(child.gameObject);
        }

        spamTickTemplate.gameObject.SetActive(true);
        currentActiveTickTemplateList.Clear();

        for (int i = 0; i < requiredSpamProgress; i++) {
            var tickTemplate = Instantiate(spamTickTemplate, activeSpamContainer).GetComponent<PlayerUI_TickTemplate>();
            tickTemplate.SetImageFill(0f);
            currentActiveTickTemplateList.Add(tickTemplate);
        }

        spamTickTemplate.gameObject.SetActive(false);
    }


    private void RefreshPrimaryOrSecondaryUI() {
        primaryGunJamObject.gameObject.SetActive(false);
        secondaryGunJamObject.gameObject.SetActive(false);
        primarySpamGameObject.gameObject.SetActive(false);
        secondarySpamGameObject.gameObject.SetActive(false);

        if (PlayerShoot.Instance.GetHeldGunSO() == PlayerShoot.Instance.GetPrimaryGunSO()) {
            activeGunJamObject = primaryGunJamObject;
            activeInputContainer = primaryGunInputIconContainer;

            activeSpamGameObject = primarySpamGameObject;
            activeSpamContainer = primaryGunSpamTickContainer;
        }
        else {
            activeGunJamObject = secondaryGunJamObject;
            activeInputContainer = secondaryGunInputIconContainer;

            activeSpamGameObject = secondarySpamGameObject;
            activeSpamContainer = secondaryGunSpamTickContainer;
        }

        activeGunJamObject.gameObject.SetActive(true);
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
        foreach(Transform child in activeInputContainer) {
            if (child == inputIconTemplate.transform) continue;
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
    }
}
