using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantItem
{
    public enum MerchantItemType {
        ActiveSkill,
        PassiveSkill,
        NewGun,
        GunUpgrade,
        Plant
    }

    public string itemName;
    public string itemDescription;
    public string itemStatChanges;
    public int price;
    public Sprite icon;
    public PlayerCurrencies.CurrencyType currencyTypeToPay;
    public MerchantItemType itemType;
    public bool isPurchased { get; private set; }


    // Méthode d'initialisation
    public virtual void Initialize(ScriptableObject data) {
        // Implémentation par les sous-classes
    }

    public virtual void Purchase() {
        // Logique générique pour l'achat (soustraction d'or, ajout à l'inventaire, etc.)

        isPurchased = true;
        Debug.Log(itemName + " " + "purchased "!);
    }
}
