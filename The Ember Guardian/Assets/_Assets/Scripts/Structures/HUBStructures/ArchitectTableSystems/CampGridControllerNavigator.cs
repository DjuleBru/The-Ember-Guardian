using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CampGridControllerNavigator : MonoBehaviour
{

    public static CampGridControllerNavigator Instance;
    [SerializeField] private GameObject StopNavigationSelectedButton;
    private HubMerchant architectTableHubMerchant;
    private bool navigatingCampGrid;
    private int currentIndex = 0;

    private bool isNavigating;
    private bool isHoldingNavigationStick = false;
    private float inputHoldTimer = 0f;
    private float inputHoldDelay = 0.3f;
    private float inputRepeatRate = 0.15f;
    private int navigationDirection = 0; // -1 = gauche, 1 = droite

    private void Awake() {
        Instance = this;
        architectTableHubMerchant = GetComponent<HubMerchant>();
    }

    private void Start() {
        GameInput.Instance.OnPlayerNavigateUIPerformed += GameInput_OnPlayerNavigateUIPerformed;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;
    }

    private void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        isNavigating = false;
    }

    private void Update() {
        if (!architectTableHubMerchant.GetPlayerInteractingWithMerchant()) return;
        if (!isNavigating) return;
        float horizontal = Input.GetAxis("Horizontal");

        if (Mathf.Abs(horizontal) > 0.5f) {
            int direction = horizontal > 0 ? 1 : -1;

            if (!isHoldingNavigationStick || direction != navigationDirection) {
                isHoldingNavigationStick = true;
                navigationDirection = direction;
                inputHoldTimer = inputHoldDelay;
                SetNextIndex(direction > 0);
            }
        }
        else {
            isHoldingNavigationStick = false;
            navigationDirection = 0;
            inputHoldTimer = 0f;
        }

        if (isHoldingNavigationStick) {
            inputHoldTimer -= Time.deltaTime;

            if (inputHoldTimer <= 0f) {
                inputHoldTimer = inputRepeatRate;
                SetNextIndex(navigationDirection > 0);
            }
        }
    }

    public void StartNavigation() {
        isNavigating = true;
        currentIndex = CampGrid.Instance.gridSize/2;
        navigatingCampGrid = true;
        Vector2Int gridPos = new Vector2Int(currentIndex, 0);

        GridVisualUnit gridUnit = CampGrid.Instance.GetGridVisualAt(gridPos);
        EventSystem.current.SetSelectedGameObject(gridUnit.gameObject);
    }

    public void StopNavigation() {
        isNavigating = false;
        navigatingCampGrid = false;
        StartCoroutine(SetStopNavigationSelectedButtonAfterDelay());
    }

    private IEnumerator SetStopNavigationSelectedButtonAfterDelay() {
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(StopNavigationSelectedButton);
        Debug.Log(StopNavigationSelectedButton);
    }

    private void HighlightCurrent() {
        Vector2Int gridPos = new Vector2Int(currentIndex, 0);

        GridVisualUnit gridUnit = CampGrid.Instance.GetGridVisualAt(gridPos);

        if (gridUnit.GetOccupyingStructure() == null) {
            EventSystem.current.SetSelectedGameObject(gridUnit.gameObject);
        }

        if(CampEditManager.Instance.GetMovingBlueprint()) {

            StructureBlueprint blueprintBeingMoved = CampEditManager.Instance.GetBlueprintBeingMoved();
            if (!CampGrid.Instance.CanPlaceAt(gridPos.x, blueprintBeingMoved.widthInCells, blueprintBeingMoved)) {
                return;
            }

            // Libère ancienne position
            CampGrid.Instance.SetOccupiedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);
            CampGrid.Instance.SetSelectedCells(blueprintBeingMoved.currentCell.x, blueprintBeingMoved.widthInCells, false);

            // Snap à la nouvelle position
            float snappedX = CampGrid.Instance.CellToWorld(gridPos.x);
            var rt = blueprintBeingMoved.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(snappedX, rt.anchoredPosition.y);

            // Met à jour la nouvelle cellule
            blueprintBeingMoved.currentCell = gridPos;

            // Réoccupe la nouvelle position
            CampGrid.Instance.SetOccupiedCells(gridPos.x, blueprintBeingMoved.widthInCells, true, blueprintBeingMoved);
            CampGrid.Instance.SetSelectedCells(gridPos.x, blueprintBeingMoved.widthInCells, true);
        }
    }

    private void GameInput_OnPlayerNavigateUIPerformed(object sender, System.EventArgs e) {
        if (!architectTableHubMerchant.GetPlayerInteractingWithMerchant()) return;
        Vector2 input = GameInput.Instance.GetUINavigationVector();
        if (input == Vector2.zero) return;

        int horizontal = Mathf.RoundToInt(input.x);
        int vertical = Mathf.RoundToInt(input.y);

        if (horizontal != 0) {
            if (!navigatingCampGrid) return;

            // Démarre juste la navigation, ne bouge pas maintenant
            isNavigating = true;
            navigationDirection = horizontal > 0 ? 1 : -1;
            inputHoldTimer = inputHoldDelay; // ou 0f si tu veux un mouvement instantané
        }

        if (vertical != 0) {
            if (vertical > 0) {
                if (!navigatingCampGrid) return;
                StopNavigation();
            }
            else {
                GameObject selected = EventSystem.current.currentSelectedGameObject;
                if (selected == null) {
                    StopNavigation();
                    return;
                };
                if (selected.GetComponent<GeneralEditionButtons>() != null) {
                    StartNavigation();
                }
            }
        }
    }

    private void SetNextIndex(bool increase) {
        int maxIndex = CampGrid.Instance.gridSize;
        int minIndex = 0;
        bool justWarpedToCenter = false;

        while (true) {
            if(!justWarpedToCenter) {
                if (increase) {
                    currentIndex++;
                    if (currentIndex > maxIndex) break;
                }
                else {
                    currentIndex--;
                    if (currentIndex < minIndex) break;
                }
            }

            Vector2Int gridPos = new Vector2Int(currentIndex, 0);
            GridVisualUnit gridUnit = CampGrid.Instance.GetGridVisualAt(gridPos);
            StructureBlueprint occupyingStructure = gridUnit.GetOccupyingStructure();

            if (occupyingStructure == null || occupyingStructure.widthInCells == 1 || occupyingStructure == CampEditManager.Instance.GetBlueprintBeingMoved()) {
                HighlightCurrent();
                EventSystem.current.SetSelectedGameObject(gridUnit.gameObject);
                break;
            }

            int minCell = occupyingStructure.currentCell.x;
            int maxCell = minCell + occupyingStructure.widthInCells - 1;
            int middleCell = minCell + occupyingStructure.widthInCells / 2;

            // Si on n’est pas encore au centre de la structure, on y saute
            if (currentIndex != middleCell && !justWarpedToCenter) {
                currentIndex = middleCell;
                justWarpedToCenter = true;
                continue;
            }

            // Si on vient de sauter au centre, on sélectionne cette cellule
            if (justWarpedToCenter) {

                if (increase) {
                    currentIndex = maxCell;
                }
                else {
                    currentIndex = minCell;
                }

                EventSystem.current.SetSelectedGameObject(gridUnit.gameObject);
                HighlightCurrent();
                break;
            }

            justWarpedToCenter = false;
        }
    }

}
