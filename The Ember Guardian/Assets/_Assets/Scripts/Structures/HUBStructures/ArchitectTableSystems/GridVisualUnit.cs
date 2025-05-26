using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridVisualUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler {

    [SerializeField] private Image background;
    [SerializeField] private Image border;

    [SerializeField] private Animator animator;
    [SerializeField] private Color initialBackgroundColor;
    [SerializeField] private Color initialBorderColor;
    [SerializeField] private Color occupiedColor;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color unvalidPlacementColor;
    [SerializeField] private Color unMovableColor;

    private bool selected;
    private bool hovered;
    private bool movable = true;
    private Vector2Int gridPos;
    private StructureBlueprint occupyingStructure;
    public static event EventHandler OnAnyGridWithoutStructureHovered;
    public static event EventHandler OnAnyGridHoveredWhileMovingBlueprint;
    public static bool justSentOnAnyGridHoveredWhileMovingBlueprintEvent;
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

            if(occupyingStructure == null) {
                OnAnyGridWithoutStructureHovered?.Invoke(this, EventArgs.Empty);
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
                CampEditManager.Instance.SetBlueprintHovered(null);
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
            hovered = true;
            if (movable) {
                animator.ResetTrigger("Hide");
                animator.SetTrigger("Show");
            };
            border.color = selectedColor;
            color.a += .1f;

            if(CampEditManager.Instance.GetMovingBlueprint() && !justSentOnAnyGridHoveredWhileMovingBlueprintEvent) {
                OnAnyGridHoveredWhileMovingBlueprint?.Invoke(this, EventArgs.Empty);
                justSentOnAnyGridHoveredWhileMovingBlueprintEvent = true;
                StartCoroutine(SetJustSentOnAnyGridHoveredWhileMovingBlueprintEventAfterFrame());
            }

        } else {
            hovered = false;

            if (movable) {
                animator.ResetTrigger("Show");
                animator.SetTrigger("Hide");
            };
            border.color = initialBorderColor;

        }

        background.color = color;
    }

    private IEnumerator SetJustSentOnAnyGridHoveredWhileMovingBlueprintEventAfterFrame() {
        yield return new WaitForEndOfFrame();
        justSentOnAnyGridHoveredWhileMovingBlueprintEvent=false;
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

    public void OnDeselect(BaseEventData eventData) {
        if (!hovered) return;

        CampEditManager.Instance.UnhoverAll();

        if (occupyingStructure != null) {
            occupyingStructure.SetHovered(false);
        }
    }

    public void OnSelect(BaseEventData eventData) {
        if (hovered) return;

        CampEditManager.Instance.HoverGridCell(gridPos);

        if (occupyingStructure != null) {
            occupyingStructure.SetHovered(true);
        }
    }

    public void OnEnable() {
        if(occupyingStructure != null && !occupyingStructure.GetLinkedStructureSO().structurePositionMovable) {
            animator.ResetTrigger("Hide");
            animator.SetTrigger("Show");
        }
    }
}
