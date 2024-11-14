using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SoundRefsSO : ScriptableObject
{
    public AudioClip[] orbDropped;
    public AudioClip[] orbTouchedFloor;
    public AudioClip[] orbPickedUpByPlayer;
    public AudioClip[] orbPickedUpByWorker;
    public AudioClip[] orbDroppedUpByWorker;

    public AudioClip[] workerRecruited;
    public AudioClip[] workerHunterJobAssigned;
    public AudioClip[] workerDied;
    public AudioClip[] hunterArrowReleased;
    public AudioClip[] hunterArrowHit;

    public AudioClip dawnStart;
    public AudioClip dawnStartWhoosh;
    public AudioClip dayStart;
    public AudioClip dayStartWhoosh;
    public AudioClip duskStart;
    public AudioClip duskStartWhoosh;
    public AudioClip nightStart;
    public AudioClip nightStartWhoosh;
    public AudioClip ammoTickAdded;
    public AudioClip hpTickAdded;
    public AudioClip fireTickRemoved;
}
