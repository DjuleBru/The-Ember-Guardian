using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SoundRefsSO : ScriptableObject
{
    public AudioClip[] bigBlueOrbDropped;
    public AudioClip[] smallBlueOrbDropped;
    public AudioClip[] bigRedOrbDropped;
    public AudioClip[] smallRedOrbDropped;
    public AudioClip[] ammoDropped;
    public AudioClip[] gemDropped;

    public AudioClip[] bigBlueOrbTouchedFloor;
    public AudioClip[] smallBlueOrbTouchedFloor;
    public AudioClip[] bigRedOrbTouchedFloor;
    public AudioClip[] smallRedOrbTouchedFloor;
    public AudioClip[] ammoTouchedFloor;
    public AudioClip[] gemTouchedFloor;

    public AudioClip[] bigBlueOrbPickedUpByPlayer;
    public AudioClip[] smallBlueOrbPickedUpByPlayer;
    public AudioClip[] bigRedOrbPickedUpByPlayer;
    public AudioClip[] smallRedOrbPickedUpByPlayer;
    public AudioClip[] ammoPickedUpByPlayer;
    public AudioClip[] emberPickedUpByPlayer;
    public AudioClip[] gemPickedUpByPlayer;

    public AudioClip[] orbPickedUpByWorker;
    public AudioClip[] orbDroppedUpByWorker;

    public AudioClip[] workerRecruited;
    public AudioClip[] workerHunterJobAssigned;
    public AudioClip[] workerDied;
    public AudioClip[] hunterArrowReleased;
    public AudioClip[] hunterArrowHit;

    public AudioClip[] propBurned;

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
