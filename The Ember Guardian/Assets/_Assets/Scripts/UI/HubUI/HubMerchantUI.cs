using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HubMerchantUI : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Scrollbar scrollBar;
    [SerializeField] private TextMeshProUGUI merchantNameText;
    [SerializeField] private TextMeshProUGUI redGemAmount;
    [SerializeField] private TextMeshProUGUI greenGemAmount;
    [SerializeField] private GameObject firstButtonSelected;

    [SerializeField] private bool hasScrollView;

    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private Animator hubMerchantUInimator;
    private HubMerchant hubMerchant;

    private void Awake() {
        hubMerchant = GetComponentInParent<HubMerchant>();
        hubMerchantUInimator = GetComponent<Animator>();
        canvas = GetComponent<Canvas>();
        raycaster = canvas.GetComponent<GraphicRaycaster>();

        mainPanel.SetActive(false);
    }

    private void Start() {
        GameInput.Instance.OnPlayerInputChanged += GameInput_OnPlayerInputChanged;
        UICurrencyManager.Instance.OnCurrencyCollected += UICurrencyManager_OnCurrencyCollected;
        UICurrencyManager.Instance.OnCurrencyDropped += UICurrencymanager_OnCurrencyDropped;
        hubMerchant.OnPlayerInteractedWithHubMerchant += HubMerchant_OnPlayerInteractedWithHubMerchant;
        hubMerchant.OnPlayerStoppedInteractingWithHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithHubMerchant;


        canvas.worldCamera = CameraManager.Instance.GetUICamera();
        canvas.sortingLayerName = "UI";
        merchantNameText.text = hubMerchant.GetHubMerchantName();
        RefreshPlayerGems();
    }

    private void GameInput_OnPlayerInputChanged(object sender, System.EventArgs e) {
        RefreshGameInputView();

        if(GameInput.Instance.IsUsingGamepad()) {
            raycaster.enabled = false;
        } else {
            raycaster.enabled = true;
        }
    }

    private void UICurrencymanager_OnCurrencyDropped(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        RefreshPlayerGems();
    }

    private void UICurrencyManager_OnCurrencyCollected(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        RefreshPlayerGems();
    }

    private void RefreshPlayerGems() {
        greenGemAmount.text = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.greenGem).Count.ToString();
        redGemAmount.text = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.redGem).Count.ToString();
    }

    private void RefreshGameInputView() {
        if(hasScrollView) {
            if (!GameInput.Instance.IsUsingGamepad()) {
                scrollRect.horizontalScrollbar = scrollBar;
                scrollBar.gameObject.SetActive(true);
            }
            else {
                scrollRect.horizontalScrollbar = null;
                scrollBar.gameObject.SetActive(false);
            }
        }
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithHubMerchant(object sender, System.EventArgs e) {
        hubMerchantUInimator.SetTrigger("Hide");

        EventSystem.current.SetSelectedGameObject(null);
    }

    private void HubMerchant_OnPlayerInteractedWithHubMerchant(object sender, System.EventArgs e) {
        RefreshGameInputView();

        hubMerchantUInimator.SetTrigger("Show");
        EventSystem.current.SetSelectedGameObject(firstButtonSelected);
    }

}
