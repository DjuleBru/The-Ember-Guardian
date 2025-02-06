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
    [SerializeField] private VideoTipSO healTentTip;
    [SerializeField] private VideoTipSO setupEconomyTip;
    [SerializeField] private VideoTipSO setupDefensesTip;
    [SerializeField] private VideoTipSO emberExtractionTip;
    [SerializeField] private VideoTipSO dayNightCycleTip;

    private bool reloadingTipShown;
    private bool fireManagementTipShown;
    private bool dieTipShown;
    private bool critHitsTipShown;
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
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
        }
    }

    private void LoadTooltipsShown() {
        reloadingTipShown = ES3.Load("reloadingTipShown", false);
        fireManagementTipShown = ES3.Load("fireManagementTipShown", false);
        dieTipShown = ES3.Load("dieTipShown", false);
        critHitsTipShown = ES3.Load("critHitsTipShown", false);
        healTentTipShown = ES3.Load("healTentTipShown", false);
        setupEconomyTipShown = ES3.Load("setupEconomyTipShown", false);
        setupDefensesTipShown = ES3.Load("setupDefensesTipShown", false);
        emberExtractionTipShown = ES3.Load("emberExtractionTipShown", false);
        dayNightCycleTipShown = ES3.Load("dayNightCycleTipShown", false);
    }

}
