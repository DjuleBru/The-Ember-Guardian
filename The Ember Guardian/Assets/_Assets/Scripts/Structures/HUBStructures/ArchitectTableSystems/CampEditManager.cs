using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampEditManager : MonoBehaviour {
    public static CampEditManager Instance;

    [SerializeField] private Canvas canvas;
    private CampGrid campGrid;

    private Vector2 mouseOffsetLocal;
    private Vector3 movingStructureInitialPosition;
    private Vector2Int previousOccupiedCell;
    private StructureBlueprint blueprintBeingMoved;
    private bool movingBlueprint;

    private bool playerJustPressedSelect;
    private float draggingTimer;
    private float draggingStartTime = .15f;

    void Awake() {
        Instance = this;
        campGrid = GetComponent<CampGrid>();
    }

    private void Start() {
        GameInput.Instance.OnEditCampDeselect += GameInput_OnEditCampDeselect;
        GameInput.Instance.OnEditCampSelect += GameInput_OnEditCampSelect;
        GameInput.Instance.OnEditCampSelectReleased += GameInput_OnEditCampSelectReleased;
    }


    void Update() {
        if(playerJustPressedSelect) {
            draggingTimer += Time.deltaTime;
            if(draggingTimer >= draggingStartTime) {
                playerJustPressedSelect = false;
            }
        }

        if (!movingBlueprint || blueprintBeingMoved == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            blueprintBeingMoved.transform.parent as RectTransform,
            Input.mousePosition, canvas.worldCamera, out Vector2 localMousePos);

        Vector2 localPointWithOffset = localMousePos - mouseOffsetLocal;
        Vector2Int hoveredCell = campGrid.WorldToCell(localPointWithOffset.x);

        if (hoveredCell != blueprintBeingMoved.currentCell) {

            // Vérifie si on peut s’y déplacer

            if (campGrid.CanPlaceAt(hoveredCell.x, blueprintBeingMoved.widthInCells, blueprintBeingMoved)) {
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
        }
    }


    public void StartMoving(StructureBlueprint structure) {
        blueprintBeingMoved = structure;
        movingStructureInitialPosition = blueprintBeingMoved.GetComponent<RectTransform>().position;
        movingBlueprint = true;
        previousOccupiedCell = blueprintBeingMoved.currentCell;

        // Calcule la position de la souris dans le repère local du parent
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            blueprintBeingMoved.transform.parent as RectTransform,
            Input.mousePosition, canvas.worldCamera, out Vector2 localMouse);

        // Calcule l’offset local entre la souris et le coin pivot de la structure
        RectTransform rt = blueprintBeingMoved.GetComponent<RectTransform>();
        Vector2 localPosition = rt.anchoredPosition + new Vector2Int((int)CampGrid.Instance.GetCellSize() / 2, 0);
        mouseOffsetLocal = localMouse - (localPosition);

        campGrid.SetSelectedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, true);
    }

    private void CancelPlacement() {
        campGrid.SetOccupiedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);

        blueprintBeingMoved.GetComponent<RectTransform>().position = movingStructureInitialPosition;

        campGrid.SetSelectedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);
        campGrid.SetOccupiedCells(previousOccupiedCell.x, blueprintBeingMoved.widthInCells, true, blueprintBeingMoved);
        blueprintBeingMoved.SetOccupiedCells();

        blueprintBeingMoved.isBeingMoved = false;
        blueprintBeingMoved = null;
        movingBlueprint = false;
    }

    private IEnumerator StopMoving() {
        int newCell = blueprintBeingMoved.currentCell.x;
        campGrid.SetOccupiedCells(newCell, blueprintBeingMoved.widthInCells, true, blueprintBeingMoved);
        campGrid.SetSelectedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);

        movingBlueprint = false;
        yield return new WaitForSeconds(.1f);

        blueprintBeingMoved.isBeingMoved = false;
        blueprintBeingMoved = null;
    }

    public StructureBlueprint GetBlueprintBeingMoved() => blueprintBeingMoved;
    public bool GetMovingBlueprint() => movingBlueprint;

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
        foreach (var cell in structure.GetOccupiedCells()) {
            if (campGrid.gridVisuals.TryGetValue(cell, out var unit)) {
                unit.SetOccupied(structure);
            }
        }
    }

    public void UnhoverAll() {
        campGrid.ClearAllHovered();
    }

    private void GameInput_OnEditCampSelect(object sender, System.EventArgs e) {
        playerJustPressedSelect = true;
        draggingTimer = 0;
    }

    private void GameInput_OnEditCampSelectReleased(object sender, System.EventArgs e) {
        if (!playerJustPressedSelect) return;
        if (!movingBlueprint) return;

        if (campGrid.CanPlaceAt(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, blueprintBeingMoved)) {
            StartCoroutine(StopMoving());
        }
    }

    private void GameInput_OnEditCampDeselect(object sender, System.EventArgs e) {
        if (!movingBlueprint) return;
        CancelPlacement();
    }
}
