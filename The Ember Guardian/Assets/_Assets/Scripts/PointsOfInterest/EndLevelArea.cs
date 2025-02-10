using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelArea : MonoBehaviour
{
    public static EndLevelArea Instance;

    [SerializeField] private Transform endLevelAreaSpawnerParent;
    [SerializeField] private GameObject endLevelAreaFire;
    [SerializeField] private Portal endLevelPortal;

    private MobSpawner[] endLevelAreaSpawnerList;
    private List<Mob> mobsInArea = new List<Mob>();
    private List<Mob> mobsAggroingPlayer = new List<Mob>();

    public event EventHandler OnEndLevelAreaCleared;
    public event EventHandler OnEndLevelFireLit;

    private bool playerInTriggerArea;
    private bool playerDestroyedNest;

    private void Awake() {
        Instance = this;

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
            MusicManager.Instance.StopEndLevelMusic();
        }
    }

    private void RemoveMobFromMobsInArea(Mob mob) {
        mobsInArea.Remove(mob);

        if(mobsAggroingPlayer.Contains(mob)) {
            RemoveMobAggroingPlayer(mob);
        } 

        if (mobsInArea.Count == 0) {
            endLevelAreaFire.SetActive(true);
            OnEndLevelAreaCleared?.Invoke(this, EventArgs.Empty);
            MusicManager.Instance.StopEndLevelMusic();
        }
    }

    public void SetEndLevelFireLit() {
        OnEndLevelFireLit?.Invoke(this, EventArgs.Empty);
        foreach(MobSpawner spawner in endLevelAreaSpawnerList) {
            spawner.SetMobsCanSpawnAtDawn(false);
        }

        playerDestroyedNest = true;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) return;

    }

    public bool GetPlayerDestroyedNest() {
        return playerDestroyedNest;
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
