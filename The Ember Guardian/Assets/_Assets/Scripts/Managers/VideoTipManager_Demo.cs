using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoTipManager_Demo : MonoBehaviour
{

    [SerializeField] private VideoTipSO fireManagementTip;
    [SerializeField] private VideoTipSO healTentTip;
    [SerializeField] private VideoTipSO hunterTip;
    [SerializeField] private VideoTipSO hunterFlagTip;
    [SerializeField] private VideoTipSO trapTip;

    private bool workerRecruited;
    private bool healTentTipShown;
    private bool hunterTipShown;
    private bool fireManagementTipShown;
    private bool hunterFlagTipShown;
    private bool trapTipShown;

    private int hunterNumberRecruited;

    private void Start() {
        LoadTooltipsShown();

        Fire.Instance.OnFireFuelled += Fire_OnFireFuelled;

        HuntingFlag_PlayerDefined.OnAnyPlayerTriggeredIn += HuntingFlag_PlayerDefined_OnAnyPlayerTriggeredIn;
        Structure.OnAnyPlayerTriggeredIn += Structure_OnAnyPlayerTriggeredIn_Level;
        Worker.OnAnyWorkerRecruited += Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter += Worker_OnAnyWorkerAssignedHunter;
        UICurrencyManager.PlayerInventoryUI.OnCurrencyCollected += PlayerInventoryUI_OnCurrencyCollected;
    }

    private void PlayerInventoryUI_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        if (trapTipShown) return;

        if (e.currencyUIDropped.GetCurrencyCategory() == PlayerCurrencies.CurrencyCategory.trap) {
            trapTipShown = true;
            VideoTipUI.Instance.PlayTipSO(trapTip, .2f);
            ES3.Save("trapTipShown", true);
        }
    }

    private void HuntingFlag_PlayerDefined_OnAnyPlayerTriggeredIn(object sender, EventArgs e) {
        if (!hunterFlagTipShown && DemoMainLevelManager.Instance != null && DemoMainLevelManager.Instance.GetDemoFirstLevelCompleted()) {
            VideoTipUI.Instance.PlayTipSO(hunterFlagTip, .2f);
            ES3.Save("hunterFlagTipShown", true);
            hunterFlagTipShown = true;
        }
    }

    private void Worker_OnAnyWorkerAssignedHunter(object sender, EventArgs e) {
        if (hunterTipShown) return;
        hunterNumberRecruited++;

        if(hunterNumberRecruited == 2) {
            hunterTipShown = true;

            VideoTipUI.Instance.PlayTipSO(hunterTip, .5f);
            ES3.Save("hunterTipShown", true);
            
        }
    }

    private void Worker_OnAnyWorkerRecruited(object sender, EventArgs e) {
        workerRecruited = true;
    }


    private void Structure_OnAnyPlayerTriggeredIn_Level(object sender, EventArgs e) {
        Structure structure = (Structure)sender;

        if (structure.GetStructureSO().structureType == StructureSO.StructureType.tent) {
            if (healTentTipShown) return;
            if (Player.Instance.GetHP() == PlayerStats.Instance.GetMaxHP()) return;
            if (!DemoMainLevelManager.Instance.GetDemoMainLevelTutorialCompleted()) return;

            VideoTipUI.Instance.PlayTipSO(healTentTip, .3f);
            healTentTipShown = true;

            ES3.Save("healTentTipShown", true);
        }
    }

    private void Fire_OnFireFuelled(object sender, EventArgs e) {
        if (DemoMainLevelManager.Instance.GetDemoMainLevelTutorialCompleted()) return;
        if (fireManagementTipShown) return;

        fireManagementTipShown = true;
        VideoTipUI.Instance.PlayTipSO(fireManagementTip, .5f);
        ES3.Save("fireManagementTipShown", true);
    }

    private void LoadTooltipsShown() {
        healTentTipShown = ES3.Load("healTentTipShown", false);
        hunterTipShown = ES3.Load("hunterTipShown", false);
        hunterFlagTipShown = ES3.Load("hunterFlagTipShown", false);
        trapTipShown = ES3.Load("trapTipShown", false);
    }

    private void OnDestroy() {
        HuntingFlag_PlayerDefined.OnAnyPlayerTriggeredIn -= HuntingFlag_PlayerDefined_OnAnyPlayerTriggeredIn;
        Structure.OnAnyPlayerTriggeredIn -= Structure_OnAnyPlayerTriggeredIn_Level;
        Worker.OnAnyWorkerRecruited -= Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter -= Worker_OnAnyWorkerAssignedHunter;
    }
}
