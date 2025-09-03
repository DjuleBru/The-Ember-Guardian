using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerTalkUI : MonoBehaviour
{
    public static PlayerTalkUI Instance;
    [SerializeField] private TextMeshProUGUI talkText;

    private bool showingText;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        talkText.text = "";

        if(DayNightManager.Instance != null) {
            if (DemoMainLevelManager.Instance != null) return;
            DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        }
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        StartCoroutine(CheckBossSpawnsThisNight());
    }

    private IEnumerator CheckBossSpawnsThisNight() {
        yield return new WaitForSeconds(1.5f);

        bool bossSpawnsThisNight = CreaturesSpawnManager.Instance.GetBossSpawnsThisNight();
        if(bossSpawnsThisNight) {
            int randomTextInt = UnityEngine.Random.Range(0, 4);

            string localizationKeyText = "player_bossSpawnsThisNight" + randomTextInt;
            ShowTalkText(localizationKeyText, 5f);
        }
    }

    public void ShowTalkText(string textToShow, float showDuration) {
        if (showingText) return;

        talkText.text = textToShow;
        showingText = true;

        StartCoroutine(HideTextAfterDelay(showDuration));
    }

    public void HideTalkText() {
        talkText.text = "";

        showingText = false;
    }

    private IEnumerator HideTextAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        HideTalkText();
    }

}
