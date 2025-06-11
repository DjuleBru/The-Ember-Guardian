using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveInfoUI_OneSide : MonoBehaviour
{
    [SerializeField] private Animator fullDescriptionAnimation;
    [SerializeField] private Transform difficultyTransformParent;
    [SerializeField] private Transform difficultyTransformTemplate;
    [SerializeField] private Transform creatureTransformParent;
    [SerializeField] private Transform creatureTransformTemplate;

    private int maxDifficulty = 10;


    public void RefreshCreatures(Dictionary<CreatureSO, int> thisSideCreatures,bool showCreatureSprite, bool showCreatureAmount) {
        creatureTransformTemplate.gameObject.SetActive(true);

        foreach (Transform child in creatureTransformParent) {
            if (child == creatureTransformTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (var entry in thisSideCreatures) {
            CreatureSO creatureSO = entry.Key;
            int creatureAmount = entry.Value;

            Transform newCreatureUI = Instantiate(creatureTransformTemplate, creatureTransformParent);
            newCreatureUI.gameObject.SetActive(true);

            WaveInfoUI_CreatureTemplate creatureTemplate = newCreatureUI.GetComponent<WaveInfoUI_CreatureTemplate>();
            creatureTemplate.SetCreature(creatureSO,creatureAmount, showCreatureSprite, showCreatureAmount);
        }

        creatureTransformTemplate.gameObject.SetActive(false);
    }


    public void RefreshDifficulty(int difficulty) {
        difficultyTransformTemplate.gameObject.SetActive(true);

        foreach (Transform child in difficultyTransformParent) {
            if (child == difficultyTransformTemplate) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < maxDifficulty; i++) {
            Transform template = Instantiate(difficultyTransformTemplate, difficultyTransformParent);

            if (i >= difficulty) {
                template.Find("Fill").gameObject.SetActive(false);
            }
        }

        difficultyTransformTemplate.gameObject.SetActive(false);
    }

    public void ShowFullWaveInfoUI() {
        fullDescriptionAnimation.ResetTrigger("Hide");
        fullDescriptionAnimation.SetTrigger("Show");
    }

    public void HideFullWaveInfoUI() {
        fullDescriptionAnimation.ResetTrigger("Show");
        fullDescriptionAnimation.SetTrigger("Hide");
    }


}
