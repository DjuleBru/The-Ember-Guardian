using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridVisualUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private Image background;
    [SerializeField] private Image border;

    private Animator animator;
    private Color initialBackgroundColor;
    private Color initialBorderColor;
    [SerializeField] private Color occupiedColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unvalidPlacementColor;
    [SerializeField] private Color unMovableColor;

    private bool selected;
    private bool hovered;
    private bool movable = true;
    private bool removable = true;
    private Vector2Int gridPos;
    private StructureBlueprint occupyingStructure;

    private void Awake() {
        animator = GetComponent<Animator>();

        initialBackgroundColor = background.color;
        initialBorderColor = border.color;
    }
    public void SetHovered(bool hovered) {
        if (selected) return;
        if (this.hovered == hovered) return;

        this.hovered = hovered;

        if (hovered) {
            if (movable) {
                animator.ResetTrigger("Hide");
                animator.SetTrigger("Show");
            };

            if (CampEditManager.Instance.GetMovingBlueprint() && occupyingStructure != CampEditManager.Instance.GetBlueprintBeingMoved() && occupyingStructure != null) {
                background.color = unvalidPlacementColor;
            } else {
                Color color = background.color;
                color.a += .1f;
                background.color = color;
            }

        }
        else {
            if (movable) {
                animator.ResetTrigger("Show");
                animator.SetTrigger("Hide");
            };

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

    public void SetUnvalidBackgroundColor() {
        background.color = unvalidPlacementColor;
    }

    public void SetOccupyingStructure(StructureBlueprint structure) {
        occupyingStructure = structure;
        background.color = occupiedColor;

        movable = occupyingStructure.GetLinkedStructureSO().structurePositionMovable;
        removable = occupyingStructure.GetLinkedStructureSO().structurePositionRemovable;

        if(!movable) {
            animator.SetTrigger("Show");
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

            if (movable) {
                animator.ResetTrigger("Hide");
                animator.SetTrigger("Show");
            };
            border.color = selectedColor;
            color.a += .1f;

        } else {

            if (movable) {
                animator.ResetTrigger("Show");
                animator.SetTrigger("Hide");
            };
            border.color = initialBorderColor;

        }

        background.color = color;
    }

    public void SetGridPos(Vector2Int pos) => gridPos = pos;
    public Vector2Int GetGridPos() => gridPos;

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

    public bool IsHovered() {
        return hovered;
    }
}
