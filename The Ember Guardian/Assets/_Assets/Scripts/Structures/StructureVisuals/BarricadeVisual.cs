using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BarricadeVisual : StructureVisual {

    [SerializeField] private Light2D barricadeSpotLight;
    [SerializeField] private GameObject barricadeSpotLightGameObject;
    [SerializeField] private List<Animator> barricadeLightBodyAnimatorList;
    [SerializeField] private List<BarricadePiece> level1BarricadePieceList;
    [SerializeField] private List<BarricadePiece> level2BarricadePieceList;
    [SerializeField] private List<BarricadePiece> level3BarricadePieceList;
    [SerializeField] private List<BarricadePiece> level4BarricadePieceList;

    private List<BarricadePiece> currentLevelBarricadePieceList;
    private List<BarricadePiece> currentFallenBarricadePieceList = new List<BarricadePiece>();

    private Barricade barricade;
    private int spriteIndex = 1;
    private bool spotLightUnlocked = true;
    private bool outerBarricade;
    private bool playerOverrideSpotLightControl;
    private bool lightEnabled;

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
        barricade.OnFireLightTriggeredIn += Barricade_OnFireLightTriggeredIn;
        barricade.OnFireLightTriggeredOut += Barricade_OnFireLightTriggeredOut;
        barricade.OnBarricadeLightSwitched += Barricade_OnBarricadeLightSwitched;

       
        foreach (Animator animator in barricadeLightBodyAnimatorList) {
            if (barricade.GetStructureBuiltOnLoad()) {
                animator.SetTrigger("BuiltAtStart");
            } else {
                animator.SetTrigger("Build");
            }
        }
        


        currentLevelBarricadePieceList = level1BarricadePieceList;
        built = true;

        //spotLightUnlocked = ES3.Load("barricadeSpotLightUnlocked", false);
        spotLightUnlocked = true;
        if(!spotLightUnlocked) {
            barricadeSpotLightGameObject.SetActive(false);
        }
        SetXAxisScale();

        BuildPieces(level1BarricadePieceList);
    }

    private void Barricade_OnBarricadeLightSwitched(object sender, EventArgs e) {
        playerOverrideSpotLightControl = true;

        lightEnabled = !lightEnabled;
        barricadeSpotLight.enabled = lightEnabled;
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
        bool animateBuild = !barricade.GetStructureBuiltOnLoad();
        foreach (BarricadePiece piece in gameObjectList) {
            piece.BuildBarricadePiece(animateBuild);
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
            piece.EnableBarricadePiece();
            piece.BarricadePieceBuilt();
        }
        currentFallenBarricadePieceList.Clear();
    }

    public void SetAsOuterBarricade(bool outerBarricade) {
        this.outerBarricade = outerBarricade;
        if (!spotLightUnlocked) return;
        if (playerOverrideSpotLightControl) return;

        barricadeSpotLight.enabled = outerBarricade;
        lightEnabled = outerBarricade;
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

    private void Barricade_OnFireLightTriggeredOut(object sender, EventArgs e) {
        if (!outerBarricade) return;
        if (playerOverrideSpotLightControl) return;
        barricadeSpotLight.enabled = true;
        lightEnabled = true;
    }

    private void Barricade_OnFireLightTriggeredIn(object sender, EventArgs e) {
        if (!outerBarricade) return;
        if (playerOverrideSpotLightControl) return;
        barricadeSpotLight.enabled = false;
        lightEnabled = false;
    }

    protected void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (!spotLightUnlocked) return;
        if (playerOverrideSpotLightControl) return;
        barricadeSpotLight.enabled = outerBarricade;
        lightEnabled = outerBarricade;
    }

    protected void DayNightManager_OnDayStart(object sender, System.EventArgs e) {
        if (!spotLightUnlocked) return;
        if (playerOverrideSpotLightControl) return;
        barricadeSpotLight.enabled = false;
        lightEnabled = false;

    }
    public bool GetBarricadeHasAllSprites() {
        if (spriteIndex == 1) {
            return true;
        } else {
            return false;
        }
    }
    public void SyncWithHealth(int currentHealth, int maxHealth) {
        currentFallenBarricadePieceList.Clear();

        float healthNormalized = (float)currentHealth / (float)maxHealth;
        int totalPieces = currentLevelBarricadePieceList.Count;

        // combien de pièces doivent être "tombées"
        int fallenPieces = Mathf.RoundToInt((1f - healthNormalized) * totalPieces);

        for (int i = 0; i < totalPieces; i++) {
            if (i < fallenPieces) {
                currentLevelBarricadePieceList[i].DisableBarricadePiece(); // ou BarricadePieceFell() si tu veux la physique
                currentFallenBarricadePieceList.Add(currentLevelBarricadePieceList[i]);
            }
            else {
                currentLevelBarricadePieceList[i].EnableBarricadePiece();
            }
        }

        spriteIndex = Mathf.Clamp(fallenPieces + 1, 1, totalPieces + 1);
    }

    public void OnEnable() {
        //SetBuildAnimation(level1BarricadePieceList);
    }
}
