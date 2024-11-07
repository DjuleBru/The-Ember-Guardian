using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ProjectileSO : ScriptableObject
{
    public Transform projectilePrefab;

    public AnimationCurve projectileTrajectoryAnimationCurve;
    public AnimationCurve projectileYDifferentialWithTargetAnimationCurve;
    public AnimationCurve projectileSpeedAnimationCurve;

    public float projectileMaxMoveSpeed;
    public float projectileTrajectoryYCurve = .2f;
    public float trajectoryEndPointRandomOffsetValue;

    public AudioClip[] projectileInstantiatedAudioClips;
    public AudioClip[] projectileHitAudioClips;

}
