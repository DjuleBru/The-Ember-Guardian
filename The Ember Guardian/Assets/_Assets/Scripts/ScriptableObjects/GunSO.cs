using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GunSO : ScriptableObject
{
    public Sprite gunSprite;
    public Sprite reticleSprite;
    public Animator gunAnimator;
    public int maxAmmo;
    public int shotsPerClip;

    public float shootCooldownTime;
    public float reloadTime;

    public float gunKnockback;
    public float gunRecoil;
    public float gunRecoilDamping;
}
