using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelUI_DayCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayText;
    private Animator dayTextAnimator;

    public static LevelUI_DayCountUI Instance;

    public event EventHandler OnDayUIShown;

    private void Awake() {
        Instance = this;
    }

    void Start()
    {
        dayTextAnimator = GetComponent<Animator>();
        DayNightManager.Instance.OnDawnStart += DayNightManager_OnDawnStart;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        StartCoroutine(ShowDayCountUIAfterDelay(7f));
    }

    private void DayNightManager_OnDawnStart(object sender, System.EventArgs e) {
        dayText.text = LocalizationManager.Instance.GetLocalizedText("day") + " " + (DayNightManager.Instance.GetCurrentDay()+1);

        if (!Fire.Instance.GetInitialFireLit()) return;
        StartCoroutine(ShowDayCountUIAfterDelay(4.5f));
    }

    private IEnumerator ShowDayCountUIAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        dayTextAnimator.SetTrigger("Show");
        OnDayUIShown?.Invoke(this, EventArgs.Empty);
    }
}
