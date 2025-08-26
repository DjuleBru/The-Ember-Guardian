using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualOnlyMainMenu : MonoBehaviour
{

    [SerializeField] private Sprite femaleSprite;
    [SerializeField] private Sprite maleSprite;
    [SerializeField] private SpriteRenderer characterSpriteRenderer;

    private void Start() {
        CharacterSelectUI.Instance.OnCharacterChanged += CharacterSelectUI_OnCharacterChanged;

        SetCharacterVisuals();
    }

    private void CharacterSelectUI_OnCharacterChanged(object sender, System.EventArgs e) {
        SetCharacterVisuals();
    }

    private void SetCharacterVisuals() {
        bool chosenCharacterIsFemale = CharacterSelectUI.Instance.GetChosenCharacterIsFemale();

        if (chosenCharacterIsFemale) {
            characterSpriteRenderer.sprite = femaleSprite;
        } else {
            characterSpriteRenderer.sprite = maleSprite;
        }
    }
}
