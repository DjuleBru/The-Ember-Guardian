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
    public float initialMaxStamina;
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    public float initialExhaustionTime;
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    public float initialRunAccelerationFactor;
    [BoxGroup("Movement")]
    [LabelWidth(125)]
    public float initialAimingSightDecelerationFactor;

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
    public int initialPlayerRespawnHP = 3;
    [BoxGroup("Health")]
    [LabelWidth(125)]
    public float initialHpRegenTimer = 0f;

    [BoxGroup("Backpack")]
    [LabelWidth(125)]
    public int initialStartLevelAmmo = 3;
    [BoxGroup("Backpack")]
    [LabelWidth(125)]
    public int initialStartLevelOrbs = 0;

    [BoxGroup("Other")]
    [LabelWidth(125)]
    public float flashlightRange = 10f;

    [BoxGroup("Other")]
    [LabelWidth(125)]
    public int initialMeleeDamage = 5;

}
