using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu()]
public class CreatureSO : ScriptableObject
{

    [BoxGroup("Basic Info")]
    [LabelWidth(100)]
    public string enemyName;
    [BoxGroup("Basic Info")]
    [LabelWidth(100)]
    [TextArea]
    public string description;


    [HorizontalGroup("Game Data", 75)]
    [PreviewField(75)]
    public GameObject creaturePrefab;

    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1,20)]
    public float moveSpeed;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(0, 5)]
    public float moveSpeedRandomizerDelta;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 100)]
    public int maxHealth;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 20)]
    public int damage;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1f, 5)]
    public float attackRate;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(.1f, 10)]
    public float attackRange;
    [VerticalGroup("Game Data/Animation Parameters")]
    [LabelWidth(200)]
    public float attackAnimationDelay;
    [VerticalGroup("Game Data/Animation Parameters")]
    [LabelWidth(200)]
    public float totalAttackAnimationTime;
    [VerticalGroup("Game Data/Animation Parameters")]
    [LabelWidth(200)]
    [Range(.5f, 2)]
    public float baseMovementAnimationSpeed;


    [BoxGroup("AI")]
    [LabelWidth(200)]
    [Range(1, 5)]
    public int workerTargetingPriority;
    [BoxGroup("AI")]
    [LabelWidth(200)]
    [Range(1, 5)]
    public int playerTargetingPriority;
    [BoxGroup("AI")]
    [LabelWidth(200)]
    [Range(1, 5)]
    public int barricadeTargetingPriority;

    [BoxGroup("Wave Stats")]
    [LabelWidth(200)]
    [Range(1, 10)]
    public int difficulty; // between 1 and 10
    [BoxGroup("Wave Stats")]
    [LabelWidth(200)]
    [Range(1, 10)]
    public int initialWaveSpawn; // between 1 and 10
    [BoxGroup("Wave Stats")]
    [LabelWidth(200)]
    [Range(0, 1)]
    public float spawnProbability; // between 0 and 1

}
