using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillsDescriptionPanel : MonoBehaviour
{
    public static SkillsDescriptionPanel Instance;

    private bool openedOnce;
    private bool panelOpen;
    private bool displayingActiveSkills;

    [SerializeField] private Transform activeSkillSlotContainer;
    [SerializeField] private Transform activeSkillSlotTemplate;
    [SerializeField] private Transform emptyActiveSkillSlotTemplate;
    [SerializeField] private Transform passiveSkillSlotContainer;
    [SerializeField] private Transform passiveSkillSlotTemplate;
    [SerializeField] private Transform emptyPassiveSkillSlotTemplate;

    private List<GameObject> displayedSkillsButtons;

    public event EventHandler OnSkillsDescriptionPanelClosed;
    public event EventHandler OnNewSkillsDescriptionPanelOpened;
    public event EventHandler OnSkillsDescriptionPanel;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        PlayerTabMenuUI.Instance.OnPlayerTabClosed += PlayerTabMenuUI_OnPlayerTabClosed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;

        gameObject.SetActive(false);
        UpdateSkillSlots(false);
    }

    public void SetDisplayingActiveSkills(bool displayActiveSkills) {
        displayingActiveSkills = displayActiveSkills;
        RefreshPanelSize();
    }

    public void UpdateSkillSlots(bool displayActiveSkills) {
        activeSkillSlotContainer.gameObject.SetActive(false);
        passiveSkillSlotContainer.gameObject.SetActive(false);
        activeSkillSlotTemplate.gameObject.SetActive(false);
        passiveSkillSlotTemplate.gameObject.SetActive(false);
        emptyActiveSkillSlotTemplate.gameObject.SetActive(false);
        emptyPassiveSkillSlotTemplate.gameObject.SetActive(false);

        if (displayActiveSkills) {
            RefreshActiveSkills();
        } else {
            RefreshPassiveSkills();
        }
    }

    private void RefreshActiveSkills() {
        activeSkillSlotContainer.gameObject.SetActive(true);
        activeSkillSlotTemplate.gameObject.SetActive(true);
        emptyActiveSkillSlotTemplate.gameObject.SetActive(true);
        displayedSkillsButtons = new List<GameObject>();

        foreach (Transform child in activeSkillSlotContainer) {
            if (child == activeSkillSlotTemplate || child == emptyActiveSkillSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        int unlockedSkillAmount = 0;
        List<SkillSO> activeSkillsUnlocked = PlayerSave.Instance.GetActiveSkillsUnlocked();
        List<SkillSO> allActiveSkills = PlayerSave.Instance.GetAllActiveSkills();

        foreach (SkillSO skillSO in activeSkillsUnlocked) {
            SkillDescriptionTemplate skillDescrptionTemplate = Instantiate(activeSkillSlotTemplate, activeSkillSlotContainer).GetComponent<SkillDescriptionTemplate>();

            skillDescrptionTemplate.SetLinkedSkillSO(skillSO);
            displayedSkillsButtons.Add(skillDescrptionTemplate.gameObject);
            unlockedSkillAmount++;
        }


        for (int i = 0; i < allActiveSkills.Count - unlockedSkillAmount; i++) {
            Instantiate(emptyActiveSkillSlotTemplate, activeSkillSlotContainer);
        }

        activeSkillSlotTemplate.gameObject.SetActive(false);
        emptyActiveSkillSlotTemplate.gameObject.SetActive(false);
    }
    private void RefreshPassiveSkills() {
        passiveSkillSlotContainer.gameObject.SetActive(true);
        passiveSkillSlotTemplate.gameObject.SetActive(true);
        emptyPassiveSkillSlotTemplate.gameObject.SetActive(true);
        displayedSkillsButtons = new List<GameObject>();

        foreach (Transform child in passiveSkillSlotContainer) {
            if (child == passiveSkillSlotTemplate || child == emptyPassiveSkillSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        int unlockedSkillAmount = 0;
        List<SkillSO> passiveSkillsUnlocked = PlayerSave.Instance.GetPassiveSkillsUnlocked();
        List<SkillSO> allPassiveSkills = PlayerSave.Instance.GetAllPassiveSkills();

        foreach (SkillSO skillSO in passiveSkillsUnlocked) {
            SkillDescriptionTemplate skillDescrptionTemplate = Instantiate(passiveSkillSlotTemplate, passiveSkillSlotContainer).GetComponent<SkillDescriptionTemplate>();

            skillDescrptionTemplate.SetLinkedSkillSO(skillSO);
            displayedSkillsButtons.Add(skillDescrptionTemplate.gameObject);
            unlockedSkillAmount++;
        }


        for (int i = 0; i < allPassiveSkills.Count - unlockedSkillAmount; i++) {
            Instantiate(emptyPassiveSkillSlotTemplate, passiveSkillSlotContainer);
        }

        passiveSkillSlotTemplate.gameObject.SetActive(false);
        emptyPassiveSkillSlotTemplate.gameObject.SetActive(false);
    }
    private void PlayerTabMenuUI_OnPlayerTabClosed(object sender, EventArgs e) {
        if (panelOpen) {
            ClosePanel();
        }
    }

    private void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        if (panelOpen) {
            ClosePanel();
        }
    }

    public void OpenClosePanel(bool calledFromActiveSkillButton) {

        bool openedAgain = false;

        if (panelOpen) {
            if (!openedOnce || (displayingActiveSkills && calledFromActiveSkillButton) || (!displayingActiveSkills && !calledFromActiveSkillButton)) {

                OnSkillsDescriptionPanelClosed?.Invoke(this, EventArgs.Empty);
                panelOpen = false;
                gameObject.SetActive(false);
            }

        }

        else {

            ChangeWeaponPanel.Instance.ClosePanel();

            openedOnce = true;
            panelOpen = true;
            gameObject.SetActive(true);
            if (GameInput.Instance.IsUsingGamepad()) {
                displayedSkillsButtons[0].gameObject.SetActive(true);
                EventSystem.current.SetSelectedGameObject(displayedSkillsButtons[0]);
                StartCoroutine(RefreshDescriptionCardPositionAfterFrame());
            };

            OnNewSkillsDescriptionPanelOpened?.Invoke(this, EventArgs.Empty);

            openedAgain = true;
        }

        if (!openedAgain && (!openedOnce || (displayingActiveSkills && !calledFromActiveSkillButton) || (!displayingActiveSkills && calledFromActiveSkillButton))) {
            OnNewSkillsDescriptionPanelOpened?.Invoke(this, EventArgs.Empty);
        }
    }

    private IEnumerator RefreshDescriptionCardPositionAfterFrame() {
        yield return new WaitForEndOfFrame();
        displayedSkillsButtons[0].GetComponent<SkillDescriptionTemplate>().OpenCloseSkillDescriptionCard(true);
        displayedSkillsButtons[0].GetComponent<SkillDescriptionTemplate>().SetDescriptionCardPosition();
    }

    private void RefreshPanelSize() {
        if(displayingActiveSkills) {
            GetComponent<RectTransform>().sizeDelta = new Vector2(350,280);
        } else {

            GetComponent<RectTransform>().sizeDelta = new Vector2(310, 210);
        }
    }

    public void ClosePanel() {
        panelOpen = false;
        gameObject.SetActive(panelOpen);

        OnSkillsDescriptionPanelClosed?.Invoke(this, EventArgs.Empty);
    }

}
