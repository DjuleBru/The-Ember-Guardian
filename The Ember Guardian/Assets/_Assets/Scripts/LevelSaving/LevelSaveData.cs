using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSaveData {

    public int currentDay;

    public LevelUI_ObjectiveUI.ObjectiveType currentObjectiveType;
    public List<LevelUI_ObjectiveUI.SubObjectiveType> currentSubObjectiveTypeList;

    public bool emberExtracted;
    public bool initialFireLit;
    public bool darklingNestFound;
    public bool darklingNestCleared;
    public bool returnToHubObjectiveShown;
    public bool levelSucceeded;

    public bool hubMerchantHasTalkLinesToShow;


    public int NPCInteractionsIndex;
    public int levelManager_levelHubMerchantInteractionIndex;
    public int nightsSurvived;
    public int obstaclesRemoved;
    public int watcherArtifactFillUpAmount;
}