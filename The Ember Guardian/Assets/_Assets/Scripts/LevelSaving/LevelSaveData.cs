using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSaveData {

    public string sceneName;
    public int currentDay;

    public LevelUI_ObjectiveUI.ObjectiveType currentObjectiveType;
    public List<LevelUI_ObjectiveUI.SubObjectiveType> currentSubObjectiveTypeList;

    public bool emberExtracted;
    public bool initialFireLit;
    public bool darklingNestFound;
    public bool darklingNestCleared;
    public bool darklingNest_RightFound;
    public bool darklingNest_RightDestroyed;
    public bool darklingNest_LeftFound;
    public bool darklingNest_LeftDestroyed;
    public bool returnToHubObjectiveShown;
    public bool levelSucceeded;
    public bool hubMerchantHasTalkLinesToShow;
    public bool conditionalLockedStructureLocationUnlocked;
    public bool conditionalLockedStructureLocationBuilt;
    public List<bool> hubMerchantsHaveTalkLinesToShow_LevelObjectives;


    public int currentSpecialWaveAmount;
    public int NPCInteractionsIndex;
    public int levelManager_levelHubMerchantInteractionIndex;
    public int nightsSurvived;
    public int obstaclesRemoved;
    public int watcherArtifactFillUpAmount;
}