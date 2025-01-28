using System;
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
    [SerializeField] private TextMeshProUGUI blueGemAmount;
    [SerializeField] private TextMeshProUGUI yellowGemAmount;
    [SerializeField] private TextMeshProUGUI purpleGemAmount;
    [SerializeField] private GameObject firstButtonSelected;

    [SerializeField] private bool hasScrollView;

    private Canvas canvas;
    private GraphicRaycaster raycaster;
    private Animator hubMerchantUInimator;
    private HubMerchant hubMerchant;

    public static event EventHandler OnAnyHubMerchantOpenUIPanel;
    public static event EventHandler OnAnyHubMerchantCloseUIPanel;

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
        hubMerchant.OnPlayerOpenedHubMerchantShop += HubMerchant_OnPlayerInteractedWithHubMerchant;
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
        blueGemAmount.text = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.blueGem).Count.ToString();
        yellowGemAmount.text = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.yellowGem).Count.ToString();
        purpleGemAmount.text = UICurrencyManager.Instance.GetCurrenciesInBagOfType(PlayerCurrencies.CurrencyType.purpleGem).Count.ToString();
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
        hubMerchantUInimator.ResetTrigger("Show");
        hubMerchantUInimator.SetTrigger("Hide");

        EventSystem.current.SetSelectedGameObject(null);

        OnAnyHubMerchantCloseUIPanel?.Invoke(this, EventArgs.Empty);
    }

    private void HubMerchant_OnPlayerInteractedWithHubMerchant(object sender, System.EventArgs e) {
        RefreshGameInputView();

        hubMerchantUInimator.SetTrigger("Show");
        hubMerchantUInimator.ResetTrigger("Hide");
        EventSystem.current.SetSelectedGameObject(firstButtonSelected);

        OnAnyHubMerchantOpenUIPanel?.Invoke(this, EventArgs.Empty);
    }

    public HubMerchant GetHubMerchant() {
        return hubMerchant;
    }

    private void OnDestroy() {

        GameInput.Instance.OnPlayerInputChanged -= GameInput_OnPlayerInputChanged;
        UICurrencyManager.Instance.OnCurrencyCollected -= UICurrencyManager_OnCurrencyCollected;
        UICurrencyManager.Instance.OnCurrencyDropped -= UICurrencymanager_OnCurrencyDropped;
    }

}
