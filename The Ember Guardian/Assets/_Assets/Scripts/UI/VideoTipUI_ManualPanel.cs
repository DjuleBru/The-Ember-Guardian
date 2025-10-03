using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VideoTipUI_ManualPanel : MonoBehaviour
{

    public static VideoTipUI_ManualPanel Instance;

    [SerializeField] private Button backToPauseMenuButton;
    [SerializeField] private Button replayTipButton;
    [SerializeField] private Scrollbar scrollBar;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private Transform buttonTemplate;
    [SerializeField] private List<VideoTipSO> allVideoTipsList;
    private Button lastSelectedTipButton;

    private void Awake() {
        Instance = this;

        backToPauseMenuButton.onClick.AddListener(() => {
            VideoTipUI.Instance.ClosePanel();
            PauseMenuUI.Instance.ShowPauseMenu(true);
        });
    }

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        RefreshScrollBarVisibility();
    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        RefreshScrollBarVisibility();
    }

    private void RefreshScrollBarVisibility() {
        if (!GameInput.Instance.IsUsingGamepad()) {
            scrollRect.verticalScrollbar = scrollBar;
            scrollBar.gameObject.SetActive(true);
        }
        else {
            scrollRect.verticalScrollbar = null;
            scrollBar.gameObject.SetActive(false);
        }
    }

    public void RefreshTipList() {
        foreach(Transform child in buttonContainer) {
            if (child == buttonTemplate) continue;
            Destroy(child.gameObject);
        }

        buttonTemplate.gameObject.SetActive(true);

        List<VideoTipUI_TipTemplate> tipButtons = new List<VideoTipUI_TipTemplate>();
        int i = 0;

        List<VideoTipSO> orderedTipSOList = new List<VideoTipSO>();

        foreach (string key in MetaProgressionManager.Instance.GetTipUnlockedList()) {
            foreach (VideoTipSO tip in allVideoTipsList) {
                if(tip.tipNameLocalizationKey == key) {
                    orderedTipSOList.Add(tip);
                }
            }
        }
        
        foreach (VideoTipSO tip in orderedTipSOList) {
            if (!MetaProgressionManager.Instance.GetTipUnlocked(tip)) continue;

            VideoTipUI_TipTemplate tipTemplate = Instantiate(buttonTemplate, buttonContainer).GetComponent<VideoTipUI_TipTemplate>();
            tipTemplate.SetVideoTipSO(tip);
            tipButtons.Add(tipTemplate);

            if (i == 0) {
                EventSystem.current.SetSelectedGameObject(tipTemplate.gameObject);
                VideoTipUI.Instance.PlayTipSO(tip, 0, false, true);
                lastSelectedTipButton = tipTemplate.GetComponent<Button>();
            }
            i++;
        }

        buttonTemplate.gameObject.SetActive(false);


        for (int j = 0; j < tipButtons.Count; j++) {
            Button btn = tipButtons[j].GetComponent<Button>();
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            // Haut : si premier bouton ; backToPauseMenuButton, sinon bouton précédent
            if (j == 0) nav.selectOnUp = backToPauseMenuButton;
            else nav.selectOnUp = tipButtons[j - 1].GetComponent<Button>();

            // Bas : si dernier bouton ; null ou boucle vers le premier
            if (j == tipButtons.Count - 1) nav.selectOnDown = null;
            else nav.selectOnDown = tipButtons[j + 1].GetComponent<Button>();

            // Gauche : null (ou si tu veux un bouton spécifique)
            nav.selectOnLeft = null;

            // Droite : ReplayTip
            nav.selectOnRight = replayTipButton;

            btn.navigation = nav;
        }

        Navigation backNav = backToPauseMenuButton.navigation;
        backNav.mode = Navigation.Mode.Explicit;
        backNav.selectOnDown = tipButtons.Count > 0 ? tipButtons[0].GetComponent<Button>() : null;
        backToPauseMenuButton.navigation = backNav;

        // Optionnel : ReplayTipButton peut remonter vers le bouton actuellement sélectionné
        Navigation replayNav = replayTipButton.navigation;
        replayNav.mode = Navigation.Mode.Explicit;
        replayNav.selectOnLeft = lastSelectedTipButton; //  maintenant ça revient au dernier tip sélectionné
        replayTipButton.navigation = replayNav;

    }

    public void SetLastSelectedTipButton(Button btn) {
        lastSelectedTipButton = btn;

        Navigation replayNav = replayTipButton.navigation;
        replayNav.mode = Navigation.Mode.Explicit;
        replayNav.selectOnLeft = lastSelectedTipButton; //  maintenant ça revient au dernier tip sélectionné
        replayTipButton.navigation = replayNav;
    }

    private void OnDestroy() {
        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
    }
}
