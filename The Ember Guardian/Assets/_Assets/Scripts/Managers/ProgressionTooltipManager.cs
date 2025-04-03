using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionTooltipManager : MonoBehaviour
{
    public static ProgressionTooltipManager Instance;

    private bool multipleFunctionsTooltipShown;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        LoadTooltipsShown();
        if(SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level) {
            Structure.OnAnyPlayerTriggeredIn += Structure_OnAnyPlayerTriggeredIn;
        }
    }

    private void LoadTooltipsShown() {
        multipleFunctionsTooltipShown = ES3.Load("multipleFunctionsTooltipShown", false);
    }

    private void Structure_OnAnyPlayerTriggeredIn(object sender, System.EventArgs e) {
        // MultipleFunctions tooltip
        Structure structure = (Structure)sender;

        if(structure.GetActiveStructureInteractionTypeList().Count > 1) {

            if (multipleFunctionsTooltipShown) return;
            PlayerTooltipManager.Instance.GetTooltipLeft().ShowTooltipInstruction(LocalizationManager.Instance.GetLocalizedText("menu_press"), LocalizationManager.Instance.GetLocalizedText("tooltip_switchStructureFunction"), InputControlIcons.Control.SwitchBuildingFunctions, 10f);
            multipleFunctionsTooltipShown = true;
            ES3.Save("multipleFunctionsTooltipShown", true);
        }
    }


}
