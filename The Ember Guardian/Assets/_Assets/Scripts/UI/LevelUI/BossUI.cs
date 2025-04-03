using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{

    public static BossUI Instance;
    [SerializeField] private GameObject bossUIPanel;
    [SerializeField] private Animator bossUIPanelAnimator;
    [SerializeField] private Image bossHealthBarFill;
    private Creature linkedBoss;

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

    public void LinkBoss(Creature creature) {
        linkedBoss = creature;
        creature.OnMobDamageTaken += Creature_OnMobDamageTaken;
        tryingToShowPanel = true;
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
        float healthNormalized = (float)linkedBoss.GetCreatureHealth() / (float)linkedBoss.GetCreatureMaxHealth();

        bossHealthBarFill.fillAmount = healthNormalized;
        Debug.Log("healthNormalized" + healthNormalized);
    }
}
