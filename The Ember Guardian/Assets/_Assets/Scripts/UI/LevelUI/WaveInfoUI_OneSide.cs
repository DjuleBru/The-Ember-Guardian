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

    private int maxDifficulty = 5;
    public void ShowFullWaveInfoUI() {
        fullDescriptionAnimation.ResetTrigger("Hide");
        fullDescriptionAnimation.SetTrigger("Show");
    }
    public void HideFullWaveInfoUI() {
        fullDescriptionAnimation.ResetTrigger("Show");
        fullDescriptionAnimation.SetTrigger("Hide");
    }

    public void RefreshCreatures() {
        foreach (Transform child in creatureTransformParent) {
            if (child == creatureTransformTemplate) continue;
            Destroy(child.gameObject);
        }

    }

    public void RefreshDifficulty() {
        foreach(Transform child in  difficultyTransformParent) {
            if (child == difficultyTransformTemplate) continue;
            Destroy(child.gameObject);
        }

        int difficulty = 2;
        for (int i = 0; i < maxDifficulty; i++) {
            Transform template = Instantiate(difficultyTransformTemplate, difficultyTransformParent);

            if(i > difficulty) {
                template.Find("Fill").gameObject.SetActive(false);
            }
        }
    }
}
