using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CampEditManager : MonoBehaviour {
    public static CampEditManager Instance;
    public class StructurePlacementData {
        public int positionIndex;
        public StructureSO.StructureType structureType;

        public StructurePlacementData(int index, StructureSO.StructureType structureType) {
            this.positionIndex = index;
            this.structureType = structureType;
        }
    }

    public enum CampEditMode {
        None,
        MovingStructure,
        AddingStructure
    }
    private CampEditMode currentMode = CampEditMode.None;

    [SerializeField] private ScrollRectEventSenders scrollRectEvents;
    [SerializeField] private AutoScrollRect autoScrollRect;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform structureBlueprintPrefab;
    [SerializeField] private Transform structureBlueprintsParent;
    [SerializeField] private int tentBlueprintInitialCell;
    [SerializeField] private StructureBlueprint tentBlueprint;
    [SerializeField] private Dictionary<int, StructureBlueprint> placedStructureBlueprints = new Dictionary<int, StructureBlueprint>();
    [SerializeField] private GameObject initialStructureBlueprintsParentGO;
    List<StructurePlacementData> savedLayout = new List<StructurePlacementData>();
    private List<StructureSO.StructureType> structureTypesUnlockedThisSession = new List<StructureSO.StructureType>();
    private CampGrid campGrid;

    private Vector2 mouseOffsetLocal;
    private Vector3 movingStructureInitialPosition;
    private Vector2Int previousOccupiedCell;
    private StructureBlueprint blueprintBeingMoved;
    private StructureBlueprint blueprintBeingHovered;
    private StructureBlueprint blueprintBeingAdded;
    private bool cancellingMovement;
    private bool dragging;
    private bool blueprintHasMoved;
    private bool hordeMode;

    private bool playerJustPressedSelect;
    private float draggingTimer;
    private float draggingStartTime = .15f;

    public event EventHandler OnAnyChangeMade;
    public event EventHandler OnLayoutSaved;
    public event EventHandler OnLayoutResetToDefault;
    public event EventHandler OnAllStructuresRemoved;
    public event EventHandler OnStructureRemovedAnySituation;
    public event EventHandler OnStructureRemovedIndividually;
    public event EventHandler OnStructureAdded;

    public event EventHandler OnStructurePickedUp;
    public event EventHandler OnStructureDroppedMoving;

    void Awake() {
        Instance = this;
        campGrid = GetComponent<CampGrid>();
        hordeMode = SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.MainMenu;

        HubMerchantItem.OnAnyHubMerchantItemBought += HubMerchantItem_OnAnyHubMerchantItemBought;
        HubMerchantItem.OnAnyHubMerchantItemArchitectTableUnlocks += HubMerchantItem_OnAnyHubMerchantItemArchitectTableUnlocks;

        LoadCampLayout();
    }

    private void Start() {
        GameInput.Instance.OnEditCampDeselect += GameInput_OnEditCampDeselect;
        GameInput.Instance.OnEditCampSelect += GameInput_OnEditCampSelect;
        GameInput.Instance.OnEditCampSelectReleased += GameInput_OnEditCampSelectReleased;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;

        scrollRectEvents.OnDragEnded += ScrollRectEvents_OnDragEnded;
        scrollRectEvents.OnDragStarted += ScrollRectEvents_OnDragStarted;

        // Check if no structure was saved
        if (savedLayout.Count == 0) return;

        InitializeCampLayout(false);
    }

    void Update() {
        if(playerJustPressedSelect) {
            draggingTimer += Time.deltaTime;
            if(draggingTimer >= draggingStartTime) {
                playerJustPressedSelect = false;
            }
        }

        if (currentMode != CampEditMode.MovingStructure || blueprintBeingMoved == null) return;
        if (dragging) return;
        if (cancellingMovement) return;
        if (GameInput.Instance.IsUsingGamepad()) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            blueprintBeingMoved.transform.parent as RectTransform,
            Input.mousePosition, canvas.worldCamera, out Vector2 localMousePos);

        Vector2 localPointWithOffset = localMousePos - mouseOffsetLocal;
        Vector2Int hoveredCell = campGrid.WorldToCell(localPointWithOffset.x);

        if (hoveredCell == blueprintBeingMoved.currentCell) return;
        // Vérifie si on peut s’y déplacer

        // Empecher mouvement sur une structure existante
        if (!campGrid.CanPlaceAt(hoveredCell.x, blueprintBeingMoved.widthInCells, blueprintBeingMoved)) {
            //campGrid.SetUnvalidPositionCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells);
            //campGrid.SetUnvalidPositionCells(hoveredCell.x, blueprintBeingMoved.widthInCells);
            return;
        }

        blueprintHasMoved = true;

        // Libère ancienne position
        campGrid.SetOccupiedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);
        campGrid.SetSelectedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);

        // Snap à la nouvelle position
        float snappedX = campGrid.CellToWorld(hoveredCell.x);
        var rt = blueprintBeingMoved.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(snappedX, rt.anchoredPosition.y);

        // Met à jour la nouvelle cellule
        blueprintBeingMoved.currentCell = hoveredCell;

        // Réoccupe la nouvelle position
        campGrid.SetOccupiedCells(hoveredCell.x, blueprintBeingMoved.widthInCells, true, blueprintBeingMoved);
        campGrid.SetSelectedCells(hoveredCell.x, blueprintBeingMoved.widthInCells, true);
        
    }

    public void StartMoving(StructureBlueprint structure, bool useMouseOffset = true) {
        if (currentMode == CampEditMode.MovingStructure) return;
        SetMode(CampEditMode.MovingStructure);

        structure.SetMoving(true);
        blueprintBeingMoved = structure;
        movingStructureInitialPosition = blueprintBeingMoved.GetComponent<RectTransform>().anchoredPosition;
        blueprintHasMoved = false;
        previousOccupiedCell = blueprintBeingMoved.currentCell;

        // Calcule la position de la souris dans le repère local du parent
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            blueprintBeingMoved.transform.parent as RectTransform,
            Input.mousePosition, canvas.worldCamera, out Vector2 localMouse);

        // Calcule l’offset local entre la souris et le coin pivot de la structure
        RectTransform rt = blueprintBeingMoved.GetComponent<RectTransform>();
        Vector2 localPosition = rt.anchoredPosition + new Vector2Int((int)CampGrid.Instance.GetCellSize() / 2, 0);
        mouseOffsetLocal = localMouse - (localPosition);

        if(!useMouseOffset) {
            mouseOffsetLocal = Vector2.zero;
        }

        campGrid.SetSelectedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, true);
        OnStructurePickedUp?.Invoke(this, EventArgs.Empty);
    }

    private void CancelPlacement() {
        cancellingMovement = true;
        blueprintBeingMoved.SetMoving(false);
        campGrid.SetOccupiedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);

        blueprintBeingMoved.GetComponent<RectTransform>().anchoredPosition = movingStructureInitialPosition;

        campGrid.SetSelectedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);
        campGrid.SetOccupiedCells(previousOccupiedCell.x, blueprintBeingMoved.widthInCells, true, blueprintBeingMoved);
        blueprintBeingMoved.SetOccupiedCells();

        SetMode(CampEditMode.None);

        blueprintBeingMoved.isBeingMoved = false;
        blueprintBeingMoved = null;
        cancellingMovement = false;
    }

    private void StopMoving() {
        SetMode(CampEditMode.None);

        blueprintBeingMoved.SetMoving(false);
        int newCell = blueprintBeingMoved.currentCell.x;

        int oldKey = placedStructureBlueprints.FirstOrDefault(kvp => kvp.Value == blueprintBeingMoved).Key;
        placedStructureBlueprints.Remove(oldKey);
        placedStructureBlueprints[newCell] = blueprintBeingMoved;

        campGrid.SetOccupiedCells(newCell, blueprintBeingMoved.widthInCells, true, blueprintBeingMoved);
        campGrid.SetSelectedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);
        StartCoroutine(SetHoveredCellsAfterFrame(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, true));
        blueprintBeingMoved.isBeingMoved = false;
        blueprintBeingMoved = null;

        OnAnyChangeMade?.Invoke(this, EventArgs.Empty);
        OnStructureDroppedMoving?.Invoke(this, EventArgs.Empty);
    }

    private IEnumerator SetHoveredCellsAfterFrame(int startCell, int widthInCells, bool hovered) {
        yield return new WaitForEndOfFrame();
        campGrid.SetHoveredCells(startCell, widthInCells, hovered);
    }

    public void HoverGridCell(Vector2Int gridPos) {
        campGrid.ClearAllHovered();
        GridVisualUnit hoveredUnit = campGrid.GetGridVisualAt(gridPos);
        blueprintBeingAdded = null;

        if (hoveredUnit == null) return;

        var structure = hoveredUnit.GetOccupyingStructure();

        if (structure != null) {
            foreach (var pos in structure.GetOccupiedCells()) {
                var unit = campGrid.GetGridVisualAt(pos);
                if (unit != null) unit.SetHovered(true);
            }
        }
        else {
            hoveredUnit.SetHovered(true);
        }

    }

    public void RegisterStructure(StructureBlueprint structure) {
        int cellX = structure.currentCell.x;
        placedStructureBlueprints[cellX] = structure;

        foreach (var cell in structure.GetOccupiedCells()) {
            if (campGrid.gridVisuals.TryGetValue(cell, out var unit)) {
                unit.SetOccupyingStructure(structure);
            }
        }


        if (hordeMode) {
            ArchitectTable.Instance.RemoveFromBudget(structure.GetLinkedStructureSO().hordeModeGemBudget);
        }

    }

    public void RemoveStructure(StructureBlueprint structureBlueprint, bool removeAllStructures = false) {
        int cellX = structureBlueprint.currentCell.x;
        if (placedStructureBlueprints.ContainsKey(cellX)) {
            placedStructureBlueprints.Remove(cellX);
        }

        campGrid.SetOccupiedCells(structureBlueprint.currentCell.x, structureBlueprint.widthInCells, false);
        campGrid.SetSelectedCells(structureBlueprint.currentCell.x, structureBlueprint.widthInCells, false);

        if(!removeAllStructures) {
            // Dont call when all structures are removed simultaneously
            OnStructureRemovedIndividually?.Invoke(this, EventArgs.Empty);
        }

        OnStructureRemovedAnySituation?.Invoke(this, EventArgs.Empty);
        OnAnyChangeMade?.Invoke(this, EventArgs.Empty);

        if(structureBlueprint.GetIsInitialBlueprint()) {
            structureBlueprint.gameObject.SetActive(false);
        } else {
            Destroy(structureBlueprint.gameObject);
        }


        if (hordeMode) {
            ArchitectTable.Instance.AddToBudget(structureBlueprint.GetLinkedStructureSO().hordeModeGemBudget);
        }

    }

    public void AddStructure(StructureSO structureSO) {
        SetMode(CampEditMode.AddingStructure);

        if (blueprintBeingAdded != null) {
            blueprintBeingAdded.SetHovered(false);
            campGrid.SetHoveredCells(blueprintBeingAdded.currentCell.x, blueprintBeingAdded.widthInCells, false);
        }

        StructureBlueprint structureBlueprint = Instantiate(structureBlueprintPrefab, structureBlueprintsParent).GetComponent<StructureBlueprint>();
        structureBlueprint.SetStructureSO(structureSO);
        structureBlueprint.SetHovered(true);
        blueprintBeingAdded = structureBlueprint;

        int startCell = campGrid.FindFirstAvailablePosition(structureBlueprint.widthInCells, structureBlueprint);
        float snappedX = campGrid.CellToWorld(startCell);
        var rt = structureBlueprint.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(snappedX, rt.anchoredPosition.y);

        campGrid.SetOccupiedCells(startCell, structureBlueprint.widthInCells, true, structureBlueprint);
        campGrid.SetHoveredCells(startCell, structureBlueprint.widthInCells, true);

        RegisterStructure(structureBlueprint);

        autoScrollRect.CenterOn(structureBlueprint.GetComponent<RectTransform>());

        OnStructureAdded?.Invoke(this, EventArgs.Empty);
        OnAnyChangeMade?.Invoke(this, EventArgs.Empty);
    }

    public void UnhoverAll() {
        campGrid.ClearAllHovered();
    }

    public void SetBlueprintHovered(StructureBlueprint blueprint) {
        this.blueprintBeingHovered = blueprint;
    }

    private void HubMerchantItem_OnAnyHubMerchantItemBought(object sender, EventArgs e) {
        HubMerchantItem merchantItem = sender as HubMerchantItem;

        HubMerchantItem_GemMerchantItem gemMerchantItem = merchantItem as HubMerchantItem_GemMerchantItem;

        if (gemMerchantItem != null) {
            structureTypesUnlockedThisSession.Add(gemMerchantItem.GetStructureType());
        }

        HubMerchantItem_WatcherMerchantItem watcherMerchantItem = merchantItem as HubMerchantItem_WatcherMerchantItem;
        if (watcherMerchantItem != null) {
            if (!structureTypesUnlockedThisSession.Contains(watcherMerchantItem.GetStructureType())) {
                structureTypesUnlockedThisSession.Add(watcherMerchantItem.GetStructureType());
            }
        }
    }

    private void HubMerchantItem_OnAnyHubMerchantItemArchitectTableUnlocks(object sender, EventArgs e) {
        HubMerchantItem merchantItem = sender as HubMerchantItem;

        HubMerchantItem_WatcherMerchantItem watcherMerchantItem = merchantItem as HubMerchantItem_WatcherMerchantItem;
        if (watcherMerchantItem != null) {
            if(!structureTypesUnlockedThisSession.Contains(watcherMerchantItem.GetStructureType())) {
                structureTypesUnlockedThisSession.Add(watcherMerchantItem.GetStructureType());
            }

        }
    }

    private void GameInput_OnEditCampSelectReleased(object sender, System.EventArgs e) {
        // Drop bluprint logic

        if (!GetMovingBlueprint()) return;
        if (playerJustPressedSelect) return;
        if (dragging) return;

        if (campGrid.CanPlaceAt(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, blueprintBeingMoved)) {
            StopMoving();
            return;
        }

    }

    private void HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant(object sender, EventArgs e) {
        HubMerchant merchant = sender as HubMerchant;
        if (merchant.GetHubMerchantType() != HubMerchant.HubMerchantType.ArchitectTable) return;

        if (GetMovingBlueprint()) {
            CancelPlacement();
        }
    }

    private void GameInput_OnEditCampSelect(object sender, System.EventArgs e) {
        // Pickup blueprint logic
        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.HUB && !ArchitectTable.Instance.GetArchitectTableHubMerchant().GetPlayerInteractingWithMerchant()) return;
        if (GetMovingBlueprint()) return;
        if (blueprintBeingAdded != null) return;


        GridVisualUnit hoveredCell = campGrid.GetFirstHoveredCellWithStructure();

        if (hoveredCell == null) return;
        StructureBlueprint structure = hoveredCell.GetOccupyingStructure();

        if (!structure.GetLinkedStructureSO().structurePositionMovable) return;
        StartMoving(structure);

        playerJustPressedSelect = true;
        draggingTimer = 0;
    }

    private void GameInput_OnEditCampDeselect(object sender, System.EventArgs e) {
        if (GetMovingBlueprint()) {
            CancelPlacement();
        }
        else {
            if (blueprintBeingAdded != null) return;
            GridVisualUnit hoveredCell = campGrid.GetFirstHoveredCellWithStructure();
            if (hoveredCell == null) return;
            StructureBlueprint structure = hoveredCell.GetOccupyingStructure();

            if (!structure.GetLinkedStructureSO().structurePositionRemovable) return;
            RemoveStructure(structure);
        }

    }

    private void ScrollRectEvents_OnDragStarted(object sender, EventArgs e) {
        dragging = true;
        //if (currentMode == CampEditMode.MovingStructure && !blueprintHasMoved) {
        //    StopMoving();
        //}
    }

    private void ScrollRectEvents_OnDragEnded(object sender, EventArgs e) {
        dragging = false;
    }

    public void SetMode(CampEditMode mode) {
        currentMode = mode;
    }

    public void TryRemoveBlueprintFromCamp(StructureSO structureSO) {
        StructureBlueprint structureBlueprintToRemove = null;

        foreach (var kvp in placedStructureBlueprints) {
            StructureBlueprint blueprint = kvp.Value;

            if (structureSO == blueprint.GetLinkedStructureSO()) {
                structureBlueprintToRemove = blueprint;
            }
        }

        if(structureBlueprintToRemove != null) {
            RemoveStructure(structureBlueprintToRemove);
            OnAnyChangeMade?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RemoveAllStructure(bool triggerSFX = true) {
        List<StructureBlueprint> structureBlueprintsToRemove = new List<StructureBlueprint>();

        foreach (var kvp in placedStructureBlueprints) {
            StructureBlueprint blueprint = kvp.Value;

            if (blueprint.GetLinkedStructureSO().structurePositionRemovable) {
                structureBlueprintsToRemove.Add(blueprint);
            }
        }

        foreach (StructureBlueprint blueprintToRemove in structureBlueprintsToRemove) {
            RemoveStructure(blueprintToRemove, true);
        }

        //RemoveStructure(tentBlueprint, false);

        OnAnyChangeMade?.Invoke(this, EventArgs.Empty);

        if(triggerSFX) {
            OnAllStructuresRemoved?.Invoke(this, EventArgs.Empty);
        }

    }

    public void ResetToDefault() {
        StartCoroutine(ResetToDefaultCoroutine());
    }

    public IEnumerator ResetToDefaultCoroutine() {
        RemoveAllStructure();

        yield return new WaitForEndOfFrame();

        StructureBlueprint[] structureBlueprints = initialStructureBlueprintsParentGO.GetComponentsInChildren<StructureBlueprint>(true);
        savedLayout = new List<StructurePlacementData>();

        PlaceBlueprintOnGrid(tentBlueprint, tentBlueprintInitialCell);

        foreach (StructureBlueprint blueprint in structureBlueprints) {
            if (blueprint.GetBlueprintLocked()) continue;
            blueprint.gameObject.SetActive(true);
            blueprint.SetOccupiedCells();
            PlaceBlueprintOnGrid(blueprint, blueprint.currentCell.x);
        }

        OnAnyChangeMade?.Invoke(this, EventArgs.Empty);
        OnLayoutResetToDefault?.Invoke(this, EventArgs.Empty);

    }

    public void SaveCampLayout() {
        List<StructurePlacementData> layoutToSave = new List<StructurePlacementData>();

        foreach (var pair in placedStructureBlueprints) {
            StructureSO so = pair.Value.GetLinkedStructureSO();
            StructureSO.StructureType type = pair.Value.GetLinkedStructureSO().structureType;

            if (!so.structurePositionMovable) {
                continue; // Skip les structures non déplaçables
            }
            layoutToSave.Add(new StructurePlacementData(pair.Key, type));
        }

        if(hordeMode) {
            ES3.Save("campLayout_HordeMode", layoutToSave);
        } else {
            ES3.Save("campLayout", layoutToSave);
        }


        OnLayoutSaved?.Invoke(this, EventArgs.Empty);
    }

    public void LoadCampLayout() {
        //Debug.Log("LoadCampLayout " + hordeMode);
        if(hordeMode) {
            savedLayout = ES3.Load("campLayout_HordeMode", new List<StructurePlacementData>());
        } else {
            savedLayout = ES3.Load("campLayout", new List<StructurePlacementData>());
        }

        //Debug.Log("LoadCampLayout " + savedLayout.Count);
    }

    public bool GetCampLayoutCustomized() {
        return savedLayout.Count > 0;
    }

    public void InitializeCampLayout(bool triggerSFX = true) {
        RemoveAllStructure(triggerSFX);

        foreach (var data in savedLayout) {

            StructureSO structureSO = StructuresManager.Instance.GetStructureSO(data.structureType);

            if (!structureSO.structurePositionMovable || !structureSO.structurePositionRemovable) {

                if(structureSO.structureType == StructureSO.StructureType.tent) {
                    int tentStartCell = data.positionIndex;
                    tentBlueprint.gameObject.SetActive(true);
                    PlaceBlueprintOnGrid(tentBlueprint, tentStartCell);
                }

                continue; // Skip les structures non déplaçables
            }

            if (structureSO.structureLocationPrefab == null) continue;

            StructureBlueprint structureBlueprint = Instantiate(structureBlueprintPrefab, structureBlueprintsParent).GetComponent<StructureBlueprint>();
            structureBlueprint.SetStructureSO(structureSO);

            int startCell = data.positionIndex;
            PlaceBlueprintOnGrid(structureBlueprint, startCell);
        }
    }

    private void PlaceBlueprintOnGrid(StructureBlueprint structureBlueprint, int startCell) {
        float snappedX = campGrid.CellToWorld(startCell);
        var rt = structureBlueprint.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(snappedX, rt.anchoredPosition.y);
        campGrid.SetOccupiedCells(startCell, structureBlueprint.widthInCells, true, structureBlueprint);

        RegisterStructure(structureBlueprint);
    }

    public bool GetStructureTypeUnlockedThisSession(StructureSO.StructureType structureType) {
        return structureTypesUnlockedThisSession.Contains(structureType);
    }
    
    public StructureBlueprint GetBlueprintBeingMoved() => blueprintBeingMoved;
    public bool GetMovingBlueprint() => currentMode == CampEditMode.MovingStructure && blueprintBeingMoved != null;
    public StructureBlueprint GetBlueprintHovered() {
        return blueprintBeingHovered;
    }

    public int GetPlacedStructureBlueprintAmountOfType(StructureSO structureSO) {
        int amount = 0;

        foreach (StructureBlueprint blueprint in placedStructureBlueprints.Values) {
            if (blueprint.GetLinkedStructureSO().structureType == structureSO.structureType) {
                amount++;
            }
        }

        return amount;
    }

    public StructureBlueprint GetStructureBlueprintBeingAdded() {
        return blueprintBeingAdded;
    }

    private void OnDestroy() {
        HubMerchantItem.OnAnyHubMerchantItemBought -= HubMerchantItem_OnAnyHubMerchantItemBought;
        HubMerchantItem.OnAnyHubMerchantItemArchitectTableUnlocks -= HubMerchantItem_OnAnyHubMerchantItemArchitectTableUnlocks;
        HubMerchant.OnPlayerStoppedInteractingWithAnyHubMerchant -= HubMerchant_OnPlayerStoppedInteractingWithAnyHubMerchant;
    }

}
