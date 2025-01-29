using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BarricadeVisual : StructureVisual {

    [SerializeField] private Light2D barricadeSpotLight;
    [SerializeField] private Animator barricadeLightBodyAnimator;
    [SerializeField] private List<BarricadePiece> level1BarricadePieceList;
    [SerializeField] private List<BarricadePiece> level2BarricadePieceList;
    [SerializeField] private List<BarricadePiece> level3BarricadePieceList;
    [SerializeField] private List<BarricadePiece> level4BarricadePieceList;

    private List<BarricadePiece> currentLevelBarricadePieceList;
    private List<BarricadePiece> currentFallenBarricadePieceList = new List<BarricadePiece>();

    private Barricade barricade;
    private int spriteIndex = 1;

    public event EventHandler OnBarricadeSpriteFell;

    protected override void Awake() {
        base.Awake();
        barricade = GetComponentInParent<Barricade>();

    }

    protected override void Start() {
        structure.OnStructureUpgraded += Structure_OnStructureUpgraded;
        structure.OnStructureInteractionsUpdated += Structure_OnStructureInteractionsUpdated;
        structure.OnPlayerTriggeredIn += Structure_OnPlayerTriggeredIn;
        structure.OnPlayerTriggeredOut += Structure_OnPlayerTriggeredOut;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Level || SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) {
            DayNightManager.Instance.OnDayStart += DayNightManager_OnDayStart;
            DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
            PlayerCampVisual.Instance.OnCampBackgroundBuilt += PlayerCampVisual_OnCampBackgroundBuilt;
        }

        barricade.OnBarricadeDamageTaken += Barricade_OnBarricadeDamageTaken;
        barricade.OnBarricadeRepaired += Barricade_OnBarricadeRepaired;

        BuildPieces(level1BarricadePieceList);
        barricadeLightBodyAnimator.SetTrigger("Build");
        currentLevelBarricadePieceList = level1BarricadePieceList;
        built = true;
    }

    private void Barricade_OnBarricadeRepaired(object sender, System.EventArgs e) {
        RepairStructureVisual();
        spriteIndex = 1;
    }

    private void Barricade_OnBarricadeDamageTaken(object sender, System.EventArgs e) {

        float barricadeHealthNormalized = barricade.GetBarricadeHealthNormalized();
        float spriteIndexNormalized = 1 - ((float)spriteIndex / (float)currentLevelBarricadePieceList.Count);

        if (spriteIndex == currentLevelBarricadePieceList.Count +1) return;

        if(barricadeHealthNormalized <= spriteIndexNormalized) {
            currentLevelBarricadePieceList[spriteIndex - 1].BarricadePieceFell();
            currentFallenBarricadePieceList.Add(currentLevelBarricadePieceList[spriteIndex - 1]);

            OnBarricadeSpriteFell?.Invoke(this, EventArgs.Empty);
            spriteIndex++;

        } else {
            currentLevelBarricadePieceList[spriteIndex - 1].BarricadePieceDamaged();
        }

    }

    protected override void Structure_OnStructureUpgraded(object sender, System.EventArgs e) {
        DeactivateAllBarricadePieces();
        currentFallenBarricadePieceList.Clear();

        int structureLevel = structure.GetStructureLevel();

        if(structureLevel == 2) {
            ActivatePieces(level2BarricadePieceList);
        }
        if (structureLevel == 3) {
            ActivatePieces(level3BarricadePieceList);
        }
        if (structureLevel == 4) {
            ActivatePieces(level4BarricadePieceList);
        }

    }

    private void BuildPieces(List<BarricadePiece> gameObjectList) {
        foreach (BarricadePiece piece in gameObjectList) {
            piece.BuildBarricadePiece();
        }
    }

    private void ActivatePieces(List<BarricadePiece> gameObjectList) {
        foreach (BarricadePiece piece in gameObjectList) {
            piece.EnableBarricadePiece();
        }
        currentLevelBarricadePieceList = gameObjectList;
    }

    private void SetBuildAnimation(List<BarricadePiece> BarricadePieceList) {
        foreach (BarricadePiece piece in BarricadePieceList) {
            piece.BarricadePieceBuilt();
        }

    }

    private void DeactivateAllBarricadePieces() {

        foreach (BarricadePiece barricadePiece in level1BarricadePieceList) {
            barricadePiece.DisableBarricadePiece();
        }
        foreach (BarricadePiece barricadePiece in level2BarricadePieceList) {
            barricadePiece.DisableBarricadePiece();
        }
        foreach (BarricadePiece barricadePiece in level3BarricadePieceList) {
            barricadePiece.DisableBarricadePiece();
        }
        foreach (BarricadePiece barricadePiece in level4BarricadePieceList) {
            barricadePiece.DisableBarricadePiece();
        }
    }

    public void ShowRepairStructureVisual(bool show) {

        if (show) {
            foreach(BarricadePiece piece in currentFallenBarricadePieceList) {
                piece.ShowBarricadePieceRepairable();
            }
        }
        else {
            foreach (BarricadePiece piece in currentFallenBarricadePieceList) {
                piece.DisableBarricadePiece();
            }

        }
        
    }

    protected void RepairStructureVisual() {
        foreach (BarricadePiece piece in currentFallenBarricadePieceList) {
            piece.BarricadePieceBuilt();
        }
        currentFallenBarricadePieceList.Clear();
    }

    public void SetAsOuterBarricade(bool outerBarricade) {
        barricadeSpotLight.enabled = outerBarricade;
    }

    protected override void Structure_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        foreach(BarricadePiece piece in currentLevelBarricadePieceList) {
            piece.SetHovered(false);
        }
    }

    protected override void Structure_OnPlayerTriggeredIn(object sender, System.EventArgs e) {
        foreach (BarricadePiece piece in currentLevelBarricadePieceList) {
            piece.SetHovered(true);
        }
    }

    public bool GetBarricadeHasAllSprites() {
        if (spriteIndex == 1) {
            return true;
        } else {
            return false;
        }
    }

    public void OnEnable() {
        SetBuildAnimation(level1BarricadePieceList);
    }
}
