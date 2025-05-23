using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CampGridControllerNavigator : MonoBehaviour
{

    public static CampGridControllerNavigator Instance;
    [SerializeField] private GameObject StopNavigationSelectedButton;
    private bool navigatingCampGrid;
    private int currentIndex = 0;
    private int previousIndex = 0;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameInput.Instance.OnPlayerNavigateUIPerformed += GameInput_OnPlayerNavigateUIPerformed;
    }

    public void StartNavigation() {
        Debug.Log("StartNavigation");
        currentIndex = CampGrid.Instance.gridSize/2;
        navigatingCampGrid = true;
        HighlightCurrent();
    }

    public void StopNavigation() {
        Debug.Log("StopNavigation");
        navigatingCampGrid = false;
        EventSystem.current.SetSelectedGameObject(StopNavigationSelectedButton);
    }

    private void HighlightCurrent() {
        Debug.Log("HighlightCurrent " + currentIndex);
        Vector2Int gridPos = new Vector2Int(currentIndex, 0);

        GridVisualUnit gridUnit = CampGrid.Instance.GetGridVisualAt(gridPos);

        if (gridUnit.GetOccupyingStructure() == null) {
            EventSystem.current.SetSelectedGameObject(gridUnit.gameObject);
        }
    }

    private void GameInput_OnPlayerNavigateUIPerformed(object sender, System.EventArgs e) {
        Vector2 input = GameInput.Instance.GetUINavigationVector();
        if (input == Vector2.zero) return;

        int horizontal = Mathf.RoundToInt(input.x);
        int vertical = Mathf.RoundToInt(input.y);

        if (horizontal != 0) {
            if (!navigatingCampGrid) return;

            if (horizontal > 0) SetNextIndex(true);
            else SetNextIndex(false);

            HighlightCurrent();
        }

        if (vertical != 0) {
            if (vertical > 0) {
                if (!navigatingCampGrid) return;
                StopNavigation();
            }
            else {
                GameObject selected = EventSystem.current.currentSelectedGameObject;
                if(selected.GetComponent<GeneralEditionButtons>() != null) {
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

            if (occupyingStructure == null) {
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
                break;
            }

            justWarpedToCenter = false;
        }
    }






}
