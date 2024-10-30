using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class CreatureSO : ScriptableObject
{
    public Transform creaturePrefab;

    public float moveSpeed;
    public int maxHealth;
    public int damage;
}
