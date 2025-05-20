using System.Collections.Generic;
using UnityEngine;

public class CampGrid : MonoBehaviour {

    [SerializeField] private RectTransform visualGridContainer;
    [SerializeField] private GridVisualUnit gridVisualPrefab;

    public static CampGrid Instance;
    public int gridSize = 100;
    private float cellSize = 50f;
    private float gridOriginX = 0f;

    public Dictionary<Vector2Int, GridVisualUnit> gridVisuals = new Dictionary<Vector2Int, GridVisualUnit>();
    private Dictionary<int, bool> occupiedCells = new Dictionary<int, bool>();

    private void Awake() {
        gridOriginX = visualGridContainer.rect.xMin;

        Instance = this;
        InstatiateVisuals();
    }
    void Start() {
    }

    private void InstatiateVisuals() {
        for (int x = 0; x < gridSize; x++) {
            Vector2Int pos = new Vector2Int(x, 0);
            GridVisualUnit visual = Instantiate(gridVisualPrefab, visualGridContainer);
            visual.gameObject.SetActive(true);
            visual.GetComponent<RectTransform>().anchoredPosition = new Vector2(x * cellSize, 300);
            visual.SetGridPos(pos);
            gridVisuals.Add(pos, visual);
        }
    }

    public GridVisualUnit GetGridVisualAt(Vector2Int pos) {
        gridVisuals.TryGetValue(pos, out var unit);
        return unit;
    }

    public void ClearAllHovered() {
        foreach (var unit in gridVisuals.Values) {
            unit.SetHovered(false);
        }
    }

    public bool CanPlaceAt(int startCell, int width, StructureBlueprint blueprintToIgnore = null) {
        for (int i = startCell; i < startCell + width; i++) {
            if (IsCellOccupied(i, blueprintToIgnore) || i >= gridSize || i < 0) {
                return false;
            }
        }
        return true;
    }

    private bool IsCellOccupied(int cellIndex, StructureBlueprint blueprintToIgnore) {
        foreach (var blueprint in FindObjectsOfType<StructureBlueprint>()) {
            if (blueprint == blueprintToIgnore) continue;

            int otherStart = blueprint.currentCell.x;
            int otherWidth = blueprint.widthInCells;

            if (cellIndex >= otherStart && cellIndex < otherStart + otherWidth)
                return true;
        }

        return false;
    }

    public float CellToWorld(int cellX) {
        return gridOriginX + (cellX * cellSize);
    }

    public Vector2Int WorldToCell(float worldX) {
        int cellX = Mathf.FloorToInt((worldX - gridOriginX) / cellSize);
        return new Vector2Int(cellX, 0);
    }

    public bool IsCellOccupied(int cellIndex) {
        return occupiedCells.TryGetValue(cellIndex, out bool occupied) && occupied;
    }

    public void SetOccupiedCells(int startCell, int width, bool occupied, StructureBlueprint blueprint = null) {

        for (int i = 0; i < width; i++) {
            Vector2Int cell = new Vector2Int(startCell + i, 0);

            if (gridVisuals.TryGetValue(cell, out var unit)) {

                if(occupied) {
                    unit.SetOccupied(blueprint);
                    blueprint.SetOccupiedCells();
                } else {
                    unit.ClearOccupied();
                }
            }

        }
    }

    public void SetSelectedCells(int startCell, int width, bool selected) {

        for (int i = 0; i < width; i++) {
            Vector2Int cell = new Vector2Int(startCell + i, 0);

            if (gridVisuals.TryGetValue(cell, out var unit)) {
                if(selected) {
                    unit.SetSelected(true);
                }
                else {
                    unit.SetSelected(false);
                }
            }

        }
    }

    public float GetCellSize() {
        return cellSize;
    }

    public int GetGridSize() {
        return gridSize;
    }
}
