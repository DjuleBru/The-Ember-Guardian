using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeleteSkillUI : ButtonUI, IPointerEnterHandler, IPointerExitHandler {

    private Button deleteSkillButton;
    [SerializeField] private LevelUI_SkillUI skillUI;

    public static event EventHandler OnAnyActiveSkillDeleted;

    private void Awake() {
        deleteSkillButton = GetComponent<Button>();

        deleteSkillButton.onClick.AddListener(() => {
            skillUI.RemoveSkill();
            OnAnyActiveSkillDeleted?.Invoke(this, EventArgs.Empty);
        });
    }
}
