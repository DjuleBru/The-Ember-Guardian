using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeDogPanel : MonoBehaviour
{
    public static ChangeDogPanel Instance;

    protected bool panelOpen;

    [SerializeField] protected Transform changeDogSlotContainer;
    [SerializeField] protected Transform changeDogSlotTemplate;
    [SerializeField] protected Transform emptyDogSlotTemplate;
    protected List<GameObject> changeDogButtons;

    public event EventHandler OnChangeDogPanelOpened;
    public event EventHandler OnChangeDogPanelClosed;

    protected void Awake() {
        Instance = this;
    }
    protected virtual void Start() {
        PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenuUI_OnPlayerTabOpened;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;

        DogStats.Instance.OnNewDogUnlocked += DogState_OnNewDogUnlocked;
        Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;

        UpdateDogSlots(Dog.Instance.GetDogType());

        gameObject.SetActive(false);
    }

    protected virtual void UpdateDogSlots(Dog.DogType activeDogType) {
        changeDogSlotTemplate.gameObject.SetActive(true);
        emptyDogSlotTemplate.gameObject.SetActive(true);
        changeDogButtons = new List<GameObject>();

        foreach (Transform child in changeDogSlotContainer) {
            if (child == changeDogSlotTemplate || child == emptyDogSlotTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (Dog.DogType type in Enum.GetValues(typeof(Dog.DogType))) {
            if (type == activeDogType) continue;

            if(DogStats.Instance.GetDogUnlocked(type)) {
                DogReplaceButton dogReplaceButton = Instantiate(changeDogSlotTemplate, changeDogSlotContainer).GetComponent<DogReplaceButton>();

                dogReplaceButton.SetLinkedDog(type);
                changeDogButtons.Add(dogReplaceButton.gameObject);
            } else {
                Instantiate(emptyDogSlotTemplate, changeDogSlotContainer);
            }

        }

        changeDogSlotTemplate.gameObject.SetActive(false);
        emptyDogSlotTemplate.gameObject.SetActive(false);

        SetupNavigation();
    }

    protected void SetupNavigation() {
        if (changeDogButtons == null || changeDogButtons.Count == 0)
            return;

        for (int i = 0; i < changeDogButtons.Count; i++) {
            GameObject go = changeDogButtons[i];
            var selectable = go.GetComponent<Selectable>();
            if (selectable == null) continue;

            Navigation nav = new Navigation {
                mode = Navigation.Mode.Explicit
            };

            // Up
            if (i == 0)
                nav.selectOnUp = changeDogButtons[changeDogButtons.Count - 1].GetComponent<Selectable>(); // wrap
            else
                nav.selectOnUp = changeDogButtons[i - 1].GetComponent<Selectable>();

            // Down
            if (i == changeDogButtons.Count - 1)
                nav.selectOnDown = changeDogButtons[0].GetComponent<Selectable>(); // wrap
            else
                nav.selectOnDown = changeDogButtons[i + 1].GetComponent<Selectable>();

            selectable.navigation = nav;
        }
    }

    protected void Dog_OnDogTypeChanged(object sender, EventArgs e) {
        UpdateDogSlots(Dog.Instance.GetDogType());
    }

    protected void DogState_OnNewDogUnlocked(object sender, EventArgs e) {
        UpdateDogSlots(Dog.Instance.GetDogType());
    }

    protected void PlayerTabMenuUI_OnPlayerTabOpened(object sender, System.EventArgs e) {
        panelOpen = false;
        gameObject.SetActive(false);
        OnChangeDogPanelClosed?.Invoke(this, EventArgs.Empty);
    }

    protected void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
        if (panelOpen) {
            OpenClosePanel();
        }
    }

    public void OpenClosePanel() {
        panelOpen = !panelOpen;
        gameObject.SetActive(panelOpen);

        if (!panelOpen) {
            OnChangeDogPanelClosed?.Invoke(this, EventArgs.Empty);
        }
        else {
            if (changeDogButtons.Count > 0) {
                if(GameInput.Instance.IsUsingGamepad()) {
                    EventSystem.current.SetSelectedGameObject(changeDogButtons[0]);
                }

                OnChangeDogPanelOpened?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}
