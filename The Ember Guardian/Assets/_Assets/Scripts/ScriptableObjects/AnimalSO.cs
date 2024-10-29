using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AnimalSO : ScriptableObject
{
    public PlayerCurrencies.CurrencyType currencyTypeDropped;
    public Transform currencyPrefab;

    public int currencyDropAmount;
    public int maxHP;

    public float roamMoveSpeed;
    public float fleeMoveSpeed;
    public float fleeDistance;

    public float roamRadius;
    public float roamChangeDestinationRate;

    public float dieAnimationTime;
}
