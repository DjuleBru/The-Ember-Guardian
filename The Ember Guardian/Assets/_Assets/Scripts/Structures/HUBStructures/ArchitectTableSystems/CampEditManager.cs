using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampEditManager : MonoBehaviour {
    public static CampEditManager Instance;

    [SerializeField] private Canvas canvas;
    private CampGrid campGrid;
    private Vector3 movingStructureInitialPosition;
    private int[] previousOccupiedCells;
    private StructureBlueprint blueprintBeingMoved;
    private bool movingBlueprint;

    void Awake() {
        Instance = this;
        campGrid = GetComponent<CampGrid>();
    }

    private void Start() {
        GameInput.Instance.OnEditCampDeselect += GameInput_OnEditCampDeselect;
        GameInput.Instance.OnEditCampSelect += GameInput_OnEditCampSelect;
    }

    void Update() {
        if (!movingBlueprint ||blueprintBeingMoved == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            blueprintBeingMoved.transform.parent as RectTransform,
            Input.mousePosition, canvas.worldCamera, out Vector2 localPoint);

        int targetCell = campGrid.WorldToCell(localPoint.x);
        int width = blueprintBeingMoved.widthInCells;

        if (campGrid.CanPlaceAt(targetCell, width, blueprintBeingMoved)) {
            float snappedX = campGrid.CellToWorld(targetCell);
            var rt = blueprintBeingMoved.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(snappedX, rt.anchoredPosition.y);
            blueprintBeingMoved.currentCell = targetCell;
        }
    }

    public void StartMoving(StructureBlueprint structure) {
        blueprintBeingMoved = structure;
        movingStructureInitialPosition = blueprintBeingMoved.GetComponent<RectTransform>().position;
        movingBlueprint = true;

        // Libère les cellules précédemment occupées
        campGrid.SetOccupiedCells(blueprintBeingMoved.currentCell, blueprintBeingMoved.widthInCells, false);
    }

    private void CancelPlacement() {
        blueprintBeingMoved.GetComponent<RectTransform>().position = movingStructureInitialPosition;
        campGrid.SetOccupiedCells(blueprintBeingMoved.currentCell, blueprintBeingMoved.widthInCells, true);
        blueprintBeingMoved.isBeingMoved = false;
        blueprintBeingMoved = null;
        movingBlueprint = false;
    }

    private IEnumerator StopMoving() {
        int newCell = blueprintBeingMoved.currentCell;
        campGrid.SetOccupiedCells(newCell, blueprintBeingMoved.widthInCells, true);

        movingBlueprint = false;
        yield return new WaitForSeconds(.1f);

        blueprintBeingMoved.isBeingMoved = false;
        blueprintBeingMoved = null;
    }

    public StructureBlueprint GetBlueprintBeingMoved() => blueprintBeingMoved;
    public bool GetMovingBlueprint() => movingBlueprint;

    private void GameInput_OnEditCampSelect(object sender, System.EventArgs e) {
        if (!movingBlueprint) return;

        if (campGrid.CanPlaceAt(blueprintBeingMoved.currentCell, blueprintBeingMoved.widthInCells, blueprintBeingMoved)) {
            StartCoroutine(StopMoving());
        }
    }

    private void GameInput_OnEditCampDeselect(object sender, System.EventArgs e) {
        if (!movingBlueprint) return;
        CancelPlacement();
    }
}
