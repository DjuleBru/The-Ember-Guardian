using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI_Skills : MonoBehaviour
{
    [SerializeField] private RectTransform activeSkillLeftRectTransform;
    [SerializeField] private RectTransform activeSkillRightRectTransform;

    [SerializeField] private RectTransform passiveSkillContainerRectTransform;
    [SerializeField] private RectTransform passiveSkillTemplateRectTransform;

    private void Start() {
        PlayerSkills.Instance.OnActiveSkillAdded += PlayerSkills_OnActiveSkillAdded;
        PlayerSkills.Instance.OnPassiveSkillAdded += PlayerSkills_OnPassiveSkillAdded;

        activeSkillLeftRectTransform.gameObject.SetActive(false);
        activeSkillRightRectTransform.gameObject.SetActive(false);
        passiveSkillTemplateRectTransform.gameObject.SetActive(false);
    }

    private void PlayerSkills_OnPassiveSkillAdded(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        RefreshPassiveSkillsUI();
    }

    private void PlayerSkills_OnActiveSkillAdded(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if(PlayerSkills.Instance.GetActiveSkillLeft() != null) {
            activeSkillLeftRectTransform.gameObject.SetActive(true);
            Debug.Log(PlayerSkills.Instance.GetActiveSkillLeft().skillType + " " + PlayerSkills.Instance.GetActiveSkillLeft().currentLevel);
            activeSkillLeftRectTransform.GetComponent<LevelUI_SkillUI>().SetLinkedSkill(PlayerSkills.Instance.GetActiveSkillLeft());
        }

        if (PlayerSkills.Instance.GetActiveSkillRight() != null) {
            activeSkillRightRectTransform.gameObject.SetActive(true);
            Debug.Log(PlayerSkills.Instance.GetActiveSkillRight().skillType + " " + PlayerSkills.Instance.GetActiveSkillRight().currentLevel);
            activeSkillRightRectTransform.GetComponent<LevelUI_SkillUI>().SetLinkedSkill(PlayerSkills.Instance.GetActiveSkillRight());
        }
    }

    private void RefreshPassiveSkillsUI() {
        foreach(RectTransform child in passiveSkillContainerRectTransform) {
            if(child == passiveSkillTemplateRectTransform) continue;
            Destroy(child.gameObject);
        }

        passiveSkillTemplateRectTransform.gameObject.SetActive(true);
        foreach (SkillItem skillItem in PlayerSkills.Instance.GetPassiveSkillList()) {
            LevelUI_SkillUI skillUI = Instantiate(passiveSkillTemplateRectTransform, passiveSkillContainerRectTransform).GetComponent<LevelUI_SkillUI>();
            skillUI.SetLinkedSkill(skillItem);
        }
        passiveSkillTemplateRectTransform.gameObject.SetActive(false);
    }

}
