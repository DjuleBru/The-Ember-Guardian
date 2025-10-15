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
    private Button button;
    [SerializeField] private bool isLeftActiveSkill;
    [SerializeField] private bool isRightActiveSkill;

    [SerializeField] private Animator skillTemplateAnimator;
    [SerializeField] private Button skillDeleteButton;
    [SerializeField] private ButtonUI skillDeleteButtonUI;
    [SerializeField] private Animator skillDeleteAnimator;
    [SerializeField] private Image skillTemplateImage;
    [SerializeField] private Image skillTemplateBackgroundImage;
    [SerializeField] private Image skillTemplateOutlineImage;
    [SerializeField] private RectTransform skillLevelRectTransform;
    [SerializeField] private TextMeshProUGUI skillLevelText;
    [SerializeField] private LevelUI_SkillDescriptionCardUI skillDescriptionCard;

    private bool tabMenuOpen;
    private bool skillReady;

    protected override void Start() {
        base.Start();
        button = GetComponent<Button>();

        if(GameInput.Instance.IsUsingGamepad()) {
            button.enabled = false;
            EnableSkillDeleteButton(true);
        }


        PlayerSkills.Instance.OnLeftActiveSkillActivated += PlayerSkills_OnLeftActiveSkillActivated;
        PlayerSkills.Instance.OnRightActiveSkillActivated += PlayerSkills_OnRightActiveSkillActivated;
        PlayerSkills.Instance.OnLeftActiveSkillDeactivated += PlayerSkills_OnLeftActiveSkillDeactivated;
        PlayerSkills.Instance.OnRightActiveSkillDeactivated += PlayerSkills_OnRightActiveSkillDeactivated;
        PlayerTabMenuUI.Instance.OnPlayerTabClosed += PlayerTabMenu_OnPlayerTabClosed;
        PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenu_OnPlayerTabOpened;

        OnAnyButtonHovered += LevelUI_SkillUI_OnAnyButtonHovered;
        OnAnyButtonSelected += LevelUI_SkillUI_OnAnyButtonSelected;
    }


    public void SetLinkedSkill(SkillItem skillItem) {
        linkedSkill = skillItem;
        RefreshSkillVisuals();
    }

    private void RefreshSkillVisuals() {
        if(linkedSkill == null) return;

        skillTemplateImage.sprite = linkedSkill.skillSO.Icon;
        skillTemplateBackgroundImage.sprite = linkedSkill.skillSO.Icon;
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
        if (!isRightActiveSkill) return;
        if (skillTemplateAnimator == null) return;
        skillTemplateAnimator.SetBool("Active", false);
    }

    private void PlayerSkills_OnLeftActiveSkillDeactivated(object sender, System.EventArgs e) {
        if (!isLeftActiveSkill) return;
        if (skillTemplateAnimator == null) return;
        skillTemplateAnimator.SetBool("Active", false);
    }

    private void PlayerSkills_OnRightActiveSkillActivated(object sender, System.EventArgs e) {
        if (!isRightActiveSkill) return;
        if (skillTemplateAnimator == null) return;
        skillTemplateAnimator.SetBool("Active", true);
    }

    private void PlayerSkills_OnLeftActiveSkillActivated(object sender, System.EventArgs e) {
        if (!isLeftActiveSkill) return;
        if (skillTemplateAnimator == null) return; 
        skillTemplateAnimator.SetBool("Active", true);
    }

    private void PlayerTabMenu_OnPlayerTabOpened(object sender, EventArgs e) {
        if (GameInput.Instance.IsUsingGamepad()) {
            button.enabled = true;
            tabMenuOpen = true;
            EnableSkillDeleteButton(true);
        }

    }

    private void PlayerTabMenu_OnPlayerTabClosed(object sender, EventArgs e) {
        if (GameInput.Instance.IsUsingGamepad()) {
            button.enabled = false;
            tabMenuOpen = false;
            EnableSkillDeleteButton(false);
        }


        if (skillDeleteAnimator != null) {
            skillDeleteAnimator.ResetTrigger("Show");
            skillDeleteAnimator.SetTrigger("Hide");
        }
    }

    public void RemoveSkill() {
        OpenCloseSkillDescriptionCard(false);
        skillTemplateAnimator.SetBool("Active", true);
        skillTemplateAnimator.SetBool("Ready", true);
        PlayerSkills.Instance.RemoveActiveSkill(linkedSkill);
        PlayerTabMenuUI.Instance.SelectFirstButtonSelected();
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

            if(!(sender as ButtonUI is DeleteSkillUI)) {
                if (skillDeleteAnimator != null) {
                    skillDeleteAnimator.ResetTrigger("Show");
                    skillDeleteAnimator.SetTrigger("Hide");
                }
            }

            return;
        }

        if (sender as ButtonUI != this) {
            if (!(sender as ButtonUI is DeleteSkillUI)) {
                if (skillDeleteAnimator != null) {
                    skillDeleteAnimator.ResetTrigger("Show");
                    skillDeleteAnimator.SetTrigger("Hide");
                }
            }
            return;
        }
        OpenCloseSkillDescriptionCard(true);
        SetDescriptionCardPosition();

        if (skillDeleteAnimator != null) {
            skillDeleteAnimator.ResetTrigger("Hide");
            skillDeleteAnimator.SetTrigger("Show");
        }

    }

    private void LevelUI_SkillUI_OnAnyButtonHovered(object sender, System.EventArgs e) {
        if (sender as ButtonUI != this) return;

        OpenCloseSkillDescriptionCard(true);
        SetDescriptionCardPosition();

        if (skillDeleteAnimator != null) {
            skillDeleteAnimator.ResetTrigger("Hide");
            skillDeleteAnimator.SetTrigger("Show");
        }

    }

    public override void OnPointerEnter(PointerEventData eventData) {
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData) {
        base.OnPointerExit(eventData);
        OpenCloseSkillDescriptionCard(false); 
        
        if (skillDeleteAnimator != null) {
            skillDeleteAnimator.ResetTrigger("Show");
            skillDeleteAnimator.SetTrigger("Hide");
        }
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
        float heigt = 320;
        if (!isLeftActiveSkill && !isRightActiveSkill) {
            heigt = 250f;
        }

        Vector3 newDescPos = new Vector3(callerWorldPos.x + distanceToSkill, descRT.position.y, 0);

        // Positionner la description card à ce point
        descRT.position = newDescPos;
        descRT.sizeDelta = new Vector2(descRT.sizeDelta.x, heigt);
    }

    private void SetDesciptionCardText() {

        string skillName = linkedSkill.skillSO.SkillName;
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

    private void EnableSkillDeleteButton(bool enabled) {
        if (skillDeleteButton == null) return;
        skillDeleteButton.enabled = enabled;
        skillDeleteButtonUI.enabled = enabled;
    }

    #endregion

    protected override void OnDestroy() {
        base.OnDestroy();

        PlayerTabMenuUI.Instance.OnPlayerTabClosed -= PlayerTabMenu_OnPlayerTabClosed;
        PlayerTabMenuUI.Instance.OnPlayerTabOpened -= PlayerTabMenu_OnPlayerTabOpened;
        OnAnyButtonHovered -= LevelUI_SkillUI_OnAnyButtonHovered;
        OnAnyButtonSelected -= LevelUI_SkillUI_OnAnyButtonSelected;

        PlayerSkills.Instance.OnLeftActiveSkillActivated -= PlayerSkills_OnLeftActiveSkillActivated;
        PlayerSkills.Instance.OnRightActiveSkillActivated -= PlayerSkills_OnRightActiveSkillActivated;
        PlayerSkills.Instance.OnLeftActiveSkillDeactivated -= PlayerSkills_OnLeftActiveSkillDeactivated;
        PlayerSkills.Instance.OnRightActiveSkillDeactivated -= PlayerSkills_OnRightActiveSkillDeactivated;

    }
}
