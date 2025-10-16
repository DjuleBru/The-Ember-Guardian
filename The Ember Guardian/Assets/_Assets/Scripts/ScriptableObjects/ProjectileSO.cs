using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectileForces;

[CreateAssetMenu()]
public class ProjectileSO : ScriptableObject
{
    public Transform projectilePrefab;
    public bool isExplosiveProjectile;
    public bool canBeDestoyedByBullets;

    public bool usesAnimationCurve;
    [BoxGroup("AnimationCurve")]
    public AnimationCurve projectileTrajectoryAnimationCurve;
    [BoxGroup("AnimationCurve")]
    public AnimationCurve projectileYDifferentialWithTargetAnimationCurve;
    [BoxGroup("AnimationCurve")]
    public AnimationCurve projectileSpeedAnimationCurve;
    [BoxGroup("AnimationCurve")]
    public float projectileMaxMoveSpeed;
    [BoxGroup("AnimationCurve")]
    public float projectileTrajectoryYCurve = .2f;
    [BoxGroup("AnimationCurve")]
    public float trajectoryEndPointRandomOffsetValue;

    public bool usesForce;

    [BoxGroup("Forces")]
    public ProjectileForces.TrajectoryMode trajectoryMode = TrajectoryMode.CurvedApex;
    [BoxGroup("Forces")]
    public float straightLineSpeed = 10f;
    [BoxGroup("Forces")]
    public float straightLineYTargetRandomizer;

    [BoxGroup("Forces")]
    public float minApexY;
    [BoxGroup("Forces")]
    public float maxApexY;
    [BoxGroup("Forces")]
    public float distanceToTargetForMinApex;
    [BoxGroup("Forces")]
    public float distanceToTargetForMaxApex;
    [BoxGroup("Forces")]
    public float apexRandomizer;
    [BoxGroup("Forces")]
    public float gravityScale;

    public AudioClip[] projectileInstantiatedAudioClips;
    public AudioClip[] projectileHitAudioClips;



}
