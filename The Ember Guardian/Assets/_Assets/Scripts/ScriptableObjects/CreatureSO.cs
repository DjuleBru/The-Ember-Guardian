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


    [HorizontalGroup("Game Data", 75)]
    [PreviewField]
    public GameObject creaturePrefab;

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
    public bool flying;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 20)]
    [ShowIf("flying")]
    public float flightMinAltitude;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 20)]
    [ShowIf("flying")]
    public float flightMaxAltitude;
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
    [Range(1, 20)]
    public int damage;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1f, 5)]
    public float attackRate;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(1, 3)]
    public float enteredLightattackRateDebuff;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    public bool isRangedAttack;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(.1f, 20)]
    public float minAttackRange;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(.1f, 20)]
    public float maxAttackRange;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    public bool hasSpecialAbility;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [ShowIf("hasSpecialAbility")]
    [Range(.1f, 20)]
    public float specialAbilityCooldown;

    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(.1f, 10)]
    public float attackRangeRandomizer;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(.1f, 10)]
    [ShowIf("isRangedAttack")]
    public float attackRangeMaxDistanceMiss;
    [VerticalGroup("Game Data/Stats")]
    [LabelWidth(200)]
    [Range(0, 3)]
    public int damageToFire;


    [BoxGroup("Game Data/Status Effects")]
    [LabelWidth(200)]
    public bool immuneToPoison; 
    [BoxGroup("Game Data/Status Effects")]
    [LabelWidth(200)]
    public bool immuneToShock;
    [BoxGroup("Game Data/Status Effects")]
    [LabelWidth(200)]
    public bool immuneToImmobilize;

    [BoxGroup("Game Data/Animation Parameters")]
    [LabelWidth(200)]
    public float attackAnimationDelay;
    [BoxGroup("Game Data/Animation Parameters")]
    [LabelWidth(200)]
    public float attackSFXDelayAfterAnimationStart;
    [BoxGroup("Game Data/Animation Parameters")]
    [LabelWidth(200)]
    public float totalAttackAnimationTime;
    [BoxGroup("Game Data/Animation Parameters")]
    [LabelWidth(200)]
    public bool hasCustomSpawnAnimation;
    [BoxGroup("Game Data/Animation Parameters")]
    [LabelWidth(200)]
    public float spawnAnimationDuration = 1f;
    [BoxGroup("Game Data/Animation Parameters")]
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
    [BoxGroup("AI")]
    [LabelWidth(200)]
    public bool canAttackPlayerBehindBarricades;
    [BoxGroup("AI")]
    [LabelWidth(200)]
    public bool canAttackPlayerOnTower;


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


    public bool attackSFXHandledByAnimation;
    [ShowIf("attackSFXHandledByAnimation")]
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] attackStartedChargingAudioClips;
    [ShowIf("attackSFXHandledByAnimation")]
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] attackReleasedAudioClips;

    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] attackHitAudioClips;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public float attackVolumeMultiplier = .5f;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public AudioClip[] attackAudioClips;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public float attackHitVolumeMultiplier = .5f;

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
    public AudioClip[] spawnAudioClips;
    [BoxGroup("SFX")]
    [LabelWidth(200)]
    public float spawnVolumeMultiplier = .75f;

}
