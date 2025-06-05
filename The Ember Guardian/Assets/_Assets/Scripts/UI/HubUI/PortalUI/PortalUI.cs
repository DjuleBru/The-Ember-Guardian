using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PortalUI : MonoBehaviour {

    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject firstButtonSelected;
    [SerializeField] private Portal portal;
    [SerializeField] private Animator portalUIAnimator;
    [SerializeField] private Animator portalDescriptionUIAnimator;

    private List<LevelSO> linkedLevelSOList;
    private List<LevelSO> unlockedLevelSOList = new List<LevelSO>();
    private LevelSO currentLevelSO;
    private int levelSOIndex;

    private Canvas canvas;

    private bool switchingLevel;
    private bool portalUIOpen;

    public static event EventHandler OnAnyPortalUIClosed;
    public static event EventHandler OnAnyPortalUIOpened;

    private void Awake() {
        canvas = GetComponent<Canvas>();
        mainPanel.SetActive(false);
    }

    private void Start() {
        canvas.worldCamera = CameraManager.Instance.GetUICamera();
        canvas.sortingLayerName = "UI";

        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
        GameInput.Instance.OnPlayerPausePerformed += GameInput_OnPlayerPausePerformed;
        portal.OnPlayerInteractedWithPortalFromHub += Portal_OnPlayerInteractedWithPortalFromHub;
        portal.OnPlayerMovedOnTeleporter += Portal_OnPlayerMovedOnTeleporter;

        linkedLevelSOList = portal.GetLinkedLevelSOList();
        foreach(LevelSO levelSO in linkedLevelSOList) {
            if(MetaProgressionManager.Instance.GetLevelUnlocked(levelSO)) {
                unlockedLevelSOList.Add(levelSO);
            }
        }
    }


    private void GameInput_OnPlayerPausePerformed(object sender, EventArgs e) {
        if (!portalUIOpen) return;
        ClosePanel();
    }

    private void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        if (!portalUIOpen) return;
        ClosePanel();
    }

    private void Portal_OnPlayerInteractedWithPortalFromHub(object sender, System.EventArgs e) {
        OpenPanel();
    }
    private void Portal_OnPlayerMovedOnTeleporter(object sender, EventArgs e) {
        ClosePanel(false);
    }

    private void OpenPanel() {
        portalUIOpen = true;
        portal.SetPlayerOpenedPortalUI(true);

        mainPanel.SetActive(true);
        portalUIAnimator.ResetTrigger("Hide");
        portalUIAnimator.SetTrigger("Show");

        EventSystem.current.SetSelectedGameObject(firstButtonSelected);

        Player.Instance.SetInTeleporterLevelSelectionMenu(true);
        PauseMenuUI.Instance.SetCanOpenPauseMenu(false);

        CameraManager.Instance.ZoomIn(false, 1.2f, 1f);
        CameraManager.Instance.ChangeCameraTarget(portal.GetLevelSelectionCameraTarget());

        OnAnyPortalUIOpened?.Invoke(this, EventArgs.Empty);
    }

    private void ClosePanel(bool zoomOut = true) {
        portalUIOpen = false;
        portal.SetPlayerOpenedPortalUI(false);
        portalUIAnimator.ResetTrigger("Show");
        portalUIAnimator.SetTrigger("Hide");

        EventSystem.current.SetSelectedGameObject(null);

        if(zoomOut) {
            CameraManager.Instance.ZoomOut(true);
        } else {
            CameraManager.Instance.ResetCameraOrthographicSize();
        }
        CameraManager.Instance.ResetCameraTargetToPlayer();

        Player.Instance.SetInTeleporterLevelSelectionMenu(false);
        PauseMenuUI.Instance.SetCanOpenPauseMenuAfterFrame(true);
        OnAnyPortalUIClosed?.Invoke(this, EventArgs.Empty);
    }

    public void PreviousLevel() {
        if (switchingLevel) return;
        if (levelSOIndex == 0) return;

        StartCoroutine(PreviousLevelCoroutine());
    }

    private IEnumerator PreviousLevelCoroutine() {
        switchingLevel = true;
        portalDescriptionUIAnimator.SetTrigger("Previous");

        yield return new WaitForSeconds(.1f);

        levelSOIndex--;
        currentLevelSO = linkedLevelSOList[levelSOIndex];
        portal.SetLinkedLevelSO(currentLevelSO);

        switchingLevel = false;
    }

    public void NextLevel() {
        if (switchingLevel) return;
        if (levelSOIndex == linkedLevelSOList.Count-1) return;

        StartCoroutine(NextLevelCoroutine());
    }

    private IEnumerator NextLevelCoroutine() {
        switchingLevel = true;
        portalDescriptionUIAnimator.SetTrigger("Next");

        yield return new WaitForSeconds(.1f);

        levelSOIndex++;
        currentLevelSO = linkedLevelSOList[levelSOIndex];
        portal.SetLinkedLevelSO(currentLevelSO);

        switchingLevel = false;
    }
}
