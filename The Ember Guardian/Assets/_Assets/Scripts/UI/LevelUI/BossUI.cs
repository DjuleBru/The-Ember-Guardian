using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{

    public static BossUI Instance;
    [SerializeField] private GameObject bossUIPanel;
    [SerializeField] private Animator bossUIPanelAnimator;

    [SerializeField] private GameObject bossHealthBarFull;
    [SerializeField] private GameObject bossHealthBarFirstHalf;
    [SerializeField] private GameObject bossHealthBarSecondHalf;
    
    [SerializeField] private Image bossHealthBarPhase1;
    [SerializeField] private Image bossHealthBarPhase2;
    [SerializeField] private Image bossHealthBarFill;
    [SerializeField] private TextMeshProUGUI bossNameText;

    private int linkedBossCount;
    private Creature linkedBoss;
    private Creature linkedBoss2;

    private bool bossHasMultiplePhases;
    private bool tryingToShowPanel;
    private bool panelShown;
    private string bossName;

    private void Awake() {
        Instance = this;
        bossUIPanel.SetActive(false);
    }

    private void Update() {
        if(tryingToShowPanel && !panelShown) {

            float distanceToPlayer = Mathf.Abs(linkedBoss.transform.position.x - Player.Instance.transform.position.x);
            if(linkedBoss2 != null) {
                distanceToPlayer = Mathf.Min(distanceToPlayer, Mathf.Abs(linkedBoss2.transform.position.x - Player.Instance.transform.position.x));
            }

            if(distanceToPlayer <= 20) {
                Show();
                tryingToShowPanel = false;
            }
        }
    }

    public void LinkBoss(Creature creature, bool hasMultiplePhases) {
        linkedBoss = creature;
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;
        tryingToShowPanel = true;

        bossHasMultiplePhases = hasMultiplePhases;
        if (bossHasMultiplePhases) {
            bossHealthBarFull.SetActive(false);
            bossHealthBarFirstHalf.SetActive(true);
            bossHealthBarSecondHalf.SetActive(true);
        }
        else {
            bossHealthBarFull.SetActive(true);
            bossHealthBarFirstHalf.SetActive(false);
            bossHealthBarSecondHalf.SetActive(false);
        }
    }

    public void LinkBossMultiple(Creature creature, bool isFirstAppearance) {
        if(isFirstAppearance) {
            LinkBoss(creature, false);
        } else {
            bossHealthBarFull.SetActive(false);
            tryingToShowPanel = true;

            if (linkedBossCount == 0) {
                linkedBoss = creature;
                bossHealthBarFirstHalf.SetActive(true);
                creature.OnMobDamageTaken += Creature_OnMultipleMobDamageTaken;
            }
            else {
                linkedBoss2 = creature;
                bossHealthBarSecondHalf.SetActive(true);
                creature.OnMobDamageTaken += Creature_OnMultipleMobDamageTaken2;
            }

            linkedBossCount++;
        }
    }

    public void Show() {
        bossUIPanel.SetActive(true);
        bossUIPanelAnimator.SetTrigger("Show");
        panelShown = true;
        StartCoroutine(ShowCoroutine());
    }

    public void SetBossName(string name) {
        bossName = name;
    }

    private IEnumerator ShowCoroutine() {
        bossHealthBarFill.fillAmount = 0;
        bossHealthBarPhase1.fillAmount = 0;
        bossHealthBarPhase2.fillAmount = 0;
        yield return new WaitForEndOfFrame();

        if (bossName != "") {
            bossNameText.text = bossName;
        }

        yield return new WaitForSeconds(.1f);
       
        while (bossHealthBarFill.fillAmount < .99f) {
            bossHealthBarFill.fillAmount += .01f;
            bossHealthBarPhase1.fillAmount += .01f;
            bossHealthBarPhase2.fillAmount += .01f;
            yield return new WaitForEndOfFrame();
        }

        bossHealthBarFill.fillAmount = 1f;
        bossHealthBarPhase1.fillAmount = 1f;
        bossHealthBarPhase2.fillAmount = 1f;
    }

    public void Hide() {
        bossUIPanelAnimator.SetTrigger("Hide");
    }

    private void Creature_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        float maxHealth = linkedBoss.GetCreatureMaxHealth();
        float currentHealth = linkedBoss.GetCreatureHealth();
        float healthNormalized = currentHealth / maxHealth;

        if (bossHasMultiplePhases) {
            if (healthNormalized > 0.5f) {
                // Phase 1 encore en cours
                bossHealthBarPhase1.fillAmount = (healthNormalized - 0.5f) / 0.5f; // 100% à 50%
                bossHealthBarPhase2.fillAmount = 1f;
            }
            else {
                // Phase 2 entamée
                bossHealthBarPhase1.fillAmount = 0f;
                bossHealthBarPhase2.fillAmount = healthNormalized / 0.5f; // 50% à 0%
            }
        } else {

            if (linkedBoss.GetCreatureSO().enemyName == "TarnishedWidow") {
                healthNormalized = (healthNormalized - .5f) * 2;
            }

            bossHealthBarFill.fillAmount = healthNormalized;
        }
    }

    private void Creature_OnMultipleMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        float maxHealth = linkedBoss.GetCreatureMaxHealth();
        float currentHealth = linkedBoss.GetCreatureHealth();
        float healthNormalized = currentHealth / maxHealth;

        bossHealthBarPhase1.fillAmount = healthNormalized;
    }

    private void Creature_OnMultipleMobDamageTaken2(object sender, Mob.OnMobDamageTakenEventArgs e) {
        float maxHealth = linkedBoss2.GetCreatureMaxHealth();
        float currentHealth = linkedBoss2.GetCreatureHealth();
        float healthNormalized = currentHealth / maxHealth;

        bossHealthBarPhase2.fillAmount = healthNormalized;
    }
}
