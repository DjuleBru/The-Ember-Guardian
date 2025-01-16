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
        mob.GetComponent<CreatureAI>().OnCreatureAggro += CreatureAI_OnCreatureAggro;
        mob.GetComponent<CreatureAI>().OnCreatureUnaggro += CreatureAI_OnCreatureUnaggro;

    }

    private void CreatureAI_OnCreatureUnaggro(object sender, EventArgs e) {
        CreatureAI creatureAI = (CreatureAI)sender;
        Mob mob = creatureAI.GetComponent<Mob>();
        RemoveMobAggroingPlayer(mob);
        
    }

    private void RemoveMobAggroingPlayer(Mob mob) {
        mobsAggroingPlayer.Remove(mob);

        if (mobsAggroingPlayer.Count == 0) {
            MusicManager.Instance.FadeOutMusic(3f);
        }
    }

    private void CreatureAI_OnCreatureAggro(object sender, EventArgs e) {
        CreatureAI creatureAI = (CreatureAI)sender;
        Mob mob = creatureAI.GetComponent<Mob>();
        mobsAggroingPlayer.Add(mob);
    }

    private void RemoveMobFromMobsInArea(Mob mob) {
        mobsInArea.Remove(mob);

        if(mobsAggroingPlayer.Contains(mob)) {
            RemoveMobAggroingPlayer(mob);
        } 

        if (mobsInArea.Count == 0) {
            endLevelAreaFire.SetActive(true);
            OnEndLevelAreaCleared?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetEndLevelFireLit() {
        OnEndLevelFireLit?.Invoke(this, EventArgs.Empty);
        foreach(MobSpawner spawner in endLevelAreaSpawnerList) {
            spawner.SetMobsCanSpawnAtDawn(false);
        }

        playerDestroyedNest = true;

        if (SceneLoader.Instance.GetSceneType() == SceneLoader.SceneType.Tutorial) return;

        StartCoroutine(EnableEndLevelPortal());
    }

    private IEnumerator EnableEndLevelPortal() {
        yield return new WaitForSeconds(4f);
        endLevelPortal.gameObject.SetActive(true);
    }

    public bool GetPlayerDestroyedNest() {
        return playerDestroyedNest;
    }

}
