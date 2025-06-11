using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveInfoUI_CreatureTemplate : MonoBehaviour
{
    [SerializeField] private Image creatureIcon;
    [SerializeField] private Sprite hideCreatureSprite;
    [SerializeField] private TextMeshProUGUI creatureAmountText;
    [SerializeField] private GameObject creatureAmountGameObject;

    public void SetCreature(CreatureSO creatureSO, int creatureAmount, bool showCreatureSprite, bool showCreatureAmount) {
        if(showCreatureSprite) {
            creatureIcon.sprite = creatureSO.creatureIcon_Portrait;
        } else {
            creatureIcon.sprite = hideCreatureSprite;
        }
        
        if(showCreatureAmount) {
            creatureAmountText.text = creatureAmount.ToString();
        } else {
            creatureAmountGameObject.SetActive(false);
        }
    }
}
