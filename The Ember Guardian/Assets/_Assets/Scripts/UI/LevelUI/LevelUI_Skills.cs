using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI_Skills : MonoBehaviour
{
    [SerializeField] private RectTransform activeSkillLeftRectTransform;
    [SerializeField] private Button activeSkillLeftDeleteButton;
    [SerializeField] private RectTransform activeSkillRightRectTransform;
    [SerializeField] private Button activeSkillRightDeleteButton;

    [SerializeField] private RectTransform passiveSkillContainerRectTransform;
    [SerializeField] private RectTransform passiveSkillTemplateRectTransform;

    private Button activeSkillLeftButton;
    private Button activeSkillRightButton;
    private List<Button> passiveButtons = new List<Button>();

    private void Start() {
        PlayerSkills.Instance.OnActiveSkillAdded += PlayerSkills_OnActiveSkillAdded;
        PlayerSkills.Instance.OnActiveSkillRemoved += PlayerSkills_OnActiveSkillRemoved;
        PlayerSkills.Instance.OnPassiveSkillAdded += PlayerSkills_OnPassiveSkillAdded;

        activeSkillLeftRectTransform.gameObject.SetActive(false);
        activeSkillRightRectTransform.gameObject.SetActive(false);
        passiveSkillTemplateRectTransform.gameObject.SetActive(false);

        activeSkillLeftButton = activeSkillLeftRectTransform.GetComponent<Button>();
        activeSkillRightButton = activeSkillRightRectTransform.GetComponent<Button>();
    }

    private void PlayerSkills_OnPassiveSkillAdded(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        RefreshPassiveSkillsUI();
        UpdateNavigation();
    }

    private void PlayerSkills_OnActiveSkillAdded(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        RefreshActiveSkillsDisplay();
    }

    private void PlayerSkills_OnActiveSkillRemoved(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        RefreshActiveSkillsDisplay();
    }

    private void RefreshActiveSkillsDisplay() {
        if (PlayerSkills.Instance.GetActiveSkillLeft() != null) {
            activeSkillLeftRectTransform.gameObject.SetActive(true);
            activeSkillLeftRectTransform.GetComponent<LevelUI_SkillUI>().SetLinkedSkill(PlayerSkills.Instance.GetActiveSkillLeft());
        } else {
            activeSkillLeftRectTransform.gameObject.SetActive(false);
            activeSkillLeftRectTransform.GetComponent<LevelUI_SkillUI>().SetLinkedSkill(null);
        }

        if (PlayerSkills.Instance.GetActiveSkillRight() != null) {
            activeSkillRightRectTransform.gameObject.SetActive(true);
            activeSkillRightRectTransform.GetComponent<LevelUI_SkillUI>().SetLinkedSkill(PlayerSkills.Instance.GetActiveSkillRight());
        } else {
            activeSkillRightRectTransform.gameObject.SetActive(false);
            activeSkillRightRectTransform.GetComponent<LevelUI_SkillUI>().SetLinkedSkill(null);
        }

        UpdateNavigation();
    } 

    private void RefreshPassiveSkillsUI() {
        passiveButtons.Clear();

        foreach (RectTransform child in passiveSkillContainerRectTransform) {
            if(child == passiveSkillTemplateRectTransform) continue;
            Destroy(child.gameObject);
        }

        passiveSkillTemplateRectTransform.gameObject.SetActive(true);
        foreach (SkillItem skillItem in PlayerSkills.Instance.GetPassiveSkillList()) {
            LevelUI_SkillUI skillUI = Instantiate(passiveSkillTemplateRectTransform, passiveSkillContainerRectTransform).GetComponent<LevelUI_SkillUI>();
            skillUI.SetLinkedSkill(skillItem);
            passiveButtons.Add(skillUI.GetComponent<Button>());
        }
        passiveSkillTemplateRectTransform.gameObject.SetActive(false);
    }

    private void UpdateNavigation() {
        // ActiveSkillLeft
        if (activeSkillLeftButton.gameObject.activeSelf) {
            var nav = new Navigation { mode = Navigation.Mode.Explicit };
            var deleteButtonNav = new Navigation { mode = Navigation.Mode.Explicit };

            nav.selectOnUp = PlayerTabMenuUI.Instance.GetFirstSelectedButton().GetComponent<Button>();
            nav.selectOnDown = activeSkillLeftDeleteButton;
            deleteButtonNav.selectOnUp = activeSkillLeftButton;

            if (activeSkillRightButton.gameObject.activeSelf) {
                nav.selectOnRight = activeSkillRightButton;
                deleteButtonNav.selectOnRight = activeSkillRightButton;
            }

            else if (passiveButtons.Count > 0) {
                nav.selectOnRight = passiveButtons[0];
                deleteButtonNav.selectOnRight = passiveButtons[0];
            }

            activeSkillLeftButton.navigation = nav;
            activeSkillLeftDeleteButton.navigation = deleteButtonNav;

        }

        // ActiveSkillRight
        if (activeSkillRightButton.gameObject.activeSelf) {
            var nav = new Navigation { mode = Navigation.Mode.Explicit };
            var deleteButtonNav = new Navigation { mode = Navigation.Mode.Explicit };

            nav.selectOnUp = PlayerTabMenuUI.Instance.GetFirstSelectedButton().GetComponent<Button>();
            nav.selectOnDown = activeSkillRightDeleteButton;
            deleteButtonNav.selectOnUp = activeSkillRightButton;

            nav.selectOnLeft = activeSkillLeftButton.gameObject.activeSelf ? activeSkillLeftButton : null;
            deleteButtonNav.selectOnLeft = activeSkillLeftButton.gameObject.activeSelf ? activeSkillLeftButton : null;

            if (passiveButtons.Count > 0) {
                nav.selectOnRight = passiveButtons[0];
                deleteButtonNav.selectOnRight = passiveButtons[0];
            }


            activeSkillRightButton.navigation = nav;
            activeSkillRightDeleteButton.navigation = deleteButtonNav;
        }

        // Passive buttons
        for (int i = 0; i < passiveButtons.Count; i++) {
            var nav = new Navigation { mode = Navigation.Mode.Explicit };
            nav.selectOnUp = PlayerTabMenuUI.Instance.GetFirstSelectedButton().GetComponent<Button>();

            if (i == 0) {
                if (activeSkillRightButton.gameObject.activeSelf)
                    nav.selectOnLeft = activeSkillRightButton;
                else if (activeSkillLeftButton.gameObject.activeSelf)
                    nav.selectOnLeft = activeSkillLeftButton;
            }
            else {
                nav.selectOnLeft = passiveButtons[i - 1];
            }

            if (i < passiveButtons.Count - 1)
                nav.selectOnRight = passiveButtons[i + 1];

            passiveButtons[i].navigation = nav;
        }
    }
}
