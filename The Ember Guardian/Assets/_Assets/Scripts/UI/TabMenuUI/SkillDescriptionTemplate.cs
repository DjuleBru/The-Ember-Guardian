using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillDescriptionTemplate : ButtonUI
{
    private SkillSO linkedSkillSO;

    private bool isPassiveSkill;
    [SerializeField] private Image skillIcon;
    [SerializeField] private LevelUI_SkillDescriptionCardUI skillDescriptionCard;

    public void SetLinkedSkillSO(SkillSO skillSO) {
        linkedSkillSO = skillSO;
        skillIcon.sprite = skillSO.Icon;

        isPassiveSkill = linkedSkillSO.itemType == MerchantItem.MerchantItemType.PassiveSkill;
    }

    public void SetDesciptionCardText() {
        SkillItem skillItem = new SkillItem();
        skillItem.Initialize(linkedSkillSO);

        string skillName = linkedSkillSO.SkillName;
        List<string> statList = new List<string>();
        List<string> statDescriptionList = new List<string>();

        if (isPassiveSkill) {
            statList = PlayerSkills.Instance.GetPassiveSkillStatList(skillItem, true);
            statDescriptionList = PlayerSkills.Instance.GetPassiveSkillStatDescriptionList(skillItem);
        }
        else {
            statList = PlayerSkills.Instance.GetActiveSkillStatList(skillItem, true);
            statDescriptionList = PlayerSkills.Instance.GetActiveSkillStatDescriptionList(skillItem);
        }

        skillDescriptionCard.SetDescriptionCardText(skillName, statDescriptionList, statList);
    }

    public void OpenCloseSkillDescriptionCard(bool open) {
        if (open) {
            skillDescriptionCard.gameObject.SetActive(true);
            skillDescriptionCard.OpenDescriptionCard();
            SetDesciptionCardText();
            SetDescriptionCardPosition();
        }
        else {
            skillDescriptionCard.gameObject.SetActive(false);
        }
    }


    public void SetDescriptionCardPosition() {
        RectTransform callerRT = this.GetComponent<RectTransform>();
        RectTransform descRT = skillDescriptionCard.GetComponent<RectTransform>();

        // Récupérer la position mondiale du bord droit-bas de l'objet appelant
        Vector3 callerWorldPos = callerRT.position;

        // Calculer la largeur en world units du caller (via taille rect et scale)
        float distanceToSkill = 35f;

        if(isPassiveSkill) {
            distanceToSkill = 25f;
        }

        // Calculer la nouvelle position de la description card : 
        // On décale le long de X de la largeur (pour coller à droite)
        float heigt = 320;
        if (isPassiveSkill) {
            heigt = 250f;
        }

        Vector3 newDescPos = new Vector3(callerWorldPos.x + distanceToSkill, descRT.position.y, 0);

        // Positionner la description card à ce point
        descRT.position = newDescPos;
        descRT.sizeDelta = new Vector2(descRT.sizeDelta.x, heigt);
    }


    #region NAVIGATION

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
        OpenCloseSkillDescriptionCard(true);
    }

    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
        OpenCloseSkillDescriptionCard(false);
    }

    protected override void ButtonUI_OnAnyButtonSelected(object sender, EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;
        ButtonUI buttonUI = sender as ButtonUI;

        if (this == buttonUI) {
            buttonSelected = true;
            OpenCloseSkillDescriptionCard(true);
        }

        if (this != buttonUI && buttonSelected) {
            buttonSelected = false;
        }
    }

    #endregion
}
