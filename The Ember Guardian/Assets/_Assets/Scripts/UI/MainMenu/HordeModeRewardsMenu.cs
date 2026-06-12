using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HordeModeRewardsMenu : MonoBehaviour
{
    public static HordeModeRewardsMenu Instance;

    [System.Serializable]
    public class UnlockableData {
        public HordeModeProgressionManager.HordeModeUnlockables unlockable;
        public Sprite sprite;
        public string localizationKey;
        public string descriptionLocalizationKey;
    }

    [SerializeField]
    private List<UnlockableData> unlockableDataList = new List<UnlockableData>();
    [SerializeField] private GameObject panelGO;
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private Image xpProgressionBar;
    [SerializeField] private Image nextUnlockImage;
    [SerializeField] private TextMeshProUGUI nightsSurvivedAmountText;
    [SerializeField] private TextMeshProUGUI nextUnlockText;
    [SerializeField] private TextMeshProUGUI nextUnlockDescriptionText;
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Material newUnlockMaterial;
    [SerializeField] private Animator newUnlockAnimator;
    [SerializeField] private Button nextUnlockButton;
    [SerializeField] private GameObject pressAnyKeyToContinueGO;
    [SerializeField] private GameObject lockedInDemoDescriptionGO;
    [SerializeField] private float barFillSpeed;

    private string nextUnlockNameLocalizationKey;
    private string nextUnlockNameDescriptionLocalizationKey;
    private bool panelOpen;
    private bool skipFill = false;
    private bool unlockSequenceCoroutineRunning;

    private Coroutine unlockSequenceCoroutine;
    private Coroutine shineCoroutine;

    public event EventHandler OnProgressionBarStartFill;
    public event EventHandler OnProgressionBarEndFill;
    public event EventHandler OnNewItemUnlocked;

    private void Awake() {
        Instance = this;
        panelGO.SetActive(false);

        xpProgressionBar.fillAmount = ES3.Load("lastProgressionBarFillAmount", 0f);
    }

    private void Start() {
        InitializeMenu();
    }

    public void InitializeMenu() {
        nextUnlockText.font = LocalizationManager.Instance.GetCurrentFont();
        nextUnlockText.text = LocalizationManager.Instance.GetLocalizedText("Next Unlock");
        nextUnlockDescriptionText.font = LocalizationManager.Instance.GetCurrentFont();
        nextUnlockDescriptionText.text = "";
        nightsSurvivedAmountText.font = LocalizationManager.Instance.GetCurrentFont();

        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnEscapePerformed += GameInput_OnEscapePerformed;

        ES3Settings hordeModeSaveFileSettings = new ES3Settings("SaveFile_HordeMode.es3");

        if (HordeModeProgressionManager.Instance.LastXPGainWasFromMainGame()) {
            nightsSurvivedAmountText.text = LocalizationManager.Instance.GetLocalizedText("menu_hordeModeXPBackFromMainGame");
        }
        else {
            nightsSurvivedAmountText.text = LocalizationManager.Instance.GetLocalizedText("menu_nightsSurvived") + " " + ES3.Load("lastHordeModeNightsSurvived", 0, hordeModeSaveFileSettings);
        }

        pressAnyKeyToContinueGO.SetActive(false);
        SetNextUnlockParameters(HordeModeProgressionManager.Instance.GetNextUnlockable());
        SetNextUnlockMaterialAndText();
    }

    private void Update() {
        if (!panelOpen) return;
        if(Input.anyKeyDown) {
            if (!unlockSequenceCoroutineRunning && panelOpen) {
                ClosePanel();
            }
        } 
    }

    [Button]
    public void OpenPanelAndCommitXP() {
        panelOpen = true;
        MusicManager.Instance.SetAudioVolume(.5f);
        StartUnlockSequence();
    }

    public void StartUnlockSequence() {
        unlockSequenceCoroutine = StartCoroutine(UnlockSequence());
        unlockSequenceCoroutineRunning = true;
    }

    private IEnumerator UnlockSequence() {
        panelGO.SetActive(true);
        panelAnimator.SetTrigger("Show");

        yield return new WaitForSeconds(.3f);

        skipFill = false;

        int totalXP = HordeModeProgressionManager.Instance.GetTotalXP();
        bool reachedLockedInDemoUnlockable = false;

        // Récupération des unlocks atteignables
        List<HordeModeProgressionManager.HordeModeUnlockables> pendingUnlocks = new List<HordeModeProgressionManager.HordeModeUnlockables>();

        HordeModeProgressionManager.HordeModeUnlockables lastDemoUnlockable = HordeModeProgressionManager.HordeModeUnlockables.Revolver;
        foreach (var kvp in HordeModeProgressionManager.Instance.unlockThresholds) {

            if (HordeModeProgressionManager.Instance.CheckUnlockableLockedInDemo(kvp.Key)) {
                lastDemoUnlockable = kvp.Key;
                break;
            };
            if (!HordeModeProgressionManager.Instance.GetUnlocked(kvp.Key) && totalXP >= kvp.Value) {
                pendingUnlocks.Add(kvp.Key);
            }

        }

        // Unlocks de migration — on les ajoute EN PREMIER pour garder l'ordre logique
        List<HordeModeProgressionManager.HordeModeUnlockables> migrationUnlocks =
            HordeModeProgressionManager.Instance.GetPendingMigrationUnlocks();

        // Fusionner sans doublons, migration en premier
        foreach (var m in migrationUnlocks) {
            if (!pendingUnlocks.Contains(m))
                pendingUnlocks.Insert(0, m);
        }

        // Séquence pour chaque unlock
        foreach (var unlock in pendingUnlocks) {
            if(unlock == HordeModeProgressionManager.HordeModeUnlockables.None) {
                HordeModeProgressionManager.Instance.SetHasNoXPToCommit();
                unlockSequenceCoroutineRunning = false;
                yield break;
            }

            if(unlock == lastDemoUnlockable) {
                reachedLockedInDemoUnlockable = true;
            }
  
            pressAnyKeyToContinueGO.SetActive(false);
            SetNextUnlockParameters(unlock);
            SetNextUnlockMaterialAndText();

            yield return new WaitForEndOfFrame();

            // Animation de début
            newUnlockAnimator.ResetTrigger("Next");
            newUnlockAnimator.SetTrigger("Unlock");

            if(shineCoroutine != null) {
                StopCoroutine(shineCoroutine);
            }
            shineCoroutine = StartCoroutine(AnimateUIShineMaterial());

            // ----------------------
            //  FILL SEQUENCE
            // ----------------------
            bool skipToEndOfUnlock = false;

            float startFill = xpProgressionBar.fillAmount;
            float targetFill = 1f;

            float duration = 1.2f;
            float elapsed = 0f;

            OnProgressionBarStartFill?.Invoke(this, EventArgs.Empty);

            while (elapsed < duration) {
                if (AnyInputPressed()) {
                    skipToEndOfUnlock = true;
                    xpProgressionBar.fillAmount = targetFill;
                    break;
                }

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = t * t;

                xpProgressionBar.fillAmount = Mathf.Lerp(startFill, targetFill, eased);

                yield return null;
            }

            xpProgressionBar.fillAmount = targetFill;

            OnProgressionBarEndFill?.Invoke(this, EventArgs.Empty);

            // ----------------------
            // FIN ANIMATION / UI
            // ----------------------
            if (skipToEndOfUnlock) {
                newUnlockAnimator.Play("HordeModeNextUnlock_Unlock", 0, 1f);
            }
            else {
                newUnlockAnimator.ResetTrigger("Next");
                newUnlockAnimator.SetTrigger("Unlock");
            }

            OnNewItemUnlocked?.Invoke(this, EventArgs.Empty);
            SetNewUnlockMaterialAndText();

            // ----------------------------------------------------
            // WAIT 1s OR SKIP — MAIS PAS AUTO-CONTINUE !
            // ----------------------------------------------------
            float waitTime = 1f;
            float waitElapsed = 0f;
            bool skipDelay = false;

            while (waitElapsed < waitTime) {
                if (AnyInputPressed()) {
                    skipDelay = true;
                    break;
                }

                waitElapsed += Time.deltaTime;
                yield return null;
            }

            // On montre toujours le "press any key" maintenant
            pressAnyKeyToContinueGO.SetActive(true);

            // On attend UNE frame pour ne pas réutiliser le même input
            // Attendre que tous les inputs soient relâchés
            yield return new WaitUntil(() => !Input.anyKey);

            // Et on attend UN NOUVEAU CLIC pour passer à la suite
            // Maintenant attendre un NOUVEL input
            yield return new WaitUntil(() => Input.anyKeyDown);

            // Ajout officiel de l’unlock
            HordeModeProgressionManager.Instance.AddUnlocked(unlock);

            // Reset pour l’étape suivante
            xpProgressionBar.fillAmount = 0f;
            yield return null;
        }

        // -------------------------
        // FIN : REMPLISSAGE FINAL
        // -------------------------

        pressAnyKeyToContinueGO.SetActive(false);

        if (!HordeModeProgressionManager.Instance.GetAllUnlocked()) {

            SetNextUnlockParameters(HordeModeProgressionManager.Instance.GetNextUnlockable());
            SetNextUnlockMaterialAndText();

            float finalTarget = Mathf.Clamp01(GetXPProgressNormalized());
            float endSpeed = 1f;

            ES3.Save("lastProgressionBarFillAmount", finalTarget);

            skipFill = false;

            OnProgressionBarStartFill?.Invoke(this, EventArgs.Empty);

            while (xpProgressionBar.fillAmount < finalTarget) {
                if (skipFill || AnyInputPressed()) {
                    xpProgressionBar.fillAmount = finalTarget;
                    break;
                }

                xpProgressionBar.fillAmount += Time.deltaTime * endSpeed;
                yield return null;
            }


        }

        OnProgressionBarEndFill?.Invoke(this, EventArgs.Empty);
        HordeModeProgressionManager.Instance.SetHasNoXPToCommit();
        HordeModeProgressionManager.Instance.ClearPendingMigrationUnlocks();
        unlockSequenceCoroutineRunning = false;

        if(reachedLockedInDemoUnlockable) {
            pressAnyKeyToContinueGO.SetActive(true);
            nextUnlockText.text = LocalizationManager.Instance.GetLocalizedText(nextUnlockNameLocalizationKey);
            nextUnlockDescriptionText.text = LocalizationManager.Instance.GetLocalizedText(nextUnlockNameDescriptionLocalizationKey);
            pressAnyKeyToContinueGO.GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizedText("card_lockedInDemo");
        }


        yield return new WaitForSeconds(1f);
        pressAnyKeyToContinueGO.SetActive(true);
    }

    private void GameInput_OnEscapePerformed(object sender, EventArgs e) {
        SkipAllRewards();
    }
    private void GameInput_OnPlayerBackPerformed(object sender, EventArgs e) {
        SkipAllRewards();
    }

    private void SkipAllRewards() {
        if (!unlockSequenceCoroutineRunning) return;

        // Récupération des unlocks atteignables
        int totalXP = HordeModeProgressionManager.Instance.GetTotalXP();
        foreach (var kvp in HordeModeProgressionManager.Instance.unlockThresholds) {
            if (!HordeModeProgressionManager.Instance.GetUnlocked(kvp.Key) && totalXP >= kvp.Value) {
                HordeModeProgressionManager.Instance.AddUnlocked(kvp.Key);
            }

        }

        foreach (var unlock in HordeModeProgressionManager.Instance.GetPendingMigrationUnlocks()) {
            HordeModeProgressionManager.Instance.AddUnlocked(unlock);
        }
        HordeModeProgressionManager.Instance.ClearPendingMigrationUnlocks();

        StopCoroutine(unlockSequenceCoroutine);
        HordeModeProgressionManager.Instance.SetHasNoXPToCommit();
        unlockSequenceCoroutineRunning = false;
        ClosePanel();
    }

    public void ClosePanel() {
        MusicManager.Instance.SetAudioVolume(1f);
        panelAnimator.SetTrigger("Hide");
        HordeModeUI.Instance.OpenHordeModePanel(false);
        panelOpen = false;
    }

    public void SetNextUnlockMaterialAndText() {
        nextUnlockImage.material = silhouetteMaterial;
        nextUnlockText.text = LocalizationManager.Instance.GetLocalizedText("Next Unlock");
        nextUnlockDescriptionText.text = "";
    }

    public void SetNewUnlockMaterialAndText() {
        nextUnlockImage.material = newUnlockMaterial;
        nextUnlockText.text = LocalizationManager.Instance.GetLocalizedText(nextUnlockNameLocalizationKey) + " " + LocalizationManager.Instance.GetLocalizedText("Unlocked") + "!";
        nextUnlockDescriptionText.text = LocalizationManager.Instance.GetLocalizedText(nextUnlockNameDescriptionLocalizationKey);
    }

    public void SetNextUnlockParameters(HordeModeProgressionManager.HordeModeUnlockables unlockable) {
        nextUnlockImage.sprite = GetUnlockableSprite(unlockable);
        nextUnlockNameLocalizationKey = GetUnlockableNameLocalizationKey(unlockable);
        nextUnlockNameDescriptionLocalizationKey = GetUnlockableDescriptionLocalizationKey(unlockable);
        newUnlockAnimator.ResetTrigger("Unlock");
        newUnlockAnimator.SetTrigger("Next");
    }

    private IEnumerator AnimateUIShineMaterial() {
        yield return new WaitForSeconds(1.2f);
        float duration = 0.8f;
        float elapsed = 0f;

        float startValue = 0.9f;
        float endValue = 0f;

        Material mat = nextUnlockImage.material;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float value = Mathf.Lerp(startValue, endValue, t);

            mat.SetFloat("_ShineLocation", value);
            //Debug.Log(value);
            yield return null;
        }

        // Sécurité : forcer la valeur finale
        mat.SetFloat("_ShineLocation", endValue);
        shineCoroutine = null;
    }

    public Sprite GetUnlockableSprite(HordeModeProgressionManager.HordeModeUnlockables unlockable) {
        for (int i = 0; i < unlockableDataList.Count; i++) {
            if (unlockableDataList[i].unlockable == unlockable)
                return unlockableDataList[i].sprite;
        }

        return null;
    }
    public string GetUnlockableNameLocalizationKey(HordeModeProgressionManager.HordeModeUnlockables unlockable) {
        for (int i = 0; i < unlockableDataList.Count; i++) {
            if (unlockableDataList[i].unlockable == unlockable)
                return unlockableDataList[i].localizationKey;
        }

        return null;
    }
    public string GetUnlockableDescriptionLocalizationKey(HordeModeProgressionManager.HordeModeUnlockables unlockable) {
        for (int i = 0; i < unlockableDataList.Count; i++) {
            if (unlockableDataList[i].unlockable == unlockable)
                return unlockableDataList[i].descriptionLocalizationKey;
        }

        return null;
    }
    private float GetXPProgressNormalized() {
        int totalXP = HordeModeProgressionManager.Instance.GetTotalXP();
        var nextUnlock = HordeModeProgressionManager.Instance.GetNextUnlockable();
        var previousUnlock = HordeModeProgressionManager.Instance.GetPreviousUnlockable();

        if (HordeModeProgressionManager.Instance.GetAllUnlocked()) {
            return 1f; // tout est unlock
        }


        int requiredXP = HordeModeProgressionManager.Instance.unlockThresholds[nextUnlock];
        int previousUnlockableXP = 0;

        if(previousUnlock != HordeModeProgressionManager.HordeModeUnlockables.None) {
            previousUnlockableXP = HordeModeProgressionManager.Instance.unlockThresholds[previousUnlock];
        }


        int diff = requiredXP - previousUnlockableXP;
        float progress = (float)(requiredXP - totalXP) / (float)diff;

        return 1-progress;
    }

    private bool AnyInputPressed() {
        return Input.anyKeyDown; // tu pourras raffiner si besoin
    }

}
