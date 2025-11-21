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
    }

    [SerializeField]
    private List<UnlockableData> unlockableDataList = new List<UnlockableData>();
    [SerializeField] private GameObject panelGO;
    [SerializeField] private Animator panelAnimator;
    [SerializeField] private Image xpProgressionBar;
    [SerializeField] private Image nextUnlockImage;
    [SerializeField] private TextMeshProUGUI nightsSurvivedAmountText;
    [SerializeField] private TextMeshProUGUI nextUnlockText;
    [SerializeField] private Material silhouetteMaterial;
    [SerializeField] private Material newUnlockMaterial;
    [SerializeField] private Animator newUnlockAnimator;
    [SerializeField] private Button nextUnlockButton;
    [SerializeField] private GameObject pressAnyKeyToContinueGO;
    [SerializeField] private float barFillSpeed;

    private string nextUnlockNameLocalizationKey;
    private bool panelOpen;
    private bool skipFill = false;
    private bool unlockSequenceCoroutineRunning;

    public event EventHandler OnProgressionBarStartFill;
    public event EventHandler OnProgressionBarEndFill;
    public event EventHandler OnNewItemUnlocked;

    private void Awake() {
        Instance = this;
        panelGO.SetActive(false);


        xpProgressionBar.fillAmount = ES3.Load("lastProgressionBarFillAmount", 0f);
    }

    private void Start() {
        nextUnlockText.font = LocalizationManager.Instance.GetCurrentFont();
        nextUnlockText.text = LocalizationManager.Instance.GetLocalizedText("Next Unlock");
        nightsSurvivedAmountText.font = LocalizationManager.Instance.GetCurrentFont();
        nightsSurvivedAmountText.text = LocalizationManager.Instance.GetLocalizedText("menu_nightsSurvived") + " " + ES3.Load("lastHordeModeNightsSurvived", 0);

        pressAnyKeyToContinueGO.SetActive(false);
        SetNextUnlockParameters(HordeModeProgressionManager.Instance.GetNextUnlockable());
        SetNextUnlockMaterialAndText();
    }

    private void Update() {
        if (!panelOpen) return;
        if(Input.anyKeyDown) {
            if (!unlockSequenceCoroutineRunning) {
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
        StartCoroutine(UnlockSequence());
        unlockSequenceCoroutineRunning = true;
    }

    private IEnumerator UnlockSequence() {
        panelGO.SetActive(true);
        panelAnimator.SetTrigger("Show");

        yield return new WaitForSeconds(.3f);

        skipFill = false;

        int totalXP = HordeModeProgressionManager.Instance.GetTotalXP();

        // Récupération des unlocks atteignables
        List<HordeModeProgressionManager.HordeModeUnlockables> pendingUnlocks = new List<HordeModeProgressionManager.HordeModeUnlockables>();

        foreach (var kvp in HordeModeProgressionManager.Instance.unlockThresholds) {
            if (!HordeModeProgressionManager.Instance.GetUnlocked(kvp.Key) && totalXP >= kvp.Value)
                pendingUnlocks.Add(kvp.Key);
        }

        // Séquence pour chaque unlock
        foreach (var unlock in pendingUnlocks) {
            pressAnyKeyToContinueGO.SetActive(false);
            SetNextUnlockParameters(unlock);
            SetNextUnlockMaterialAndText();

            yield return new WaitForEndOfFrame();

            // Animation de début
            newUnlockAnimator.ResetTrigger("Next");
            newUnlockAnimator.SetTrigger("Unlock");

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
            yield return new WaitForEndOfFrame();

            // Et on attend UN NOUVEAU CLIC pour passer à la suite
            yield return new WaitUntil(() => AnyInputPressed());

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

        OnProgressionBarEndFill?.Invoke(this, EventArgs.Empty);

        HordeModeProgressionManager.Instance.SetHasNoXPToCommit();
        unlockSequenceCoroutineRunning = false;

        Debug.Log("Unlock sequence completed.");

        yield return new WaitForSeconds(1f);
        pressAnyKeyToContinueGO.SetActive(true);
    }


    public void ClosePanel() {
        MusicManager.Instance.SetAudioVolume(1f);
        panelAnimator.SetTrigger("Hide");
        HordeModeUI.Instance.OpenHordeModePanel(false);
    }

    public void SetNextUnlockMaterialAndText() {
        nextUnlockImage.material = silhouetteMaterial;
        nextUnlockText.text = LocalizationManager.Instance.GetLocalizedText("Next Unlock");
    }

    public void SetNewUnlockMaterialAndText() {
        nextUnlockImage.material = newUnlockMaterial;
        nextUnlockText.text = LocalizationManager.Instance.GetLocalizedText(nextUnlockNameLocalizationKey) + " " + LocalizationManager.Instance.GetLocalizedText("Unlocked") + "!";
    }

    public void SetNextUnlockParameters(HordeModeProgressionManager.HordeModeUnlockables unlockable) {
        nextUnlockImage.sprite = GetUnlockableSprite(unlockable);
        nextUnlockNameLocalizationKey = GetUnlockableNameLocalizationKey(unlockable);
        newUnlockAnimator.ResetTrigger("Unlock");
        newUnlockAnimator.SetTrigger("Next");
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
    private float GetXPProgressNormalized() {
        int totalXP = HordeModeProgressionManager.Instance.GetTotalXP();
        var nextUnlock = HordeModeProgressionManager.Instance.GetNextUnlockable();

        if ((int)nextUnlock < 0)
            return 1f; // tout est unlock

        int requiredXP = HordeModeProgressionManager.Instance.unlockThresholds[nextUnlock];

        return Mathf.Clamp01((float)totalXP / requiredXP);
    }

    private bool AnyInputPressed() {
        return Input.anyKeyDown; // tu pourras raffiner si besoin
    }

#if UNITY_EDITOR
    [Sirenix.OdinInspector.Button]
    public void AutoGenerateUnlockableList() {
        unlockableDataList.Clear();

        foreach (var unlock in System.Enum.GetValues(typeof(HordeModeProgressionManager.HordeModeUnlockables))) {
            unlockableDataList.Add(new UnlockableData {
                unlockable = (HordeModeProgressionManager.HordeModeUnlockables)unlock,
                sprite = null,
                localizationKey = ""
            });
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
