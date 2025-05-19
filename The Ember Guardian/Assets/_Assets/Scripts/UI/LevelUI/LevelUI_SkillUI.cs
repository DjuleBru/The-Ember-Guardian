using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelUI_SkillUI : ButtonUI, IPointerEnterHandler, IPointerExitHandler
{
    private SkillItem linkedSkill;
    [SerializeField] private bool isLeftActiveSkill;
    [SerializeField] private bool isRightActiveSkill;

    [SerializeField] private Animator skillTemplateAnimator;
    [SerializeField] private Image skillTemplateImage;
    [SerializeField] private Image skillTemplateBackgroundImage;
    [SerializeField] private Image skillTemplateOutlineImage;
    [SerializeField] private RectTransform skillLevelRectTransform;
    [SerializeField] private TextMeshProUGUI skillLevelText;
    [SerializeField] private LevelUI_SkillDescriptionCardUI skillDescriptionCard;

    private bool skillReady;

    protected override void Start() {
        base.Start();
        PlayerSkills.Instance.OnLeftActiveSkillActivated += PlayerSkills_OnLeftActiveSkillActivated;
        PlayerSkills.Instance.OnRightActiveSkillActivated += PlayerSkills_OnRightActiveSkillActivated;
        PlayerSkills.Instance.OnLeftActiveSkillDeactivated += PlayerSkills_OnLeftActiveSkillDeactivated;
        PlayerSkills.Instance.OnRightActiveSkillDeactivated += PlayerSkills_OnRightActiveSkillDeactivated;

        OnAnyButtonHovered += LevelUI_SkillUI_OnAnyButtonHovered;
        OnAnyButtonSelected += LevelUI_SkillUI_OnAnyButtonSelected;
    }

    public void SetLinkedSkill(SkillItem skillItem) {
        linkedSkill = skillItem;
        RefreshSkillVisuals();
    }

    private void RefreshSkillVisuals() {
        skillTemplateImage.sprite = linkedSkill.skillSO.Icon;
        skillTemplateBackgroundImage.sprite = linkedSkill.skillSO.Icon;
        Debug.Log(linkedSkill.skillSO);
        Debug.Log(linkedSkill.skillSO.Icon);
        skillLevelText.text = linkedSkill.currentLevel.ToString();
    }

    private void Update() {
        if(isRightActiveSkill|| isLeftActiveSkill) {
            HandleActiveSkillCooldownVisuals();
        } else {
            HandlePassiveSkillCooldownVisuals();
        }

    }

    private void HandleActiveSkillCooldownVisuals() {
        float cooldownNormalized = 1f;

        if (isRightActiveSkill) {
            cooldownNormalized = PlayerSkills.Instance.GetRightActiveTimerNormalized();
        } else {
            cooldownNormalized = PlayerSkills.Instance.GetLeftActiveTimerNormalized();
        }

        if (cooldownNormalized >= 1f && !skillReady) {

            skillReady = true;
            cooldownNormalized = 1f;
            skillTemplateAnimator.SetBool("Ready", true);

        }
        if(cooldownNormalized < 1f && skillReady) { 

            if (skillReady) {
                skillReady = false;
                skillTemplateAnimator.SetBool("Ready", false);
            }

        }

        skillTemplateImage.fillAmount = cooldownNormalized;
    }

    private void HandlePassiveSkillCooldownVisuals() {
        //float cooldownNormalized = 1f;
        //switch(linkedSkill.skillType) {

        //    case SkillItem.SkillType.activeMoveSpeedBuff:
        //        cooldownNormalized = PlayerSkills.Instance.Get
        //    break;
        //}

        //skillTemplateImage.fillAmount = cooldownNormalized;
    }

    private void PlayerSkills_OnRightActiveSkillDeactivated(object sender, System.EventArgs e) {
        skillTemplateAnimator.SetBool("Active", false);
    }

    private void PlayerSkills_OnLeftActiveSkillDeactivated(object sender, System.EventArgs e) {
        skillTemplateAnimator.SetBool("Active", false);
    }

    private void PlayerSkills_OnRightActiveSkillActivated(object sender, System.EventArgs e) {
        skillTemplateAnimator.SetBool("Active", true);
    }

    private void PlayerSkills_OnLeftActiveSkillActivated(object sender, System.EventArgs e) {
        skillTemplateAnimator.SetBool("Active", true);
    }



    #region UI NAVIGATION
    protected override void ButtonUI_OnAnyButtonSelected(object sender, EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;
        ButtonUI buttonUI = sender as ButtonUI;

        if (this == buttonUI) {
            buttonSelected = true;
        }

        if (this != buttonUI && buttonSelected) {
            buttonSelected = false;
        }
    }
    private void LevelUI_SkillUI_OnAnyButtonSelected(object sender, System.EventArgs e) {
        if (!(sender as ButtonUI is LevelUI_SkillUI)) {
            OpenCloseSkillDescriptionCard(false);
            return;
        }
        if (sender as ButtonUI != this) return;
        OpenCloseSkillDescriptionCard(true);
        SetDescriptionCardPosition();
    }

    private void LevelUI_SkillUI_OnAnyButtonHovered(object sender, System.EventArgs e) {
        if (sender as ButtonUI != this) return;
        OpenCloseSkillDescriptionCard(true);
        SetDescriptionCardPosition();
    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
        OpenCloseSkillDescriptionCard(false);
    }

    private void OpenCloseSkillDescriptionCard(bool open) {
        if(open) {
            skillDescriptionCard.gameObject.SetActive(true);
            skillDescriptionCard.OpenDescriptionCard();
            SetDesciptionCardText();
        } else {
            skillDescriptionCard.gameObject.SetActive(false);
        }
    }

    public void SetDescriptionCardPosition() {
        RectTransform callerRT = this.GetComponent<RectTransform>();
        RectTransform descRT = skillDescriptionCard.GetComponent<RectTransform>();

        // Récupérer la position mondiale du bord droit-bas de l'objet appelant
        Vector3 callerWorldPos = callerRT.position;

        // Calculer la largeur en world units du caller (via taille rect et scale)
        float distanceToSkill = .2f;

        // Calculer la nouvelle position de la description card : 
        // On décale le long de X de la largeur (pour coller à droite)
        float heigt = 280f;
        if (!isLeftActiveSkill && !isRightActiveSkill) {
            heigt = 230f;
        }

        Vector3 newDescPos = new Vector3(callerWorldPos.x + distanceToSkill, descRT.position.y, 0);

        // Positionner la description card à ce point
        descRT.position = newDescPos;
        descRT.sizeDelta = new Vector2(descRT.sizeDelta.x, heigt);
    }

    private void SetDesciptionCardText() {

        string skillName = LocalizationManager.Instance.GetLocalizedText(linkedSkill.skillSO.SkillName);
        List<string> statList = new List<string>();
        List<string> statDescriptionList = new List<string>();

        if (!isLeftActiveSkill && !isRightActiveSkill) {
            statList = PlayerSkills.Instance.GetPassiveSkillStatList(linkedSkill);
            statDescriptionList = PlayerSkills.Instance.GetPassiveSkillStatDescriptionList(linkedSkill);
        } else {
            statList = PlayerSkills.Instance.GetActiveSkillStatList(linkedSkill);
            statDescriptionList = PlayerSkills.Instance.GetActiveSkillStatDescriptionList(linkedSkill);
        }

        skillDescriptionCard.SetDescriptionCardText(skillName, statDescriptionList, statList);
    }

    #endregion

    protected override void OnDestroy() {
        base.OnDestroy();

        OnAnyButtonHovered -= LevelUI_SkillUI_OnAnyButtonHovered;
        OnAnyButtonSelected -= LevelUI_SkillUI_OnAnyButtonSelected;
    }
}
