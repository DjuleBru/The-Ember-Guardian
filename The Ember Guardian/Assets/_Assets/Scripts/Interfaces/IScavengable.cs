using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScavengable : IDamageable
{

    public event EventHandler OnPlayerTriggerIn;
    public event EventHandler OnPlayerTriggerOut;
    public event EventHandler OnScavengableDepleted;
    public event EventHandler OnScavengableMarkedToScavenge;
    public event EventHandler OnMinerExtractedResourceFromMine;
    public event EventHandler OnMinerStartsMining;
    public event EventHandler OnMinerStopsMining;
    public event EventHandler OnMineEffortLoaded;
    public event EventHandler<Scavengable.OnScavengableDeactivatedMiningEventArgs> OnActivatedMining;
    public event EventHandler<Scavengable.OnScavengableDeactivatedMiningEventArgs> OnDeactivatedMining;

    bool GetDepleted();
    bool GetMaxMinersAssigned();
    bool GetMarkedToScavenge();
    bool GetIsMine();
    int GetMiningPriority();
    bool GetScavengingActive();
    int GetMaxMinerAmount();
    int GetMinerAmountMining();
    float GetTimeToExtractOneResourceNormalized();
    void MinerStartsMining(MinerJob minerJob);
    void MinerStopsMining(MinerJob minerJob);
    void AssignMiner(MinerJob minerJob);
    void UnassignMiner(MinerJob minerJob);

    void SetScavengableUnlocked(bool unlocked);
}
