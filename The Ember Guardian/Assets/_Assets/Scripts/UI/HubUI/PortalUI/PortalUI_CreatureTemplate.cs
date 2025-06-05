using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PortalUI_CreatureTemplate : MonoBehaviour
{
    [SerializeField] private Image creatureIconFill;
    [SerializeField] private Image creatureIconBackground;

    public void SetCreatureSO(CreatureSO creatureSO) {
        creatureIconFill.sprite = creatureSO.creatureIcon;
        creatureIconBackground.sprite = creatureSO.creatureIcon;

        if(!MetaProgressionManager.Instance.GetCreatureUnlocked(creatureSO)) {
            creatureIconFill.gameObject.SetActive(false);
        }
    }
}
