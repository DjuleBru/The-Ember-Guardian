using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StructureBlueprint : MonoBehaviour {

    [SerializeField] private StructureSO linkedStructureSO;
    [SerializeField] private Image blueprintImage;

    private Animator uiAnimator;
    public int widthInCells = 3;
    public bool isBeingMoved = false;
    public Vector2Int currentCell;
    public List<Vector2Int> occupiedCells = new List<Vector2Int>();

    private bool hovered;

    private void Awake() {
        Debug.Log("cac");
        uiAnimator = GetComponent<Animator>();
        Debug.Log(uiAnimator);
    }

    void Start() { 
        // Calcul de la cellule à partir de la position actuelle
        SetOccupiedCells();
        CampEditManager.Instance.RegisterStructure(this);

        if(linkedStructureSO != null) {
            blueprintImage.sprite = linkedStructureSO.structureSprite;
            SetBlueprintSize();
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
        }
    }

    public StructureSO GetLinkedStructureSO() {
        return linkedStructureSO;
    }

    public void SetStructureSO(StructureSO structureSO) {
        linkedStructureSO = structureSO;
        blueprintImage.sprite = linkedStructureSO.structureSprite;
    }

    public void SetBlueprintSize() {
        GetComponent<RectTransform>().sizeDelta = new Vector2(widthInCells * CampGrid.Instance.GetCellSize(), 300);
        blueprintImage.GetComponent<RectTransform>().sizeDelta = new Vector2(linkedStructureSO.structureSprite.rect.width * 2.8f, linkedStructureSO.structureSprite.rect.height * 2.8f);
    }

    public void SetHovered(bool hovered) {
        if(!this.hovered && hovered) {
            this.hovered = hovered;

            if (uiAnimator == null) return;
            uiAnimator.ResetTrigger("Hide");
            uiAnimator.SetTrigger("Show");
        }

        if(this.hovered && !hovered) {
            this.hovered = hovered;

            if (uiAnimator == null) return;
            uiAnimator.ResetTrigger("Show");
            uiAnimator.SetTrigger("Hide");
        }
    }

}
