using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsSO : ScriptableObject
{
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    public float initialMoveSpeed;
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    public float initialRunMaxTime;
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    public float initialExhaustionTime;
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    public float initialRunAccelerationFactor;

    [BoxGroup("Health")]
    [LabelWidth(125)]
    public int initialMaxPlayerHP = 3;
    [BoxGroup("Health")]
    [LabelWidth(125)]
    public float initialDamagedImmunityTime = 1.5f;
    [BoxGroup("Health")]
    [LabelWidth(125)]
    public float initialRespawnTime = 5f;
    [BoxGroup("Health")]
    [LabelWidth(125)]
    public int initialPlayerRespawnHealth = 3;
    [BoxGroup("Health")]
    [LabelWidth(125)]
    public float initialHpRegenTimer = 0f;
}
