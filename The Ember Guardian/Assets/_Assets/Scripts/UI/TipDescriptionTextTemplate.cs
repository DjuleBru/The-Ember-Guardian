using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TipDescriptionTextTemplate : MonoBehaviour
{
    [SerializeField] private Animator templateAnimator;
    [SerializeField] private TextMeshProUGUI templateText;
    [SerializeField] private Image templateIcon;

    private void Awake() {
        templateAnimator.enabled = false;
    }

    public void SetTipDescriptionAdvanced(string text, bool isMainTitle = false) {
        RectTransform rt = templateIcon.GetComponent<RectTransform>();
        if (isMainTitle) {
            templateText.fontSize = 35f;
            rt.sizeDelta = new Vector2(45, 45);
        } else {
            templateText.fontSize = 30f;
            rt.sizeDelta = new Vector2(35, 35);

        }

        string[] parts = text.Split(new string[] { "[icon:" }, System.StringSplitOptions.None);
        Dictionary<string, Sprite> iconDictionary = GameIcons.Instance.GetIconDictionary();

        // Nettoyage des anciennes icônes/textes sauf les templates
        foreach (Transform child in transform) {
            if (child == templateIcon.transform) continue;
            if (child == templateText.transform) continue;
            Destroy(child.gameObject);
        }

        foreach (string part in parts) {
            Debug.Log("part " + part);

            if (part.Contains("]")) {
                string[] split = part.Split(']');
                string iconKey = split[0]; // Clé de l'icône (ex: "Reload", "Shoot", "Fireball")
                string remainingText = split.Length > 1 ? split[1] : "";

                bool iconAdded = false;

                // 1. Vérifier si c'est une icône d'input (clavier/manette)
                if (Enum.TryParse(iconKey, out InputControlIcons.Control control)) {
                    List<Sprite> inputIcons = InputControlIcons.Instance.GetControlIconSprite(control);

                    if (inputIcons.Count > 0) {
                        foreach (Sprite iconSprite in inputIcons) {
                            Image newIcon = Instantiate(templateIcon, transform);
                            newIcon.sprite = iconSprite;
                            newIcon.gameObject.SetActive(true);
                        }
                        iconAdded = true;
                    }
                }

                // 2. Vérifier si c'est une icône standard (ex: icône d'arme, potion, etc.)
                if (!iconAdded && iconDictionary.TryGetValue(iconKey, out Sprite standardIcon)) {
                    Image newIcon = Instantiate(templateIcon, transform);
                    newIcon.sprite = standardIcon;
                    newIcon.gameObject.SetActive(true);
                    iconAdded = true;
                }

                // 3. Ajouter le texte restant après l'icône
                if (!string.IsNullOrEmpty(remainingText)) {
                    TextMeshProUGUI newText = Instantiate(templateText, transform);
                    newText.text = remainingText;
                    newText.gameObject.SetActive(true);
                }
            }
            else {
                // 4. Ajouter du texte normal s'il n'y a pas d'icône
                TextMeshProUGUI newText = Instantiate(templateText, transform);
                newText.text = part;
                newText.gameObject.SetActive(true);
            }
        }

        // Désactiver les templates pour éviter qu'ils apparaissent
        templateText.gameObject.SetActive(false);
        templateIcon.gameObject.SetActive(false);
    }


    public void ShowTipText() {
        gameObject.SetActive(true);
        templateAnimator.enabled = true;
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    public void HideTipText() {
        gameObject.SetActive(false);
        templateAnimator.enabled = false;
    }
}
