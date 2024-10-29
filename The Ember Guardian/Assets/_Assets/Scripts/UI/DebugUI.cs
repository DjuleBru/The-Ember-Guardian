using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI orbAmountText;

    private void Start() {
        PlayerCurrencies.Instance.OnBlueOrbChanged += PlayerCurrencies_OnOrbChanged;
        orbAmountText.text = PlayerCurrencies.Instance.GetCurrencyAmount(PlayerCurrencies.CurrencyType.blueOrb).ToString();
    }

    private void PlayerCurrencies_OnOrbChanged(object sender, PlayerCurrencies.OnCurrencyChangedEventArgs e) {
        orbAmountText.text = e.newAmount.ToString();
    }
}
