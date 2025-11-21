using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HordeModeUI : MonoBehaviour
{
    public static HordeModeUI Instance;

    [SerializeField] private GameObject hordeModePanelGO;
    [SerializeField] private GameObject customizeCampButtonWorlUI;
    [SerializeField] private GameObject swapWeaponButtonWorlUI;
    [SerializeField] private GameObject SwapDogButtonWorlUI;

    [SerializeField] private ChangeWeaponPanel_HordeMode changeWeaponPanel;
    [SerializeField] private ChangeDogPanel_HordeMode changeDogPanel;
    [SerializeField] private ArchitectTable_MainMenu architectTable;
    [SerializeField] private CustomizeHordeModeEquipmentButton changeWeaponCustomizable;
    [SerializeField] private CustomizeHordeModeEquipmentButton changeDogCustomizable;
    [SerializeField] private CustomizeHordeModeEquipmentButton architectTableCustomizable;

    [SerializeField] private Transform hordeModeMenuCameraTarget;
    [SerializeField] private TextMeshProUGUI levelEnvironmentText;
    [SerializeField] private List<CanvasGroup> allUICanvasGroups;

    [SerializeField] protected TextMeshProUGUI maxNightsSurvivedText;
    [SerializeField] private Animator panelAnimator;

    private LevelSO.LevelEnvironment currentSelectedEnvironment;
    private List<LevelSO.LevelEnvironment> unlockedEnvironmentList;
    private bool panelOpen;
    private bool changeWeaponPanelOpen;
    private bool changeDogPanelOpen;
    private bool customizeCampPanelOpen;

    private bool weaponCustomizationUnlocked;
    private bool dogCustomizationUnlocked;
    private bool campCustomizationUnlocked;

    private GunSO.GunType selectedGunType;
    public event EventHandler OnWeaponSelected;
    private Dog.DogType selectedDogType;
    public event EventHandler OnDogSelected;

    private void Awake() {
        Instance = this;
        hordeModePanelGO.gameObject.SetActive(false);
        customizeCampButtonWorlUI.gameObject.SetActive(false);
        SwapDogButtonWorlUI.gameObject.SetActive(false);
        swapWeaponButtonWorlUI.gameObject.SetActive(false);
    }

    private void Start() {
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnEscapePerformed += GameInput_OnEscapePerformed;
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;

        maxNightsSurvivedText.font = LocalizationManager.Instance.GetCurrentFont();
        maxNightsSurvivedText.text = LocalizationManager.Instance.GetLocalizedText("menu_maxNightsSurvived");

        InitializeUnlockedEnvironments();
        LoadUnlockedCustomizationOptions();
    }

    private void Update() {
        if (!panelOpen) return;

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
            if (!EventSystem.current.IsPointerOverGameObject()) {

                if (changeWeaponPanelOpen) {
                    CloseChangeWeaponPanel();
                }
                if (changeDogPanelOpen) {
                    CloseChangeDogPanel();
                }

            }
        }
    }

    private void LoadUnlockedCustomizationOptions() {
        dogCustomizationUnlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.GoldenRetreiver);
        campCustomizationUnlocked = HordeModeProgressionManager.Instance.GetUnlocked(HordeModeProgressionManager.HordeModeUnlockables.ArchitectTable);

        changeWeaponCustomizable.SetCustomizable(true);
        changeDogCustomizable.SetCustomizable(dogCustomizationUnlocked);
        architectTableCustomizable.SetCustomizable(campCustomizationUnlocked);

        Navigation nav = changeWeaponCustomizable.GetComponent<Button>().navigation;
        if (!changeDogCustomizable) {
            nav.selectOnRight = null;
        }
        if (!architectTableCustomizable) {
            nav.selectOnLeft = null;
        }
        changeWeaponCustomizable.GetComponent<Button>().navigation = nav;
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;

        if(panelOpen) {
            EventSystem.current.SetSelectedGameObject(swapWeaponButtonWorlUI);
        }
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
        BackOrEscape();
    }

    private void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        BackOrEscape();
    }

    private void BackOrEscape() {
        if (customizeCampPanelOpen) {
            CloseCustomizeCampPanel();
            if (GameInput.Instance.IsUsingGamepad()) {
                EventSystem.current.SetSelectedGameObject(SwapDogButtonWorlUI);
            }
            return;
        }

        if (changeDogPanelOpen) {
            CloseChangeDogPanel();
            if (GameInput.Instance.IsUsingGamepad()) {
                EventSystem.current.SetSelectedGameObject(SwapDogButtonWorlUI);
            }
            return;
        }

        if (changeWeaponPanelOpen) {
            CloseChangeWeaponPanel();
            if (GameInput.Instance.IsUsingGamepad()) {
                EventSystem.current.SetSelectedGameObject(swapWeaponButtonWorlUI);
            }
            return;
        }

        if (panelOpen) {
            CloseHordeModePanel();
        }
    }

    #region CUSTOMIZATION BUTTONS
    public void SetSelectedWeapon(GunSO gunSO) {
        selectedGunType = gunSO.gunType;
        OnWeaponSelected?.Invoke(this, EventArgs.Empty);

        if(GameInput.Instance.IsUsingGamepad()) {
            EventSystem.current.SetSelectedGameObject(swapWeaponButtonWorlUI);
        }

        CloseChangeWeaponPanel();
    }

    public GunSO.GunType GetSelectedGunType() {
        return selectedGunType;
    }

    public void OpenCloseChangeWeaponPanel() {
        if (changeDogPanelOpen) {
            CloseChangeDogPanel();
        }

        changeWeaponPanel.OpenClosePanel(true);
        changeWeaponPanelOpen = !changeWeaponPanelOpen;


        if (changeWeaponPanelOpen) {
            changeWeaponCustomizable.SetSelected(true);
        } else {
            changeWeaponCustomizable.SetSelected(false);
        }
    }

    public void OpenCloseChangeDogPanel() {
        if(changeWeaponPanelOpen) {
            CloseChangeWeaponPanel();
        }

        changeDogPanel.OpenClosePanel();
        changeDogPanelOpen = !changeDogPanelOpen;

        if(changeDogPanelOpen) {
            changeDogCustomizable.SetSelected(true);
        } else {
            changeDogCustomizable.SetSelected(false);
        }
    }

    public void OpenCloseCustomizeCampPanel() {
        if (changeDogPanelOpen) {
            CloseChangeDogPanel();
        }

        if (changeWeaponPanelOpen) {
            CloseChangeWeaponPanel();
        }


        architectTable.OpenCloseCustomizeCampPanel();
        customizeCampPanelOpen = !customizeCampPanelOpen;

        foreach (CanvasGroup canvasGroup in allUICanvasGroups) {
            if(customizeCampPanelOpen) {
                canvasGroup.alpha = 0f;
                //architectTableCustomizable.SetSelected(true);
            } else {
                canvasGroup.alpha = 1.0f;
                architectTableCustomizable.SetSelected(false);
            }
        }
    }

    private void CloseChangeDogPanel() {
        changeDogPanel.OpenClosePanel();
        changeDogPanelOpen = false;
        changeDogCustomizable.SetSelected(false);
    }

    private void CloseChangeWeaponPanel() {
        changeWeaponPanel.OpenClosePanel(true);
        changeWeaponPanelOpen = false;
        changeWeaponCustomizable.SetSelected(false);
    }

    private void CloseCustomizeCampPanel() {
        architectTable.OpenCloseCustomizeCampPanel();
        customizeCampPanelOpen = false;
        architectTableCustomizable.SetSelected(false);

        foreach (CanvasGroup canvasGroup in allUICanvasGroups) {
            canvasGroup.alpha = 1.0f;
        }
    }

    public void SetSelectedDog(Dog.DogType dogType) {
        selectedDogType = dogType;
        OnDogSelected?.Invoke(this, EventArgs.Empty);

        if (GameInput.Instance.IsUsingGamepad()) {
            EventSystem.current.SetSelectedGameObject(SwapDogButtonWorlUI);
        }

        CloseChangeDogPanel();
    }

    public Dog.DogType GetSelectedDogType() {
        return selectedDogType;
    }

    public bool GetCustomizeCampPanelOpen() {
        return customizeCampPanelOpen;
    }
    #endregion

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

        string key = "hordeMode_maxNightsSurvived_" + env.ToString();
        int maxNightsSurvivedInEnvironment = ES3.Load(key, 0);
        maxNightsSurvivedText.text = LocalizationManager.Instance.GetLocalizedText("menu_maxNightsSurvived") + " " + maxNightsSurvivedInEnvironment;

        MainMenuVisual.Instance.SetEnvironment(env);
    }

    public void StartNewHordeMode() {
        SaveHordeModeParameters();
        MainMenuUI.Instance.StartHordeMode();
    }

    #endregion

    #region OpenClosePanel

    public void OpenHordeModePanel(bool changeCameraTarget = true) {
        Debug.Log("OpenHordeModePanel");
        StartCoroutine(OpenHordeModePanelCoroutine(changeCameraTarget));
    }

    private IEnumerator OpenHordeModePanelCoroutine(bool changeCameraTarget = true) {
        if(changeCameraTarget) {
            CameraManager.Instance.ChangeCameraTarget(hordeModeMenuCameraTarget);
            CameraManager.Instance.ZoomIn(false, 1.3f);
        }

        SetEnvironment(MainMenuVisual.Instance.GetLevelEnvironment());

        yield return new WaitForSeconds(1f);

        if(!HordeModeProgressionManager.Instance.GetHasXPToCommit()) {

            hordeModePanelGO.gameObject.SetActive(true);
            panelAnimator.SetTrigger("Show");

            if (campCustomizationUnlocked) {
                customizeCampButtonWorlUI.gameObject.SetActive(true);
            }
            if (dogCustomizationUnlocked) {
                SwapDogButtonWorlUI.gameObject.SetActive(true);
            }

            swapWeaponButtonWorlUI.gameObject.SetActive(true);

            architectTableCustomizable.SetCustomizing(true);
            changeWeaponCustomizable.SetCustomizing(true);
            changeDogCustomizable.SetCustomizing(true);
            EventSystem.current.SetSelectedGameObject(swapWeaponButtonWorlUI);
            panelOpen = true;

        } else {

            HordeModeRewardsMenu.Instance.OpenPanelAndCommitXP();

        }
       
    }

    private IEnumerator CloseHordeModePanelCoroutine() {
        CameraManager.Instance.ResetCameraTarget();
        CameraManager.Instance.ZoomOut(true);
        panelAnimator.SetTrigger("Hide");

        customizeCampButtonWorlUI.gameObject.SetActive(false);
        SwapDogButtonWorlUI.gameObject.SetActive(false);
        swapWeaponButtonWorlUI.gameObject.SetActive(false);
        architectTableCustomizable.SetCustomizing(false);
        changeWeaponCustomizable.SetCustomizing(false);
        changeDogCustomizable.SetCustomizing(false);

        yield return new WaitForSeconds(1f);

        panelOpen = false;
        MainMenuUI.Instance.ShowMainMenuButtons();
    }

    public void CloseHordeModePanel() {
        StartCoroutine(CloseHordeModePanelCoroutine());
    }

    #endregion


    public void SaveHordeModeParameters() {
        ES3.Save("currentHordeModeLevelEnvironment", currentSelectedEnvironment);
        ES3.Save("currentHordeModeWeaponSelected", selectedGunType);
        ES3.Save("currentHordeModeDogSelected", selectedDogType);

        CampEditManager.Instance.SaveCampLayout();
    }
}
