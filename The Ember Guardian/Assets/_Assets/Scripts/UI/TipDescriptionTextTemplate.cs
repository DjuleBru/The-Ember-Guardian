using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TipDescriptionTextTemplate : MonoBehaviour
{
    [SerializeField] private Animator templateAnimator;
    [SerializeField] private TextMeshProUGUI templateText;

    private void Awake() {
        templateAnimator.enabled = false;
    }

    public void SetTipDescription(string text) {
        templateText.text = text;
    }

    public void ShowTipText() {
        gameObject.SetActive(true);
        templateAnimator.enabled = true;
    }
}
