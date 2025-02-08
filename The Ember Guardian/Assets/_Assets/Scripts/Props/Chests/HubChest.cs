using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubChest : MonoBehaviour
{
    public static HubChest Instance;

    private bool playerInTriggerArea;
    private bool chestOpen;

    private PayCurrencyUI payCurrencyUI;
    private PlayerCurrencies.CurrencyType currentGemType;
    private List<PlayerCurrencies.CurrencyType> allGemTypesList = new List<PlayerCurrencies.CurrencyType>();
    private int gemTypeIndex;

    public event EventHandler OnChestOpened;
    public event EventHandler OnChestClosed;

    [SerializeField] protected PayCurrencyTemplateWorldUI payGemTemplate;
    protected List<PayCurrencyTemplateWorldUI> payCurrencyTemplates = new List<PayCurrencyTemplateWorldUI>();

    private void Awake() {
        Instance = this;
        payCurrencyUI = GetComponent<PayCurrencyUI>();
    }

    private void Start() {
        payCurrencyUI.OnCurrencyPaymentSuccess += PayCurrencyUI_OnCurrencyPaymentSuccess;
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;
        GameInput.Instance.OnPlayerInteractCanceled += GameInput_OnPlayerInteractCanceled;

        payGemTemplate.SetCurrencyTypeToPay(PlayerCurrencies.CurrencyType.greenGem);
        payCurrencyTemplates.Add(payGemTemplate);
        payCurrencyUI.SetOrbTemplateUIList(payCurrencyTemplates);

        allGemTypesList.Add(PlayerCurrencies.CurrencyType.greenGem);
        allGemTypesList.Add(PlayerCurrencies.CurrencyType.redGem);
        allGemTypesList.Add(PlayerCurrencies.CurrencyType.blueGem);
        allGemTypesList.Add(PlayerCurrencies.CurrencyType.yellowGem);
        allGemTypesList.Add(PlayerCurrencies.CurrencyType.purpleGem);
        currentGemType = allGemTypesList[0];
    }

    private void PayCurrencyUI_OnCurrencyPaymentSuccess(object sender, EventArgs e) {
        UICurrencyManager.HubInventoryUI.AddCurrencyAmount(currentGemType, 1);

        if (!PayNextGem()) {
            CloseChest(); // Ferme le coffre si plus de gemmes
        }
        else {
            payCurrencyUI.SetPlayerInteractingContinuous(); // Continue l'interaction
        }
    }

    protected void GameInput_OnPlayerInteractCanceled(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        payCurrencyUI.SetPlayerInteracting(false);
        payCurrencyUI.ResetCurrencyPayment();
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, EventArgs e) {
        if (!playerInTriggerArea) return;

        PayNextGem();
        payCurrencyUI.SetPlayerInteracting(true);
    }

    private bool PayNextGem() {
        // Tant qu'il reste des gemmes, on continue à en retirer
        for (int i = 0; i < allGemTypesList.Count; i++) {
            PlayerCurrencies.CurrencyType gemType = allGemTypesList[i];

            // Vérifie si le joueur a au moins une gemme de ce type
            if (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(gemType).Count > 0) {
                currentGemType = gemType;
                payGemTemplate.SetCurrencyTypeToPay(currentGemType);
                return true; // Continue à payer
            }
        }

        return false; // Plus aucune gemme à payer, on arrête
    }

    private bool HasGemsToPay() {
        // Tant qu'il reste des gemmes, on continue à en retirer
        for (int i = 0; i < allGemTypesList.Count; i++) {
            PlayerCurrencies.CurrencyType gemType = allGemTypesList[i];

            // Vérifie si le joueur a au moins une gemme de ce type
            if (UICurrencyManager.PlayerInventoryUI.GetCurrenciesInBagOfType(gemType).Count > 0) {
                return true; // Continue à payer
            }
        }

        return false; // Plus aucune gemme à payer, on arrête
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Player>() == null) return;
        if (!HasGemsToPay()) return;

        playerInTriggerArea = true;
        chestOpen = true;
        OpenChest();
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.GetComponent<Player>() == null) return;
        if (!chestOpen) return;

        playerInTriggerArea = false;
        CloseChest();
    }

    private void OpenChest() {
        OnChestOpened?.Invoke(this, EventArgs.Empty);
    }

    private void CloseChest() {
        chestOpen = false;
        gemTypeIndex = 0;
        currentGemType = allGemTypesList[0];
        OnChestClosed?.Invoke(this, EventArgs.Empty);
    }


    public bool GetPlayerInTriggerArea() {
        return playerInTriggerArea;
    }

    private void OnDestroy() {
        payCurrencyUI.OnCurrencyPaymentSuccess -= PayCurrencyUI_OnCurrencyPaymentSuccess;
        GameInput.Instance.OnPlayerInteractPerformed -= GameInput_OnPlayerInteractPerformed;
        GameInput.Instance.OnPlayerInteractCanceled -= GameInput_OnPlayerInteractCanceled;
    }

}
