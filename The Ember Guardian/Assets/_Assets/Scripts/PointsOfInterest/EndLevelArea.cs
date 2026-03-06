using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelArea : MonoBehaviour
{
    public static EndLevelArea Instance;
    public static EndLevelArea Instance_Left;

    [SerializeField] private Transform endLevelAreaSpawnerParent;
    [SerializeField] private GameObject endLevelAreaFire;
    [SerializeField] private Portal endLevelPortal;
    [SerializeField] private StructureLocation endLevelAreaBackToBaseTPLocation;

    [SerializeField] private bool isLeftEndLevelArea;

    private MobSpawner[] endLevelAreaSpawnerList;
    private List<Mob> mobsInArea = new List<Mob>();
    private List<Mob> mobsAggroingPlayer = new List<Mob>();

    public event EventHandler OnEndLevelAreaUnCleared;
    public event EventHandler OnEndLevelAreaCleared;
    public event EventHandler OnEndLevelFireLit;

    private bool playerInTriggerArea;
    private bool playerDestroyedNest;
    private bool endLevelAreaCleared;

    private void Awake() {
        if(!isLeftEndLevelArea) {
            Instance = this;
        } else {
            Instance_Left = this;
        }

        endLevelAreaSpawnerList = endLevelAreaSpawnerParent.GetComponentsInChildren<MobSpawner>();

        foreach (MobSpawner mobSpawner in endLevelAreaSpawnerList) {
            mobSpawner.OnMobRemoved += MobSpawner_OnMobRemoved;
            mobSpawner.OnMobSpawned += MobSpawner_OnMobSpawned;
        }

        endLevelAreaFire.SetActive(false);
        endLevelPortal.gameObject.SetActive(false);
    }


    private void MobSpawner_OnMobSpawned(object sender, MobSpawner.OnMobSpawnedEventArgs e) {
        AddMobToMobsInArea(e.mob);
    }

    private void MobSpawner_OnMobRemoved(object sender, MobSpawner.OnMobSpawnedEventArgs e) {
        RemoveMobFromMobsInArea(e.mob);
       
    }

    private void AddMobToMobsInArea(Mob mob) {
        mobsInArea.Add(mob);
        mob.GetComponent<CreatureAI>().OnCreatureTargetPlayer += CreatureAI_OnCreatureTargetPlayer;
        mob.GetComponent<CreatureAI>().OnCreatureUntargetPlayer += CreatureAI_OnCreatureUnaggro;

        if(endLevelAreaCleared) {
            endLevelAreaCleared = false;
            endLevelAreaFire.SetActive(false);
            OnEndLevelAreaUnCleared?.Invoke(this, EventArgs.Empty);
        }

    }

    private void CreatureAI_OnCreatureTargetPlayer(object sender, EventArgs e) {
        CreatureAI creatureAI = (CreatureAI)sender;
        Mob mob = creatureAI.GetComponent<Mob>();
        mobsAggroingPlayer.Add(mob);

    }


    private void CreatureAI_OnCreatureUnaggro(object sender, EventArgs e) {
        CreatureAI creatureAI = (CreatureAI)sender;
        Mob mob = creatureAI.GetComponent<Mob>();
        RemoveMobAggroingPlayer(mob);
        
    }

    private void RemoveMobAggroingPlayer(Mob mob) {
        mobsAggroingPlayer.Remove(mob);
        TryFadeOutMusic();
    }

    public void TryFadeOutMusic() {
        if (mobsAggroingPlayer.Count == 0) {
            if (playerInTriggerArea) return;
            MusicManager.Instance.StopCurrentMusic();
        }
    }

    private void RemoveMobFromMobsInArea(Mob mob) {
        mobsInArea.Remove(mob);

        if(mobsAggroingPlayer.Contains(mob)) {
            RemoveMobAggroingPlayer(mob);
        } 

        if (mobsInArea.Count == 0) {
            ClearEndLevelArea();
        }
    }

    [Button]
    public void ClearEndLevelArea() {
        endLevelAreaFire.SetActive(true);
        StartCoroutine(ActivateEndLevelTPAfterDelay(1f));
        OnEndLevelAreaCleared?.Invoke(this, EventArgs.Empty);
        MusicManager.Instance.StopCurrentMusic();

        endLevelAreaCleared = true;
    }

    private IEnumerator ActivateEndLevelTPAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        if (endLevelAreaBackToBaseTPLocation != null) {
            endLevelAreaBackToBaseTPLocation.BuildStructure();
        }
    }

    public void SetEndLevelFireLit() {
        OnEndLevelFireLit?.Invoke(this, EventArgs.Empty);
        foreach(MobSpawner spawner in endLevelAreaSpawnerList) {
            spawner.SetMobsCanSpawnAtDawn(false);
        }

        playerDestroyedNest = true;
    }

    public bool GetPlayerDestroyedNest() {
        return playerDestroyedNest;
    }

    public void SetPlayerDestroyedNest() {

        OnEndLevelFireLit?.Invoke(this, EventArgs.Empty);
        foreach (MobSpawner spawner in endLevelAreaSpawnerList) {
            spawner.SetMobsCanSpawnAtDawn(false);
        }

        this.playerDestroyedNest = true;
    }

    public bool AllCreaturesKilled() {
        return mobsInArea.Count == 0;
    }

    public void SetPlayerInTriggerArea(bool playerInTriggerArea) {
        this.playerInTriggerArea = playerInTriggerArea;

        if(playerInTriggerArea) {
            DayNightManager.Instance.SetCyclePaused(true, true);
        } else {
            DayNightManager.Instance.SetCyclePaused(false, true);
        }
    }

}
