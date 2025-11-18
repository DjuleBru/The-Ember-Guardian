using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HordeModeUI : MonoBehaviour
{
    public static HordeModeUI Instance;

    [SerializeField] private GameObject hordeModePanelGO;
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private Transform hordeModeMenuCameraTarget;
    [SerializeField] private TextMeshProUGUI levelEnvironmentText;

    private LevelSO.LevelEnvironment currentSelectedEnvironment;
    private List<LevelSO.LevelEnvironment> unlockedEnvironmentList;
    private bool panelOpen;
    private Animator panelAnimator;

    private void Awake() {
        Instance = this;
        hordeModePanelGO.gameObject.SetActive(false);
        panelAnimator = GetComponent<Animator>();
    }

    private void Start() {
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnEscapePerformed += GameInput_OnEscapePerformed;

        InitializeUnlockedEnvironments();
    }

    private void InitializeUnlockedEnvironments() {

        LevelSO.LevelEnvironment defaultEnvironment = LevelSO.LevelEnvironment.TheVerdantGraveyard;
        currentSelectedEnvironment = ES3.Load("lastLevelEnvironment", defaultEnvironment);

        unlockedEnvironmentList = new List<LevelSO.LevelEnvironment> { LevelSO.LevelEnvironment.TheVerdantGraveyard, LevelSO.LevelEnvironment.TheLostGreens };
        if (MetaProgressionManager.Instance.GetLevelRegionUnlocked(LevelSO.LevelEnvironment.CorruptedCity)) {
            unlockedEnvironmentList.Add(LevelSO.LevelEnvironment.CorruptedCity);
        }
        if (MetaProgressionManager.Instance.GetLevelRegionUnlocked(LevelSO.LevelEnvironment.TheLumenHollow)) {
            unlockedEnvironmentList.Add(LevelSO.LevelEnvironment.TheLumenHollow);
        }
        if (MetaProgressionManager.Instance.GetLevelRegionUnlocked(LevelSO.LevelEnvironment.TheFracturedDistrict)) {
            unlockedEnvironmentList.Add(LevelSO.LevelEnvironment.TheFracturedDistrict);
        }
    }

    private void GameInput_OnEscapePerformed(object sender, System.EventArgs e) {
        if (panelOpen) {
            CloseHordeModePanel();
        }
    }

    private void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        if (panelOpen) {
            CloseHordeModePanel();
        }
    }

    #region BUTTONS
    public void SetNextEnvironment() {
        if (unlockedEnvironmentList == null || unlockedEnvironmentList.Count == 0)
            return;

        int index = unlockedEnvironmentList.IndexOf(currentSelectedEnvironment);
        if (index < 0) index = 0;

        index = (index + 1) % unlockedEnvironmentList.Count;

        SetEnvironment(unlockedEnvironmentList[index]);
    }

    public void SetPreviousEnvironment() {
        if (unlockedEnvironmentList == null || unlockedEnvironmentList.Count == 0)
            return;

        int index = unlockedEnvironmentList.IndexOf(currentSelectedEnvironment);
        if (index < 0) index = 0;

        index = (index - 1 + unlockedEnvironmentList.Count) % unlockedEnvironmentList.Count;

        SetEnvironment(unlockedEnvironmentList[index]);
    }

    private void SetEnvironment(LevelSO.LevelEnvironment env) {
        currentSelectedEnvironment = env;

        levelEnvironmentText.text = LocalizationManager.Instance.GetLocalizedText(env.ToString());

        MainMenuVisual.Instance.SetEnvironment(env);
    }

    #endregion

    #region OpenClosePanel

    public void OpenHordeModePanel() {
        StartCoroutine(OpenHordeModePanelCoroutine());
    }

    private IEnumerator OpenHordeModePanelCoroutine() {
        CameraManager.Instance.ChangeCameraTarget(hordeModeMenuCameraTarget);
        CameraManager.Instance.ZoomIn(false, 1.3f);
        yield return new WaitForSeconds(1f);

        panelAnimator.SetTrigger("Show");

        yield return new WaitForSeconds(.5f);

        panelOpen = true;
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    private IEnumerator CloseHordeModePanelCoroutine() {
        CameraManager.Instance.ResetCameraTarget();
        CameraManager.Instance.ZoomOut(true);
        panelAnimator.SetTrigger("Hide");

        yield return new WaitForSeconds(1f);

        panelOpen = false;
        MainMenuUI.Instance.ShowMainMenuButtons();
    }

    public void CloseHordeModePanel() {
        StartCoroutine(CloseHordeModePanelCoroutine());
    }

    #endregion
}
