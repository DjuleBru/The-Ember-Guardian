using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TradeGemsUI : MonoBehaviour {

    [SerializeField] private Button inputGemButton;
    [SerializeField] private Button outputGemButton;
    [SerializeField] private Button tradeButton;
    [SerializeField] private ItemButtonUI_Visual tradeButtonVisual;

    [SerializeField] private Image inputGemImage;
    [SerializeField] private Image outputGemImage;

    [SerializeField] private Sprite greenGemSprite;
    [SerializeField] private Sprite redGemSprite;
    [SerializeField] private Sprite yellowGemSprite;
    [SerializeField] private Sprite blueGemSprite;
    [SerializeField] private Sprite purpleGemSprite;
    [SerializeField] private Sprite cyanGemSprite;
    [SerializeField] private CanvasGroup tradeButtonCanvasGroup;

    [SerializeField] private Color notEnoughGemsTextColor;
    [SerializeField] private TextMeshProUGUI inputTextGems;
    [SerializeField] private int inputGemAmount = 5;
    [SerializeField] private int outputGemAmount = 1;

    private bool canTrade;

    private List<PlayerCurrencies.CurrencyType> gemTypes = new List<PlayerCurrencies.CurrencyType>();
    private PlayerCurrencies.CurrencyType currentInputGemType;
    private PlayerCurrencies.CurrencyType currentOutputGemType;

    public static event EventHandler OnAnyGemTypeChanged;
    private Coroutine ResetTradeButtonCoroutine;

    private void Start() {
        gemTypes = CurrenciesManager.Instance.GetCurrencyTypesInCategory(PlayerCurrencies.CurrencyCategory.gem);
        UICurrencyManager.HubInventoryUI.OnCurrencyRemovedFromBag += HubInventoryUI_OnCurrencyRemovedFromBag;

        inputGemButton.onClick.AddListener(OnClickInputGem);
        outputGemButton.onClick.AddListener(OnClickOutputGem);
        tradeButton.onClick.AddListener(TradeGems);

        InitSelection();
        RefreshUI();
    }

    private void HubInventoryUI_OnCurrencyRemovedFromBag(object sender, UICurrencyManager.OnCurrencyDroppedEventArgs e) {
        RefreshUI();
    }

    private void TradeGems() {
        if (!canTrade) return;

        UICurrencyManager.HubInventoryUI.RemoveCurrencyFromBag(currentInputGemType, inputGemAmount);
        UICurrencyManager.HubInventoryUI.AddCurrencyAmount(currentOutputGemType, outputGemAmount);

        int red = 0;
        int blue = 0;
        int purple = 0;
        int cyan = 0;
        int green = 0;
        int yellow = 0;

        if(currentOutputGemType == PlayerCurrencies.CurrencyType.blueGem) {
            blue = 5;
        }
        if (currentOutputGemType == PlayerCurrencies.CurrencyType.redGem) {
            red = 5;
        }
        if (currentOutputGemType == PlayerCurrencies.CurrencyType.yellowGem) {
            yellow = 5;
        }
        if (currentOutputGemType == PlayerCurrencies.CurrencyType.purpleGem) {
            purple = 5;
        }
        if (currentOutputGemType == PlayerCurrencies.CurrencyType.greenGem) {
            green = 5;
        }
        if (currentOutputGemType == PlayerCurrencies.CurrencyType.cyanGem) {
            cyan = 5;
        }

        tradeButtonVisual.BuyAnimationInstant(red, green, blue, yellow, purple, cyan);

        if(ResetTradeButtonCoroutine != null) {
            StopCoroutine(ResetTradeButtonAfterDelay());
        }

        ResetTradeButtonCoroutine = StartCoroutine(ResetTradeButtonAfterDelay());
    }

    private IEnumerator ResetTradeButtonAfterDelay() {
        yield return new WaitForSeconds(1f);
        tradeButtonVisual.ResetAnimator();
    }

    private void InitSelection() {
        if (gemTypes.Count < 2) {
            Debug.LogWarning("Not enough gem types to trade.");
            return;
        }

        currentInputGemType = PlayerCurrencies.CurrencyType.greenGem;
        currentOutputGemType = PlayerCurrencies.CurrencyType.redGem;
    }

    private void OnClickInputGem() {
        currentInputGemType = GetNextGem(currentInputGemType, currentOutputGemType);
        RefreshUI();

        OnAnyGemTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnClickOutputGem() {
        currentOutputGemType = GetNextGem(currentOutputGemType, currentInputGemType);
        RefreshUI();

        OnAnyGemTypeChanged?.Invoke(this, EventArgs.Empty);
    }

    private PlayerCurrencies.CurrencyType GetNextGem(PlayerCurrencies.CurrencyType current, PlayerCurrencies.CurrencyType excluded) {

        int index = gemTypes.IndexOf(current);

        for (int i = 1; i < gemTypes.Count; i++) {
            int nextIndex = (index + i) % gemTypes.Count;

            if (gemTypes[nextIndex] != excluded) {
                return gemTypes[nextIndex];
            }
        }

        return current;
    }

    private void RefreshUI() {
        inputGemImage.sprite = GetSpriteFromType(currentInputGemType);
        outputGemImage.sprite = GetSpriteFromType(currentOutputGemType);

        RefreshCanTrade();
    }

    private void RefreshCanTrade() {
        inputTextGems.text = inputGemAmount.ToString();

        bool wasSelected = false;

        if (EventSystem.current != null) {
            if (EventSystem.current.currentSelectedGameObject == tradeButton.gameObject) {
                wasSelected = true;
            }
        }


        if (HasEnoughGemsOfType(currentInputGemType)) {
            canTrade = true;
            inputTextGems.color = Color.white;
            tradeButton.enabled = true;
            tradeButtonCanvasGroup.alpha = 1f;
        }
        else {
            canTrade = false;
            inputTextGems.color = notEnoughGemsTextColor;
            tradeButton.enabled = false;
            tradeButtonCanvasGroup.alpha = .5f;

            if (wasSelected) {
                SelectInputButton();
            }
        }
    }
    private void SelectInputButton() {
        if (EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(inputGemButton.gameObject);
    }

    private bool HasEnoughGemsOfType(PlayerCurrencies.CurrencyType gemType) {
        return UICurrencyManager.HubInventoryUI.GetCurrenciesInBagOfType(gemType).Count >= inputGemAmount;
    }

    private Sprite GetSpriteFromType(PlayerCurrencies.CurrencyType type) {
        if (type == PlayerCurrencies.CurrencyType.greenGem) {
            return greenGemSprite;
        }

        if (type == PlayerCurrencies.CurrencyType.redGem) {
            return redGemSprite;
        }

        if (type == PlayerCurrencies.CurrencyType.yellowGem) {
            return yellowGemSprite;
        }

        if (type == PlayerCurrencies.CurrencyType.blueGem) {
            return blueGemSprite;
        }

        if (type == PlayerCurrencies.CurrencyType.purpleGem) {
            return purpleGemSprite;
        }

        if (type == PlayerCurrencies.CurrencyType.cyanGem) {
            return cyanGemSprite;
        }

        return null;
    }
}
