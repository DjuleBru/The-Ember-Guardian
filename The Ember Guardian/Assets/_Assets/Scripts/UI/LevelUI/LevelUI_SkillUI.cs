using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI_SkillUI : MonoBehaviour
{
    private SkillItem linkedSkill;

    [SerializeField] private Image skillTemplateImage;
    [SerializeField] private RectTransform skillLevelRectTransform;
    [SerializeField] private TextMeshProUGUI skillLevelText;

    public void SetLinkedSkill(SkillItem skillItem) {
        linkedSkill = skillItem;
        RefreshSkillVisuals();
    }

    private void RefreshSkillVisuals() {
        skillTemplateImage.sprite = linkedSkill.icon;
        skillLevelText.text = linkedSkill.currentLevel.ToString();
    }
}
