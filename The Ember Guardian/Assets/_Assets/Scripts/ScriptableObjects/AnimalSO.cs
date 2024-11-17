using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AnimalSO : ScriptableObject
{
    public PlayerCurrencies.CurrencyType currencyTypeDropped;
    public int currencyDropAmount;

    public PlayerCurrencies.CurrencyType currency2TypeDropped;
    public int currency2DropAmount;

    public int maxHP;

    public float roamMoveSpeed;
    public float fleeMoveSpeed;
    public float fleeDistance;

    public float roamRadius;
    public float roamChangeDestinationRate;

    public float dieAnimationTime;
}
