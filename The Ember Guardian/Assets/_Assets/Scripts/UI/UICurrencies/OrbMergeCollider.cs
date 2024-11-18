using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbMergeCollider : MonoBehaviour
{
    private List<Currency_UI> smallOrbs = new List<Currency_UI>();

    private void OnTriggerEnter2D(Collider2D collision) {

        CurrencyUI_DetectionCollider OrbCollider = collision.GetComponent<CurrencyUI_DetectionCollider>();

        if (OrbCollider != null) {

            Currency_UI currencyUI = OrbCollider.GetComponentInParent<Currency_UI>();

            if (currencyUI == null) return;

                if(currencyUI.GetCurrencyType() == PlayerCurrencies.CurrencyType.smallBlueOrb) {

                    if (currencyUI.GetMoving()) {
                        if (!smallOrbs.Contains(currencyUI)) {
                            smallOrbs.Add(currencyUI);
                        }
                    }

                    if (smallOrbs.Count == UICurrencyManager.Instance.GetSmallOrbValue()) {
                        smallOrbs.Clear();

                        //UICurrencyManager.Instance.MergeSmallOrbs();
                    }

                }
        } 
    }

    private void OnTriggerExit2D(Collider2D collision) {
        CurrencyUI_DetectionCollider OrbCollider = collision.GetComponent<CurrencyUI_DetectionCollider>();

        if (OrbCollider == null) return;

        Currency_UI currencyUI = OrbCollider.GetComponentInParent<Currency_UI>();
        if(currencyUI.GetCurrencyType() == PlayerCurrencies.CurrencyType.smallBlueOrb) {
            if (smallOrbs.Contains(currencyUI)) {
                smallOrbs.Remove(currencyUI);
            }
        }
    }
}
