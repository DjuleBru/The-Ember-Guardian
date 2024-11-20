using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AnimalSO : ScriptableObject
{
    public List<PlayerCurrencies.CurrencyType> currencyTypeDroppedList;
    public List<int> currencyDropAmountList;

    public int maxHP;

    public float roamMoveSpeed;
    public float fleeMoveSpeed;
    public float fleeDistance;

    public float roamRadius;
    public float roamChangeDestinationRate;

    public float dieAnimationTime;
}
