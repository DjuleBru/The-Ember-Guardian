using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenSkillPanelButton : ButtonUI
{
    [SerializeField] private bool isActiveSkillButton;

    public static event EventHandler OnAnyDisplaySkillPanelButtonPressed;

    private void Awake() {
        GetComponent<Button>().onClick.AddListener(() => ButtonPress());
    }

    protected override void Start() {
        base.Start();
        if(!MetaProgressionManager.Instance.GetDropRedOrbsUnlocked()) {
            gameObject.SetActive(false);
        }; 
        
        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.HUB) {
            gameObject.SetActive(false);
        };
    }

    private void ButtonPress() {
        OnAnyDisplaySkillPanelButtonPressed?.Invoke(this, EventArgs.Empty);

        SkillsDescriptionPanel.Instance.UpdateSkillSlots(isActiveSkillButton);
        SkillsDescriptionPanel.Instance.OpenClosePanel(isActiveSkillButton);
        SkillsDescriptionPanel.Instance.SetDisplayingActiveSkills(isActiveSkillButton);
    }
}
