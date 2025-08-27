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
    public bool isBoss;
    [BoxGroup("Basic Info")]
    [LabelWidth(100)]
    [TextArea]
    public string description;
    [BoxGroup("Basic Info")]
    [LabelWidth(100)]
    public Sprite creatureIcon;
    [BoxGroup("Basic Info")]
    [LabelWidth(100)]
    public Sprite creatureIcon_Portrait;


    [BoxGroup("Basic Info")]
    [LabelWidth(100)]
    public GameObject creaturePrefab;

    [HorizontalGroup("Game Data")]
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 100)]
    public float mass;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 20)]
    public float detectionRange_Day;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 20)]
    public float detectionRange_Night;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    public CreatureAttackSO primaryAttackSO;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    public bool hasRangedAndMeleeAttack;
    [ShowIf("hasRangedAndMeleeAttack")]
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    public CreatureAttackSO secondaryAttackSO;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    public CreatureAttackSO specialAttackSO;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    public bool flying;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    public bool canFlank;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(0, 20)]
    [ShowIf("flying")]
    public float flightMinAltitude;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(0, 20)]
    [ShowIf("flying")]
    public float flightMaxAltitude;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(0, 5)]
    [ShowIf("flying")]
    public float flightAltitudeRandomizer;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 20)]
    public float dayMoveSpeed;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 20)]
    public float nightMoveSpeed;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 20)]
    public float aggroMoveSpeedBuff = 1.5f;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 3)]
    public float enteredLightMoveSpeedDebuff;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(0, 5)]
    public float moveSpeedRandomizerDelta;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 3000)]
    public int maxHealth;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 3)]
    public float enteredLightattackRateDebuff;

    [BoxGroup("Game Data/Status Effects")]
    [LabelWidth(200)]
    public float probabilityToAggroOnGunShot = 1f;
    [BoxGroup("Game Data/Status Effects")]
    [LabelWidth(200)]
    public bool immuneToPoison; 
    [BoxGroup("Game Data/Status Effects")]
    [LabelWidth(200)]
    public bool immuneToShock;
    [BoxGroup("Game Data/Status Effects")]
    [LabelWidth(200)]
    public bool immuneToImmobilize;

    [BoxGroup("Animation Parameters")]
    [LabelWidth(200)]
    public bool hasCustomSpawnAnimation;
    [BoxGroup("Animation Parameters")]
    [LabelWidth(200)]
    public float spawnAnimationDuration = 1f;
    [BoxGroup("Animation Parameters")]
    [LabelWidth(200)]
    [Range(.5f, 2)]
    public float baseMovementAnimationSpeed;


    [BoxGroup("AI")]
    [LabelWidth(200)]
    [Range(0, 5)]
    public int workerTargetingPriority;
    [BoxGroup("AI")]
    [LabelWidth(200)]
    [Range(0, 5)]
    public int playerTargetingPriority;
    [BoxGroup("AI")]
    [LabelWidth(200)]
    [Range(0, 5)]
    public int barricadeTargetingPriority;


    [BoxGroup("Drop Stats")]
    [LabelWidth(200)]
    public List<PlayerCurrencies.CurrencyType> currencyTypeDroppedList;
    [BoxGroup("Drop Stats")]
    [LabelWidth(200)]
    public List<int> currencyDropAmountList;

    [BoxGroup("Wave Stats")]
    [LabelWidth(200)]
    [Range(1, 100)]
    public int difficulty; // between 1 and 100
    [BoxGroup("Wave Stats")]
    [LabelWidth(200)]
    [Range(1, 10)]
    public int initialWaveSpawn; // between 1 and 10
    [BoxGroup("Wave Stats")]
    [LabelWidth(200)]
    [Range(0, 1)]
    public float spawnProbability; // between 0 and 1
    [BoxGroup("Wave Stats")]
    [LabelWidth(200)]
    [Range(0, 15)]
    public int maxCreaturesPerPacket;


    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] dieAudioClips;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public float dieVolumeMultiplier = .75f;

    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] aggroAudioClips;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public float aggroVolumeMultiplier = .5f;

    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] idleAudioClips;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public float idleVolumeMultiplier = .5f;

    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] footStepAudioClips;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public float footstepVolumeMultiplier = .2f;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public bool hasContinousAudioClip;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] continuousAudioClip;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public bool hasContinousMovementAudioClip;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] continuousMovementAudioClip;

    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] spawnAudioClips;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public float spawnVolumeMultiplier = .75f;


    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] bulletHitAudioClips;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public float bulletHitVolumeMultiplier = 1f;

}
