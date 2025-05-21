using Mono.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampEditManager : MonoBehaviour {
    public static CampEditManager Instance;

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
    [SerializeField] private List<StructureBlueprint> placedStructureBlueprints = new List<StructureBlueprint>();
    private CampGrid campGrid;

    private Vector2 mouseOffsetLocal;
    private Vector3 movingStructureInitialPosition;
    private Vector2Int previousOccupiedCell;
    private StructureBlueprint blueprintBeingMoved;
    private StructureBlueprint blueprintBeingAdded;
    private bool cancellingMovement;
    private bool dragging;
    private bool blueprintHasMoved;

    private bool playerJustPressedSelect;
    private float draggingTimer;
    private float draggingStartTime = .15f;

    public event EventHandler OnStructureRemoved;
    public event EventHandler OnStructureAdded;

    void Awake() {
        Instance = this;
        campGrid = GetComponent<CampGrid>();
    }

    private void Start() {
        GameInput.Instance.OnEditCampDeselect += GameInput_OnEditCampDeselect;
        GameInput.Instance.OnEditCampSelect += GameInput_OnEditCampSelect;
        GameInput.Instance.OnEditCampSelectReleased += GameInput_OnEditCampSelectReleased;

        scrollRectEvents.OnDragEnded += ScrollRectEvents_OnDragEnded;
        scrollRectEvents.OnDragStarted += ScrollRectEvents_OnDragStarted;
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
        campGrid.SetOccupiedCells(newCell, blueprintBeingMoved.widthInCells, true, blueprintBeingMoved);
        campGrid.SetSelectedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);

        blueprintBeingMoved.isBeingMoved = false;
        blueprintBeingMoved = null;
    }

    public StructureBlueprint GetBlueprintBeingMoved() => blueprintBeingMoved;
    public bool GetMovingBlueprint() => currentMode == CampEditMode.MovingStructure && blueprintBeingMoved != null;

    public void HoverGridCell(Vector2Int gridPos) {
        campGrid.ClearAllHovered();
        GridVisualUnit hoveredUnit = campGrid.GetGridVisualAt(gridPos);

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
        placedStructureBlueprints.Add(structure);

        foreach (var cell in structure.GetOccupiedCells()) {
            if (campGrid.gridVisuals.TryGetValue(cell, out var unit)) {
                unit.SetOccupyingStructure(structure);
            }
        }
    }

    public void RemoveStructure(StructureBlueprint structureBlueprint) {
        placedStructureBlueprints.Remove(structureBlueprint);

        campGrid.SetOccupiedCells(structureBlueprint.currentCell.x, structureBlueprint.widthInCells, false);
        campGrid.SetSelectedCells(structureBlueprint.currentCell.x, structureBlueprint.widthInCells, false);
        
        OnStructureRemoved?.Invoke(this, EventArgs.Empty);

        Destroy(structureBlueprint.gameObject);
    }

    public void AddStructure(StructureSO structureSO) {
        SetMode(CampEditMode.AddingStructure);

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

        //StartMoving(structureBlueprint, false);

        OnStructureAdded?.Invoke(this, EventArgs.Empty);
    }

    public void UnhoverAll() {
        campGrid.ClearAllHovered();
    }

    public int GetPlacedStructureBlueprintAmountOfType(StructureSO structureSO) {
        int amount = 0; 

        foreach(StructureBlueprint blueprint in placedStructureBlueprints) {
            if(blueprint.GetLinkedStructureSO().structureType == structureSO.structureType) {
                amount++;
            }
        }

        return amount;
    }

    private void GameInput_OnEditCampSelect(object sender, System.EventArgs e) {
        // Pickup blueprint logic
        if (currentMode == CampEditMode.AddingStructure) {

            campGrid.SetHoveredCells(blueprintBeingAdded.currentCell.x, blueprintBeingAdded.widthInCells, false);
            blueprintBeingAdded.SetHovered(false);

            SetMode(CampEditMode.None);
            return;
        };


        if (GetMovingBlueprint()) return;

        GridVisualUnit hoveredCell = campGrid.GetFirstHoveredCellWithStructure();

        if (hoveredCell == null) return;
        StructureBlueprint structure = hoveredCell.GetOccupyingStructure();

        if (!structure.GetLinkedStructureSO().structurePositionMovable) return;
        StartMoving(structure);

        playerJustPressedSelect = true;
        draggingTimer = 0;
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

    private void GameInput_OnEditCampDeselect(object sender, System.EventArgs e) {
        if (GetMovingBlueprint()) {
            CancelPlacement();
        }
        else {
            GridVisualUnit hoveredCell = campGrid.GetFirstHoveredCellWithStructure();
            if (hoveredCell == null) return;
            StructureBlueprint structure = hoveredCell.GetOccupyingStructure();

            if (!structure.GetLinkedStructureSO().structurePositionRemovable) return;
            RemoveStructure(structure);
        }

    }

    private void ScrollRectEvents_OnDragStarted(object sender, EventArgs e) {
        dragging = true;
        if (currentMode == CampEditMode.MovingStructure && !blueprintHasMoved) {
            StopMoving();
        }
    }

    private void ScrollRectEvents_OnDragEnded(object sender, EventArgs e) {
        dragging = false;
    }

    public void SetMode(CampEditMode mode) {
        currentMode = mode;
    }
}
