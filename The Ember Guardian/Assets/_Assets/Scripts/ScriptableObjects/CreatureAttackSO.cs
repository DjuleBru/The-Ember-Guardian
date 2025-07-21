using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class CreatureAttackSO : ScriptableObject
{
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool isRangedAttack; 
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool isProjectileAttack;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool isStaticProjectileAttack;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool isAnimatedAttack;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public float attackCooldown;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public float minAttackRange;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public float maxAttackRange;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool hasDifferentDayAndNightRange;
    [ShowIf("hasDifferentDayAndNightRange")]
    [BoxGroup("General")]
    [LabelWidth(200)]
    public float minAttackRange_Night;
    [ShowIf("hasDifferentDayAndNightRange")]
    [BoxGroup("General")]
    [LabelWidth(200)]
    public float maxAttackRange_Night;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public float attackRangeRandomizer;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public int damage;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public int damageToFire;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public int damageToBarricades;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool canAttackPlayerBehindBarricades;
    [BoxGroup("General")]
    [LabelWidth(200)]
    public bool canAttackPlayerOnTower;

    [BoxGroup("Animation")]
    [LabelWidth(200)]
    public float attackAnimationDelay;
    [BoxGroup("Animation")]
    [LabelWidth(200)]
    public float totalAttackAnimationTime;
    [BoxGroup("Animation")]
    [LabelWidth(200)]
    public float attackSFXDelayAfterAnimationStart;

    [BoxGroup("Projectile")]
    [ShowIf("isStaticProjectileAttack")]
    [LabelWidth(200)]
    public Transform staticProjectilePrefab;
    [BoxGroup("Projectile")]
    [ShowIf("isStaticProjectileAttack")]
    [LabelWidth(200)]
    public bool staticProjectileAutoTargetsPlayer;
    [BoxGroup("Projectile")]
    [LabelWidth(200)]
    public int projectileAmountInPool = 2;
    [BoxGroup("Projectile")]
    [LabelWidth(200)]
    public int projectileAmountShotInAttack = 1;
    [BoxGroup("Projectile")]
    [LabelWidth(200)]
    public float delayBetweenProjectileSpawns;

    [ShowIf("isRangedAttack")]
    [BoxGroup("Ranged")]
    [LabelWidth(200)]
    public ProjectileSO projectileSO;
    [ShowIf("isRangedAttack")]
    [BoxGroup("Ranged")]
    [LabelWidth(200)]
    public float attackRangeRandomizerMiss;

    [BoxGroup("Audio")]
    [LabelWidth(200)]
    public bool attackSFXHandledByAnimation;
    [BoxGroup("Audio")]
    [LabelWidth(200)]
    public AudioClip[] attackAudioClips;
    [BoxGroup("Audio")]
    [LabelWidth(200)]
    public AudioClip[] attackHitAudioClips;
    [BoxGroup("Audio")]
    [LabelWidth(200)]
    public AudioClip[] attackStartedChargingAudioClips;
    [ShowIf("attackSFXHandledByAnimation")]
    [BoxGroup("Audio")]
    [LabelWidth(200)]
    public AudioClip[] attackReleasedAudioClips;
    [BoxGroup("Audio")]
    [LabelWidth(200)]
    public float attackVolumeMultiplier = .5f;
    [BoxGroup("Audio")]
    [LabelWidth(200)]
    public float attackHitVolumeMultiplier = .5f;
}
