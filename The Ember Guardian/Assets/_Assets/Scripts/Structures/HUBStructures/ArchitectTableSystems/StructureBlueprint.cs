using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StructureBlueprint : MonoBehaviour {

    [SerializeField] private StructureSO linkedStructureSO;
    [SerializeField] private Image blueprintImage;
    [SerializeField] private Animator linkedUIHoveredObjectAnimator;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material emptyMaterial;
    [SerializeField] private bool isInitialBlueprint;
    private Animator blueprintAnimator;

    public int widthInCells = 3;
    public bool isBeingMoved = false;
    public bool isBeingRemoved = false;
    public Vector2Int currentCell;
    public List<Vector2Int> occupiedCells = new List<Vector2Int>();

    private bool hovered;
    public event EventHandler OnBlueprintHovered;
    public event EventHandler OnBlueprintUnhovered;
    public event EventHandler OnStructureStartedMoving;
    public event EventHandler OnStructureStoppedMoving;

    private void Awake() {
        blueprintAnimator = GetComponent<Animator>();
    }

    void Start() {

        if (linkedStructureSO != null) {
            blueprintImage.sprite = linkedStructureSO.structureSprite;
            SetBlueprintSize();
        }

        if (isInitialBlueprint && linkedStructureSO.structurePositionMovable && CampEditManager.Instance.GetCampLayoutCustomized()) {

            if (linkedStructureSO.structureType != StructureSO.StructureType.tent) {
                gameObject.SetActive(false);
                return;
            };

        };

        // Calcul de la cellule à partir de la position actuelle
        SetOccupiedCells();


        if(isInitialBlueprint) {
            CampEditManager.Instance.RegisterStructure(this);
        }
    }

    public List<Vector2Int> GetOccupiedCells() {
        return occupiedCells;
    }

    public void SetOccupiedCells() {
        float x = GetComponent<RectTransform>().anchoredPosition.x;
        currentCell = CampGrid.Instance.WorldToCell(x);

        occupiedCells.Clear();
        for (int i = 0; i < widthInCells; i++) {
            occupiedCells.Add(new Vector2Int(currentCell.x + i, 0));
        }

        if(currentCell.x < CampGrid.Instance.GetGridSize()/2) {
            blueprintImage.GetComponent<RectTransform>().localScale = new Vector2(-1, 1);
        } else {
            blueprintImage.GetComponent<RectTransform>().localScale = new Vector2(1, 1);
        }
    }

    public StructureSO GetLinkedStructureSO() {
        return linkedStructureSO;
    }

    public void SetStructureSO(StructureSO structureSO) {
        linkedStructureSO = structureSO;
        blueprintImage.sprite = linkedStructureSO.structureSprite;
        widthInCells = structureSO.widthInCells;

        SetOccupiedCells();
        SetBlueprintSize();
    }

    public void SetBlueprintSize() {
        GetComponent<RectTransform>().sizeDelta = new Vector2(widthInCells * CampGrid.Instance.GetCellSize(), linkedStructureSO.structureSprite.rect.height * 2.8f);
        blueprintImage.GetComponent<RectTransform>().sizeDelta = new Vector2(linkedStructureSO.structureSprite.rect.width * 2.8f, linkedStructureSO.structureSprite.rect.height * 2.8f);
    }

    public Vector3 GetBlueprintSize() {
        return GetComponent<RectTransform>().sizeDelta;
    }

    public void SetHovered(bool hovered) {
        if (isBeingMoved) return;
        if (CampEditManager.Instance.GetMovingBlueprint()) return;

        if(!this.hovered && hovered) {
            this.hovered = hovered;
            HoverFunction(hovered);

            OnBlueprintHovered?.Invoke(this, EventArgs.Empty);
        }

        if(this.hovered && !hovered) {
            this.hovered = hovered;
            HoverFunction(hovered);

            OnBlueprintUnhovered?.Invoke(this, EventArgs.Empty);
        }
    }

    public void HoverFunction(bool hover) {
        if (CampEditManager.Instance.GetMovingBlueprint()) return;
        if(linkedUIHoveredObjectAnimator != null) {
            if(hover) {
                linkedUIHoveredObjectAnimator.ResetTrigger("Hide");
                linkedUIHoveredObjectAnimator.SetTrigger("Show");
            } else {
                linkedUIHoveredObjectAnimator.ResetTrigger("Show");
                linkedUIHoveredObjectAnimator.SetTrigger("Hide");
            }
        }
    }

    public void SetMoving(bool moving) {
        isBeingMoved = moving;

        if (moving) {
            blueprintImage.material = selectedMaterial;
            OnStructureStartedMoving?.Invoke(this, EventArgs.Empty);
            if(!hovered) {
                OnBlueprintHovered?.Invoke(this, EventArgs.Empty);
                if (blueprintAnimator == null) return;
                blueprintAnimator.SetTrigger("PickUp");
                blueprintAnimator.ResetTrigger("Drop");
            }

        } else {
            blueprintImage.material = emptyMaterial;
            OnStructureStoppedMoving?.Invoke(this, EventArgs.Empty);

            OnBlueprintUnhovered?.Invoke(this, EventArgs.Empty);
            if (blueprintAnimator == null) return;
            blueprintAnimator.SetTrigger("Drop");
            blueprintAnimator.ResetTrigger("PickUp");
        }
    }

    public bool GetIsInitialBlueprint() {
        return isInitialBlueprint;
    }

}
