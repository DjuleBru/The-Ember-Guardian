using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveDamageSpells : MonoBehaviour
{
    [SerializeField] private Transform darkFlamePrefab;
    [SerializeField] private Transform darkSwordPrefab;
    [SerializeField] private Transform reaperPrefab;
    [SerializeField] private Transform darkMinePrefab;

    private float darkFlameDistanceToPlayer = 0f;
    private float darkSwordistanceToPlayer = 3f;
    private float reaperDistanceToPlayer = 5f;
    private float darkMinePrefabDistanceToPlayer = 3f;
    private float darkMineKnockbackAmount = 75f;
    private float darkSwordImmobilizeDuration = 1.5f;


    private void Start() {
        PlayerSkills.Instance.OnActiveSkillActivated += PLayerSkills_OnActiveSkillActivated;
    }

    private void PLayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        float instantiateDir = PlayerAim.Instance.GetAimDir().x;
        if(instantiateDir < 0) {
            instantiateDir = -1;
        } else {
            instantiateDir = 1;
        }

        if (e.skillItemAdded.skillType ==SkillItem.SkillType.activeDarkFlame) {
            Vector3 instantiatePosition = new Vector3(Player.Instance.transform.position.x + darkFlameDistanceToPlayer* instantiateDir, 0, 0);
            StaticProjectile projectile = Instantiate(darkFlamePrefab, instantiatePosition, Quaternion.identity).GetComponent<StaticProjectile>();

            projectile.Initialize(instantiateDir, null, PlayerSkills.Instance.GetDarkFlameDamage(), true, true);
            projectile.InitializeBurning(PlayerSkills.Instance.GetDarkFlameBurnAmount());
        }
        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeReaper) {
            Vector3 instantiatePosition = new Vector3(Player.Instance.transform.position.x + reaperDistanceToPlayer * instantiateDir, 0, 0);
            StaticProjectile projectile = Instantiate(reaperPrefab, instantiatePosition, Quaternion.identity).GetComponent<StaticProjectile>();

            projectile.Initialize(instantiateDir, null, PlayerSkills.Instance.GetReaperDamage(), true, true);
        }
        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeDarkSword) {
            Vector3 instantiatePosition = new Vector3(Player.Instance.transform.position.x + darkSwordistanceToPlayer * instantiateDir, 0, 0);
            StaticProjectile projectile = Instantiate(darkSwordPrefab, instantiatePosition, Quaternion.identity).GetComponent<StaticProjectile>();

            projectile.Initialize(instantiateDir, null, PlayerSkills.Instance.GetDarkSwordDamage(), true, true);
            projectile.InitializeImmobilize(darkSwordImmobilizeDuration);
        }
        if (e.skillItemAdded.skillType == SkillItem.SkillType.activePlantMine) {
            Vector3 instantiatePosition = new Vector3(Player.Instance.transform.position.x + darkMinePrefabDistanceToPlayer * instantiateDir, 0, 0);
            StaticProjectile projectile = Instantiate(darkMinePrefab, instantiatePosition, Quaternion.identity).GetComponent<StaticProjectile>();

            projectile.Initialize(instantiateDir, null, PlayerSkills.Instance.GetDarkMineDamage(), true, true);
            projectile.InitializePoison(PlayerSkills.Instance.GetDarkMinePoisonAmount());
            projectile.InitializeKnockback(darkMineKnockbackAmount);
        }
    }
}
