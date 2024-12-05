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

    public event EventHandler OnEndLevelAreaCleared;
    public event EventHandler OnEndLevelFireLit;

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
        mobsInArea.Add(e.mob);
    }

    private void MobSpawner_OnMobRemoved(object sender, MobSpawner.OnMobSpawnedEventArgs e) {
        mobsInArea.Remove(e.mob);

        if(mobsInArea.Count == 0) {
            endLevelAreaFire.SetActive(true);
            OnEndLevelAreaCleared?.Invoke(this, EventArgs.Empty);
            MusicManager.Instance.FadeOutMusic(3f);
        }
    }

    public void SetEndLevelFireLit() {
        OnEndLevelFireLit?.Invoke(this, EventArgs.Empty);
        if (Tutorial.Instance != null) return;

        StartCoroutine(EnableEndLevelPortal());
    }

    private IEnumerator EnableEndLevelPortal() {
        yield return new WaitForSeconds(4f);
        endLevelPortal.gameObject.SetActive(true);
    }


}
