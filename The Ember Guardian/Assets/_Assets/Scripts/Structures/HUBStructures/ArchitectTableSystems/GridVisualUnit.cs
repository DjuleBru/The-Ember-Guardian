using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridVisualUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    [SerializeField] private Image background;
    [SerializeField] private Image border;

    private Color initialBackgroundColor;
    private Color initialBorderColor;
    [SerializeField] private Color occupiedColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unvalidPlacementColor;
    [SerializeField] private Color unMovableColor;

    private bool selected;
    private bool hovered;
    private bool movable = true;
    private Vector2Int gridPos;
    private StructureBlueprint occupyingStructure;

    private void Awake() {
        initialBackgroundColor = background.color;
        initialBorderColor = border.color;
    }

    public void SetHovered(bool hovered) {
        if (selected) return;
        if (this.hovered == hovered) return;
        this.hovered = hovered;

        if (hovered) {

            if(CampEditManager.Instance.GetMovingBlueprint() && occupyingStructure != null) {
                background.color = unvalidPlacementColor;
            } else {
                Color color = background.color;
                color.a += .1f;
                background.color = color;
            }

        }
        else {

            if (occupyingStructure != null) {

                if(movable) {
                    background.color = occupiedColor;
                } else {
                    background.color = unMovableColor;
                }

            } else {
                background.color = initialBackgroundColor;
            }
        }
    }

    public void SetOccupied(StructureBlueprint structure) {
        occupyingStructure = structure;
        background.color = occupiedColor;

        movable = occupyingStructure.GetLinkedStructureSO().structurePositionEditable;

        if(!movable) {
            background.color = unMovableColor;
        }
    }

    public void ClearOccupied() {
        occupyingStructure = null;
        background.color = initialBackgroundColor;
    }

    public StructureBlueprint GetOccupyingStructure() => occupyingStructure;

    public void SetSelected(bool selected) {
        this.selected = selected;
        Color color = background.color;
        if (selected) {
            border.color = selectedColor;
            color.a += .1f;
        } else {
            border.color = initialBorderColor;
        }
        background.color = color;
    }

    public void SetGridPos(Vector2Int pos) => gridPos = pos;
    public Vector2Int GetGridPos() => gridPos;

    public void OnPointerClick(PointerEventData eventData) {
        if (!movable) return;
        if (occupyingStructure == null) return;

        if (CampEditManager.Instance.GetBlueprintBeingMoved() == null && !CampEditManager.Instance.GetMovingBlueprint()) {
            CampEditManager.Instance.StartMoving(occupyingStructure);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (hovered) return;
        CampEditManager.Instance.HoverGridCell(gridPos);

        if(occupyingStructure != null) {
            occupyingStructure.SetHovered(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!hovered) return;

        CampEditManager.Instance.UnhoverAll();

        if (occupyingStructure != null) {
            occupyingStructure.SetHovered(false);
        }
    }
}
