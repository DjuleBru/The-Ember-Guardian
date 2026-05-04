using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveFuelFireOnKills : MonoBehaviour {

    [SerializeField] private Transform fuelFirePrefab;
    [SerializeField] private ParticleSystem activateSkillPS;
    [SerializeField] private ParticleSystem skillActivePS;
    private bool initialFireLit;

    private void Start() {
        if (SceneLoader.Instance.GetSceneType() != SceneLoader.SceneType.Level) return;

        Creature.OnAnyMobDied += Creature_OnAnyMobDied;
        Fire.Instance.OnInitialFireActivated += Fire_OnInitialFireActivated;
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated += PlayerSkills_OnActiveSkillDeactivated;
    }
    private void PlayerSkills_OnActiveSkillDeactivated(object sender, PlayerSkills.OnSkillDeactivatedArgs e) {
        if (e.skillTypeDeactivated == SkillItem.SkillType.activeFeedFireOnKills) {
            skillActivePS.Stop();
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeFeedFireOnKills) {
            activateSkillPS.Play();
            skillActivePS.Play();
        }
    }

    private void Fire_OnInitialFireActivated(object sender, System.EventArgs e) {
        initialFireLit = true;
    }

    private void Creature_OnAnyMobDied(object sender, System.EventArgs e) {
        if (!initialFireLit) return;
        if (!PlayerSkills.Instance.GetFuelFireOnKills()) return;

        Creature creatureKilled = sender as Creature;
        if (creatureKilled == null) return;

        Vector3 spawnPosition = creatureKilled.GetProjectileTarget().position;
        StartCoroutine(InstantiateHealPips(spawnPosition, PlayerSkills.Instance.GetFuelPipsPerKill()));
    }

    private IEnumerator InstantiateHealPips(Vector3 spawnPosition, int pipAmount) {

        for (int i = 0; i < pipAmount; i++) {
            Vector3 spawnPositionRandomized = new Vector3(Random.Range(spawnPosition.x - .5f, spawnPosition.x + .5f), Random.Range(spawnPosition.y - .5f, spawnPosition.y + .5f), 0);
            FlyingCollectible flyingCollectible = Instantiate(fuelFirePrefab, spawnPositionRandomized, Quaternion.identity).GetComponent<FlyingCollectible>();
            flyingCollectible.SetDestination(Fire.Instance.transform);

            flyingCollectible.OnDestinationReached += FlyingCollectible_OnDestinationReached;

            float delayRandomized = Random.Range(.05f, .2f);
            yield return new WaitForSeconds(delayRandomized);
        }
    }

    private void FlyingCollectible_OnDestinationReached(object sender, System.EventArgs e) {
        Fire.Instance.FuelFire(PlayerSkills.Instance.GetFuelPerPip());
    }


    private void OnDestroy() {
        Creature.OnAnyMobDied -= Creature_OnAnyMobDied;
        PlayerSkills.Instance.OnActiveSkillActivated -= PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated -= PlayerSkills_OnActiveSkillDeactivated;
    }
}
