using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyUI_AlmostFullColliders : MonoBehaviour
{
    private List<Currency_UI> currenciesInTriggerCollider = new List<Currency_UI>();

    private void OnTriggerEnter2D(Collider2D collision) {
        Currency_UI currencyUI = collision.gameObject.GetComponent<Currency_UI>();
        if (currencyUI != null) {
            currenciesInTriggerCollider.Add(currencyUI);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Currency_UI currencyUI = collision.gameObject.GetComponent<Currency_UI>();
        if (currencyUI != null) {
            currenciesInTriggerCollider.Remove(currencyUI);
        }
    }

    public bool GetBagIsAlmostFull() {

        return currenciesInTriggerCollider.Count >= 2;
    }
}
