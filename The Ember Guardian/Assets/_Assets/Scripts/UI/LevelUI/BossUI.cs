using System.Collections;
using System.Collections.Generic;
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
    private Creature linkedBoss;

    private bool bossHasMultiplePhases;
    private bool tryingToShowPanel;

    private void Awake() {
        Instance = this;
        bossUIPanel.SetActive(false);

    }

    private void Update() {
        if(tryingToShowPanel) {
            float distanceToPlayer = Mathf.Abs(linkedBoss.transform.position.x - Player.Instance.transform.position.x);
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

    public void Show() {
        bossUIPanel.SetActive(true);
        bossUIPanelAnimator.SetTrigger("Show");
        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine() {
        yield return new WaitForSeconds(1f);
        while (bossHealthBarFill.fillAmount < .99f) {
            bossHealthBarFill.fillAmount += .1f;
            yield return new WaitForEndOfFrame();
        }
        bossHealthBarFill.fillAmount = 1f;
    }

    public void Hide() {
        bossUIPanelAnimator.SetTrigger("Hide");
    }

    private void Creature_OnMobDamageTaken(object sender, Mob.OnMobDamageTakenEventArgs e) {
        float maxHealth = linkedBoss.GetCreatureMaxHealth();
        float currentHealth = linkedBoss.GetCreatureHealth();
        float healthNormalized = currentHealth / maxHealth;

        if(bossHasMultiplePhases) {
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
            bossHealthBarFill.fillAmount = healthNormalized;
        }
    }
}
