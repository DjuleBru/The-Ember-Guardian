using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RefundGunButtonUI : MonoBehaviour
{
    private Animator animator;

    private ItemButtonUI linkedItemButtonUI;
    private ButtonUI linkedButtonUI;
    private bool buttonHovered;

    private void Awake() {
        animator = GetComponent<Animator>();
        linkedItemButtonUI = GetComponentInParent<ItemButtonUI>();
        linkedButtonUI = GetComponentInParent<ButtonUI>();

        ButtonUI.OnAnyButtonHovered += ButtonUI_OnAnyButtonHovered;
        ButtonUI.OnAnyButtonSelected += ButtonUI_OnAnyButtonSelected;
        linkedButtonUI.OnPointerExitedButtonUI += LinkedButtonUI_OnPointerExitedButtonUI;
        linkedItemButtonUI.OnHubMerchantItemRefunded += LinkedItemButtonUI_OnHubMerchantItemRefunded;
    }

    private void Start() {
        
    }

    private void ButtonUI_OnAnyButtonSelected(object sender, System.EventArgs e) {
        if (!GameInput.Instance.IsUsingGamepad()) return;
        if (!linkedItemButtonUI.GetItemRefundable()) return;

        ButtonUI button = sender as ButtonUI;

        if (button == linkedButtonUI) {
            if (buttonHovered) return;

            buttonHovered = true;
            animator.SetTrigger("Show");

        }
        else {

            if (!buttonHovered) return;
            buttonHovered = false;
            animator.SetTrigger("Hide");

        }
    }

    private void LinkedItemButtonUI_OnHubMerchantItemRefunded(object sender, System.EventArgs e) {
        if (!buttonHovered) return;
        buttonHovered = false;
        animator.SetTrigger("Hide");
    }

    private void LinkedButtonUI_OnPointerExitedButtonUI(object sender, System.EventArgs e) {
        if (!buttonHovered) return;

        buttonHovered = false;
        animator.SetTrigger("Hide");
    }

    private void ButtonUI_OnAnyButtonHovered(object sender, System.EventArgs e) {
        if (!linkedItemButtonUI.GetItemRefundable()) return;

        ButtonUI button = sender as ButtonUI;
        
        if (button == linkedButtonUI) {
            if (buttonHovered) return;

            buttonHovered = true;
            animator.SetTrigger("Show");

        } else {

            if (!buttonHovered) return;
            buttonHovered = false;
            animator.SetTrigger("Hide");

        }
    }

    private void OnDestroy() {
        ButtonUI.OnAnyButtonHovered -= ButtonUI_OnAnyButtonHovered;
        ButtonUI.OnAnyButtonSelected -= ButtonUI_OnAnyButtonSelected;
    }

}
