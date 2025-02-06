using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoTipManager : MonoBehaviour
{
    public static VideoTipManager Instance;

    [SerializeField] private VideoTipSO reloadingTip;
    [SerializeField] private VideoTipSO fireManagementTip;
    [SerializeField] private VideoTipSO dieTip;
    [SerializeField] private VideoTipSO critHitsTip;
    [SerializeField] private VideoTipSO rollTip;
    [SerializeField] private VideoTipSO healTentTip;
    [SerializeField] private VideoTipSO setupEconomyTip;
    [SerializeField] private VideoTipSO setupDefensesTip;
    [SerializeField] private VideoTipSO emberExtractionTip;
    [SerializeField] private VideoTipSO dayNightCycleTip;
    [SerializeField] private VideoTipSO hunterTip;
    [SerializeField] private VideoTipSO recruitEmberlingTip;

    private bool reloadingTipShown;
    private bool critHitsTipShown;
    private bool rollTipShown;
    private bool fireManagementTipShown;
    private bool hunterTipShown;
    private bool recruitEmberlingTipShown;

    private bool dieTipShown;
    private bool healTentTipShown;
    private bool setupEconomyTipShown;
    private bool setupDefensesTipShown;
    private bool emberExtractionTipShown;
    private bool dayNightCycleTipShown;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        LoadTooltipsShown();

        UICurrencyManager.Instance.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        Mob.OnAnyMobDied += Creature_OnAnyMobDied;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
        }

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {
            TutorialCollider.OnRollTipCollided += TutorialCollider_OnRollTipCollided;
            Fire.Instance.OnFireFuelled += Fire_OnFireFuelled;
        } else {
            reloadingTipShown = true;
            critHitsTipShown = true;
            rollTipShown = true;
            fireManagementTipShown = true;
            hunterTipShown = true;
            recruitEmberlingTipShown = true;
        }
    }

    private void Fire_OnFireFuelled(object sender, EventArgs e) {
        if (fireManagementTipShown) return;
        //VideoTipUI.Instance.PlayTipSO(fireManagementTip, 0.5f);
        fireManagementTipShown = true;
    }

    private void TutorialCollider_OnRollTipCollided(object sender, EventArgs e) {
        if (rollTipShown) return;
        VideoTipUI.Instance.PlayTipSO(rollTip, 0f);
        rollTipShown = true;
    }

    private void Creature_OnAnyMobDied(object sender, EventArgs e) {
        if (critHitsTipShown) return;
        VideoTipUI.Instance.PlayTipSO(critHitsTip, 3f);
        critHitsTipShown = true;
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        PlayerCurrencies.CurrencyType currencyType = e.currencyUIDropped.GetCurrencyType();

        if (currencyType == PlayerCurrencies.CurrencyType.ammo && !reloadingTipShown) {
            VideoTipUI.Instance.PlayTipSO(reloadingTip, 1f);
            reloadingTipShown = true;
        }
    }

    private void LoadTooltipsShown() {
        dieTipShown = ES3.Load("dieTipShown", false);
        healTentTipShown = ES3.Load("healTentTipShown", false);
        setupEconomyTipShown = ES3.Load("setupEconomyTipShown", false);
        setupDefensesTipShown = ES3.Load("setupDefensesTipShown", false);
        emberExtractionTipShown = ES3.Load("emberExtractionTipShown", false);
        dayNightCycleTipShown = ES3.Load("dayNightCycleTipShown", false);
    }


    private void OnDestroy() {
        Mob.OnAnyMobDied -= Creature_OnAnyMobDied;
        TutorialCollider.OnRollTipCollided -= TutorialCollider_OnRollTipCollided;
    }
}
