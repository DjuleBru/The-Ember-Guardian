using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveHealOnKills : MonoBehaviour
{

    [SerializeField] private Transform healOnKillsPrefab;
    [SerializeField] private ParticleSystem healOnKillsPS;
    private int pipsToHeal1Health = 4;
    private int currentPipIndex;

    private void Start() {
        Creature.OnAnyMobDied += Creature_OnAnyMobDied;
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
        PlayerSkills.Instance.OnActiveSkillDeactivated += PlayerSkills_OnActiveSkillDeactivated;
    }

    private void PlayerSkills_OnActiveSkillDeactivated(object sender, PlayerSkills.OnSkillDeactivatedArgs e) {
        if(e.skillTypeDeactivated == SkillItem.SkillType.activeHealOnKills) {
            healOnKillsPS.Stop();
        }
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if(e.skillItemAdded.skillType == SkillItem.SkillType.activeHealOnKills) {
            healOnKillsPS.Play();
        }
    }

    private void Creature_OnAnyMobDied(object sender, System.EventArgs e) {
        if (!PlayerSkills.Instance.GetHealPlayerOnKills()) return;

        Creature creatureKilled = sender as Creature;
        Vector3 spawnPosition = creatureKilled.GetProjectileTarget().position;
       StartCoroutine(InstantiateHealPips(spawnPosition, PlayerSkills.Instance.GetHealPipsPerKill()));
    }

    private IEnumerator InstantiateHealPips(Vector3 spawnPosition, int pipAmount) {

        for(int i=0; i < pipAmount; i++) {
            Vector3 spawnPositionRandomized = new Vector3(Random.Range(spawnPosition.x - .5f, spawnPosition.x + .5f), Random.Range(spawnPosition.y - .5f, spawnPosition.y + .5f), 0);
            FlyingCollectible flyingCollectible = Instantiate(healOnKillsPrefab, spawnPositionRandomized, Quaternion.identity).GetComponent<FlyingCollectible>();
            flyingCollectible.SetDestination(Player.Instance.GetProjectileTarget());

            flyingCollectible.OnDestinationReached += FlyingCollectible_OnDestinationReached;

            float delayRandomized = Random.Range(.05f, .2f);
            yield return new WaitForSeconds(delayRandomized);
        }
    }

    private void FlyingCollectible_OnDestinationReached(object sender, System.EventArgs e) {
        currentPipIndex++;
        if(currentPipIndex >= pipsToHeal1Health) {
            Player.Instance.HealPlayer(1);
            currentPipIndex = 0;
        }
    }

    private void OnDestroy() {
        Creature.OnAnyMobDied -= Creature_OnAnyMobDied;
    }
}
