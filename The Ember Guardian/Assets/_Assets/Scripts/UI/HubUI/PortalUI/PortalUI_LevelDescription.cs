using Mono.CSharp;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PortalUI_LevelDescription : MonoBehaviour {

    [SerializeField] private Portal portal;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private TextMeshProUGUI levelDescriptionText;
    [SerializeField] private Image levelSprite;
    [SerializeField] private Image completedIndicatorImage;
    [SerializeField] private Sprite completedIndicatorSprite;
    [SerializeField] private Sprite newIndicatorSprite;

    [SerializeField] private Transform animalTickContainer;
    [SerializeField] private Transform animalTickTemplate;
    [SerializeField] private Transform scavengableTickContainer;
    [SerializeField] private Transform scavengableTickTemplate;
    [SerializeField] private Transform workerCampsTickContainer;
    [SerializeField] private Transform workerCampsTickTemplate;
    [SerializeField] private Transform ChestTickContainer;
    [SerializeField] private Transform ChestTickTemplate;

    [SerializeField] private Transform creatureContainer;
    [SerializeField] private Transform creatureTemplate;

    [SerializeField] private LevelSO debugLevelSO;

    private LevelSO currentLevelSO;

    private void Start() {
        portal.OnLinkedLevelSOSet += Portal_OnLinkedLevelSOSet;
        SetLevelDescription(portal.GetLinkedLevelSO());
    }

    private void Portal_OnLinkedLevelSOSet(object sender, System.EventArgs e) {
        SetLevelDescription(portal.GetLinkedLevelSO());
    }

    public void SetLevelDescription(LevelSO levelSO) {
        currentLevelSO = levelSO;

        TMP_FontAsset font = LocalizationManager.Instance.GetCurrentFont();
        levelNameText.font = font;
        levelDescriptionText.font = font;

        levelNameText.text = LocalizationManager.Instance.GetLocalizedText(levelSO.levelNameLocalizationKey);
        levelDescriptionText.text = LocalizationManager.Instance.GetLocalizedText(levelSO.levelDescriptionLocalizationKey);
        levelSprite.sprite = levelSO.levelImage;

        StartCoroutine(RefreshCreaturePanel());
        RefreshCompletedIndicator();
        RefreshStat(animalTickContainer, animalTickTemplate, levelSO.faunaAmount);
        RefreshStat(scavengableTickContainer, scavengableTickTemplate, levelSO.scrapAmount);
        RefreshStat(workerCampsTickContainer, workerCampsTickTemplate, levelSO.wildEmberlingsAmount);
        RefreshStat(ChestTickContainer, ChestTickTemplate, levelSO.chestAmount);
    }

    private void RefreshCompletedIndicator() {
        if(MetaProgressionManager.Instance.GetLevelCompleted(currentLevelSO)) {
            completedIndicatorImage.sprite = completedIndicatorSprite;
        } else {
            completedIndicatorImage.sprite = newIndicatorSprite;
        }
    }

    private IEnumerator RefreshCreaturePanel() {
        creatureTemplate.gameObject.SetActive(true);

        foreach(Transform child in creatureContainer) {
            if (child == creatureTemplate) continue;
            Destroy(child.gameObject);
        }

        List<CreatureSO> creatureSOInLevel = new List<CreatureSO>();

        foreach (CreatureSO creatureSO in currentLevelSO.dayCreatureTypes) {
            creatureSOInLevel.Add(creatureSO);
        }

        foreach (CreatureSO creatureSO in currentLevelSO.nightCreatureTypes) {
            if (creatureSOInLevel.Contains(creatureSO)) continue;
            creatureSOInLevel.Add(creatureSO);
        }

        if(currentLevelSO.hasBoss) {
            creatureSOInLevel.Add(currentLevelSO.bossCreatureType);
        }


        foreach (CreatureSO creatureSO in creatureSOInLevel) {
            PortalUI_CreatureTemplate creatureTemplateUI = Instantiate(creatureTemplate, creatureContainer).GetComponent<PortalUI_CreatureTemplate>();
            creatureTemplateUI.SetCreatureSO(creatureSO);
        }

        creatureTemplate.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();

        creatureContainer.GetComponent<DynamicGridLayoutController>().RefreshLayout();
    }

    private void RefreshStat(Transform container, Transform template, int amount) {
        template.gameObject.SetActive(true);

        foreach (Transform child in container) {
            if (child == template) continue;
            Destroy(child.gameObject);
        }

        for(int i = 1; i <= 10; i++) {
            PortalUI_StatTemplate statTemplate = Instantiate(template, container).GetComponent<PortalUI_StatTemplate>();
            statTemplate.SetFilled(i <= amount);
        }

        template.gameObject.SetActive(false);
    }
}
