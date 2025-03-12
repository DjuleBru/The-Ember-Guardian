using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public AudioClip[] trapTouchedFloor;

    public AudioClip[] bigBlueOrbPickedUpByPlayer;
    public AudioClip[] smallBlueOrbPickedUpByPlayer;
    public AudioClip[] bigRedOrbPickedUpByPlayer;
    public AudioClip[] smallRedOrbPickedUpByPlayer;
    public AudioClip[] ammoPickedUpByPlayer;
    public AudioClip[] emberPickedUpByPlayer;
    public AudioClip[] redGemPickedUpByPlayer;
    public AudioClip[] greenGemPickedUpByPlayer;
    public AudioClip[] trapPickedUpByPlayer;

    public AudioClip[] bigCollectiblePlouf;
    public AudioClip[] smallCollectiblePlouf;

    public AudioClip[] orbPickedUpByWorker;
    public AudioClip[] orbDroppedUpByWorker;

    public AudioClip[] workerRecruited;
    public AudioClip[] workerHunterJobAssigned;
    public AudioClip[] workerDied;
    public AudioClip[] hunterArrowReleased;
    public AudioClip[] hunterArrowHit;

    public AudioClip[] propBurned;
    public AudioClip[] playerCallDog;

    public AudioClip[] gemMerchantVoiceLines;
    public AudioClip[] dogTamerVoiceLines;
    public AudioClip[] structuresMerchantVoiceLines;
    public AudioClip[] muhsroomMerchantVoiceLines;
    public AudioClip[] heroMerchantVoiceLines;
    public AudioClip[] workerMerchantVoiceLines;
    public AudioClip[] gunMerchantVoiceLines;
    public AudioClip[] codexVoiceLines;

    public AudioClip pressMenuButton;
    public AudioClip hoverOrSelectMenuButton;
    public AudioClip hoverOrSelectHubMerchantItem;
    public AudioClip buyHubMerchantItem;
    public AudioClip[] upgradeHubMerchantItem;
    public AudioClip failBuyHubMerchantItem;
    public AudioClip tryBuyLockedHubMerchantItem;
    public AudioClip tryBuyMaxedHubMerchantItem;
    public AudioClip[] gemPSExplosion;

    public AudioClip hubChestOpen;
    public AudioClip hubChestClose;
    public AudioClip scavengableMarkedToScavenge;

    public AudioClip workerStartedFollowing;
    public AudioClip workerStoppedFollowing;
    public AudioClip hoveredFollowingWorkerChanged;

    public AudioClip huntingFlagPickedUp;
    public AudioClip huntingFlagDropped;
    public AudioClip huntingFlagReset;
    public AudioClip gunLightSwitch;
    public AudioClip spotLightActivate;
    public AudioClip switchGunFireMode;
    public AudioClip aimSightStart;
    public AudioClip aimSightEnd;
    public AudioClip smgOverclockStart;
    public AudioClip smgOverclockEnd;
    public AudioClip shotgunFocusedBlast;
    public AudioClip lmgSetup;
    public AudioClip lmgReset;
    public AudioClip revolverCooldown;

    public AudioClip playerRespawnFireExtact;
    public AudioClip activeSkillReady;
    public AudioClip passiveShieldActivate;
    public AudioClip passiveShieldDie;
    public AudioClip playerActiveTeleport;

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
    public AudioClip criticalFireTickRemoved;
    public AudioClip tooltipShown;
    public AudioClip tooltipHidden;
    public AudioClip objectiveShown;
    public AudioClip objectiveCompleted;
    public AudioClip subObjectiveCompleted;
    public AudioClip subObjectiveProgressed;
    public AudioClip locationRevealed;
}
