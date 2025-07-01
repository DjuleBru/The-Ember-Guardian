using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChangeDogPanel : MonoBehaviour
{
    public static ChangeDogPanel Instance;

    private bool panelOpen;

    [SerializeField] private Transform changeDogSlotContainer;
    [SerializeField] private Transform changeDogSlotTemplate;
    [SerializeField] private Transform emptyDogSlotTemplate;
    private List<GameObject> changeDogButtons;

    public event EventHandler OnChangeDogPanelOpened;
    public event EventHandler OnChangeDogPanelClosed;

    private void Awake() {
        Instance = this;
    }
    private void Start() {
        PlayerTabMenuUI.Instance.OnPlayerTabOpened += PlayerTabMenuUI_OnPlayerTabOpened;
        GameInput.Instance.OnPlayerBackPerformed += GameInput_OnPlayerBackPerformed;

        DogStats.Instance.OnNewDogUnlocked += DogState_OnNewDogUnlocked;
        Dog.Instance.OnDogTypeChanged += Dog_OnDogTypeChanged;

        UpdateDogSlots(Dog.Instance.GetDogType());

        gameObject.SetActive(false);
    }

    private void UpdateDogSlots(Dog.DogType activeDogType) {
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
    }

    private void Dog_OnDogTypeChanged(object sender, EventArgs e) {
        UpdateDogSlots(Dog.Instance.GetDogType());
    }

    private void DogState_OnNewDogUnlocked(object sender, EventArgs e) {
        UpdateDogSlots(Dog.Instance.GetDogType());
    }

    private void PlayerTabMenuUI_OnPlayerTabOpened(object sender, System.EventArgs e) {
        panelOpen = false;
        gameObject.SetActive(false);
        OnChangeDogPanelClosed?.Invoke(this, EventArgs.Empty);
    }
    private void GameInput_OnPlayerBackPerformed(object sender, System.EventArgs e) {
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
                EventSystem.current.SetSelectedGameObject(changeDogButtons[0]);
                OnChangeDogPanelOpened?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}
