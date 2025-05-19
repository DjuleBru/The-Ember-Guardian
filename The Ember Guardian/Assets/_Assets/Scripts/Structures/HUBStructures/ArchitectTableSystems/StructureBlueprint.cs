using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StructureBlueprint : MonoBehaviour, IPointerClickHandler {

    public int widthInCells = 3;
    public bool isBeingMoved = false;
    public int currentCell = 0;

    private void Start() {
        SetOccupiedCells(CampGrid.Instance, true);
    }

    public int GetStartCell(CampGrid grid) {
        return grid.WorldToCell(GetComponent<RectTransform>().anchoredPosition.x);
    }

    public bool CanPlaceAt(int startCell, CampGrid grid) {
        for (int i = 0; i < widthInCells; i++) {
            if (grid.IsCellOccupied(startCell + i)) return false;
        }
        return true;
    }

    public void SetOccupiedCells(CampGrid grid, bool state) {
        int start = GetStartCell(grid);
        for (int i = 0; i < widthInCells; i++) {
            grid.SetCellOccupied(start + i, state);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (CampEditManager.Instance.GetBlueprintBeingMoved() == null && !CampEditManager.Instance.GetMovingBlueprint()) {
            isBeingMoved = true;
            CampEditManager.Instance.StartMoving(this);
        }
    }

    public void StopMoving() {
        isBeingMoved = false;
    }
}
