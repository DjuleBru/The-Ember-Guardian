using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualOnlyMainMenu : MonoBehaviour
{

    [SerializeField] private RuntimeAnimatorController femaleAnimatorController;
    [SerializeField] Animator characterVisualAnimator;

    private void Start() {
        CharacterSelectUI.Instance.OnCharacterChanged += CharacterSelectUI_OnCharacterChanged;

        SetCharacterVisuals();
    }

    private void CharacterSelectUI_OnCharacterChanged(object sender, System.EventArgs e) {
        SetCharacterVisuals();
    }

    private void SetCharacterVisuals() {
        bool chosenCharacterIsFemale = CharacterSelectUI.Instance.GetChosenCharacterIsFemale();

        Debug.Log("SetCharacterVisuals chosenCharacterIsFemale " + chosenCharacterIsFemale);
        if (chosenCharacterIsFemale) {
            characterVisualAnimator.runtimeAnimatorController = femaleAnimatorController;
        }
    }
}
