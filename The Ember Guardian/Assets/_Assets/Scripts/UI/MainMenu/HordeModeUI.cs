using System;
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
    [SerializeField] private GameObject customizeCampButtonWorlUI;
    [SerializeField] private GameObject swapWeaponButtonWorlUI;
    [SerializeField] private GameObject SwapDogButtonWorlUI;
    [SerializeField] private ChangeWeaponPanel_HordeMode changeWeaponPanel;
    [SerializeField] private ChangeDogPanel_HordeMode changeDogPanel;
    [SerializeField] private ArchitectTable_MainMenu architectTable;
    [SerializeField] private Transform hordeModeMenuCameraTarget;
    [SerializeField] private TextMeshProUGUI levelEnvironmentText;
    [SerializeField] private List<CanvasGroup> allUICanvasGroups;

    private LevelSO.LevelEnvironment currentSelectedEnvironment;
    private List<LevelSO.LevelEnvironment> unlockedEnvironmentList;
    private bool panelOpen;
    private bool changeWeaponPanelOpen;
    private bool changeDogPanelOpen;
    private bool customizeCampPanelOpen;
    private Animator panelAnimator;

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
        panelAnimator = GetComponent<Animator>();
    }

    private void Start() {
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnEscapePerformed += GameInput_OnEscapePerformed;
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;

        InitializeUnlockedEnvironments();
    }

    private void GameInput_OnPlayerInputChanged(object sender, EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;

        if(panelOpen) {
            EventSystem.current.SetSelectedGameObject(swapWeaponButtonWorlUI);
        }
    }

    private void Update() {
        if (!panelOpen) return;

        if(Input.GetMouseButtonDown(0) ||  Input.GetMouseButtonDown(1)) {
            if (!EventSystem.current.IsPointerOverGameObject()) {

                if(changeWeaponPanelOpen) {
                    CloseChangeWeaponPanel();
                }
                if (changeDogPanelOpen) {
                    CloseChangeDogPanel();
                }

            }
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

    public void OpenCloseChangeWeaponPanel() {
        if (changeDogPanelOpen) {
            CloseChangeDogPanel();
        }

        changeWeaponPanel.OpenClosePanel(true);
        changeWeaponPanelOpen = !changeWeaponPanelOpen;
    }

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

    public void OpenCloseChangeDogPanel() {
        if(changeWeaponPanelOpen) {
            CloseChangeWeaponPanel();
        }

        changeDogPanel.OpenClosePanel();
        changeDogPanelOpen = !changeDogPanelOpen;
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

        foreach(CanvasGroup canvasGroup in allUICanvasGroups) {
            if(customizeCampPanelOpen) {
                canvasGroup.alpha = 0f;
            } else {
                canvasGroup.alpha = 1.0f;
            }

        }
    }

    private void CloseChangeDogPanel() {
        changeDogPanel.OpenClosePanel();
        changeDogPanelOpen = false;
    }

    private void CloseChangeWeaponPanel() {
        changeWeaponPanel.OpenClosePanel(true);
        changeWeaponPanelOpen = false;
    }
    private void CloseCustomizeCampPanel() {
        architectTable.OpenCloseCustomizeCampPanel();
        customizeCampPanelOpen = !customizeCampPanelOpen;

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
        customizeCampButtonWorlUI.gameObject.SetActive(true);
        SwapDogButtonWorlUI.gameObject.SetActive(true);
        swapWeaponButtonWorlUI.gameObject.SetActive(true);

        yield return new WaitForSeconds(.5f);

        panelOpen = true;
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }

    private IEnumerator CloseHordeModePanelCoroutine() {
        CameraManager.Instance.ResetCameraTarget();
        CameraManager.Instance.ZoomOut(true);
        panelAnimator.SetTrigger("Hide");
        customizeCampButtonWorlUI.gameObject.SetActive(false);
        SwapDogButtonWorlUI.gameObject.SetActive(false);
        swapWeaponButtonWorlUI.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        panelOpen = false;
        MainMenuUI.Instance.ShowMainMenuButtons();
    }

    public void CloseHordeModePanel() {
        StartCoroutine(CloseHordeModePanelCoroutine());
    }

    #endregion
}
