using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoTipManager_Demo : MonoBehaviour
{

    [SerializeField] private VideoTipSO fireManagementTip;
    [SerializeField] private VideoTipSO healTentTip;
    [SerializeField] private VideoTipSO hunterTip;

    private bool workerRecruited;
    private bool healTentTipShown;
    private bool hunterTipShown;
    private bool fireManagementTipShown;

    private int hunterNumberRecruited;

    private void Start() {
        LoadTooltipsShown();

        Fire.Instance.OnFireFuelled += Fire_OnFireFuelled;

        Structure.OnAnyPlayerTriggeredIn += Structure_OnAnyPlayerTriggeredIn_Level;
        Worker.OnAnyWorkerRecruited += Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter += Worker_OnAnyWorkerAssignedHunter;
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
            if (!DemoMainLevelManager.Instance.GetDemoMainLevelTutorialCompleted()) return;

            VideoTipUI.Instance.PlayTipSO(healTentTip, .3f);
            healTentTipShown = true;

            ES3.Save("healTentTipShown", true);
        }
    }

    private void Fire_OnFireFuelled(object sender, EventArgs e) {
        if (DemoMainLevelManager.Instance.GetDemoMainLevelTutorialCompleted()) return;

        fireManagementTipShown = true;
        VideoTipUI.Instance.PlayTipSO(fireManagementTip, 1.5f);
        ES3.Save("fireManagementTipShown", true);
    }

    private void LoadTooltipsShown() {
        healTentTipShown = ES3.Load("healTentTipShown", false);
        hunterTipShown = ES3.Load("hunterTipShown", false);
    }

    private void OnDestroy() {
        Structure.OnAnyPlayerTriggeredIn -= Structure_OnAnyPlayerTriggeredIn_Level;
        Worker.OnAnyWorkerRecruited -= Worker_OnAnyWorkerRecruited;
        Worker.OnAnyWorkerAssignedHunter -= Worker_OnAnyWorkerAssignedHunter;
    }
}
