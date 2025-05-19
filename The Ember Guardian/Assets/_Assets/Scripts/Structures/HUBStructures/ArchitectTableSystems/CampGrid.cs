using System.Collections.Generic;
using UnityEngine;

public class CampGrid : MonoBehaviour {

    public static CampGrid Instance;
    private float cellSize = 50f;
    private float originX = 0f;

    private Dictionary<int, bool> occupiedCells = new Dictionary<int, bool>();

    private void Awake() {
        Instance = this;
    }

    public bool CanPlaceAt(int startCell, int width, StructureBlueprint blueprintToIgnore = null) {
        for (int i = startCell; i < startCell + width; i++) {
            if (IsCellOccupied(i, blueprintToIgnore))
                return false;
        }
        return true;
    }

    public void SetOccupiedCells(int startCell, int width, bool state, StructureBlueprint owner = null) {
        for (int i = startCell; i < startCell + width; i++) {
            if (state)
                occupiedCells[i] = true;
            else if (occupiedCells.ContainsKey(i))
                occupiedCells.Remove(i);
        }
    }

    private bool IsCellOccupied(int cellIndex, StructureBlueprint blueprintToIgnore) {
        foreach (var blueprint in FindObjectsOfType<StructureBlueprint>()) {
            if (blueprint == blueprintToIgnore) continue;

            int otherStart = blueprint.currentCell;
            int otherWidth = blueprint.widthInCells;

            if (cellIndex >= otherStart && cellIndex < otherStart + otherWidth)
                return true;
        }

        return false;
    }

    public int WorldToCell(float x) {
        return Mathf.FloorToInt((x - originX) / cellSize);
    }

    public float CellToWorld(int cellIndex) {
        return originX + cellIndex * cellSize;
    }

    public bool IsCellOccupied(int cellIndex) {
        return occupiedCells.TryGetValue(cellIndex, out bool occupied) && occupied;
    }

    public void SetCellOccupied(int cellIndex, bool state) {
        occupiedCells[cellIndex] = state;
    }

    public void ClearOccupiedCells() {
        occupiedCells.Clear();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        foreach (var kvp in occupiedCells) {
            if (kvp.Value) {
                float x = CellToWorld(kvp.Key);
                Vector3 pos = transform.TransformPoint(new Vector3(x, 0f, 0f));
                Gizmos.DrawWireCube(pos, new Vector3(cellSize, cellSize, 0));
            }
        }
    }
}
