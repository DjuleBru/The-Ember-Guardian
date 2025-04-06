using Febucci.UI.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WorkerUI : MonoBehaviour
{
    [SerializeField] private Worker worker;
    [SerializeField] private WorkerAI workerAI;
    [SerializeField] private WorkerInteractionCollider interactionCollider;
    [SerializeField] private TextMeshProUGUI talkText;
    private HunterJob hunterJob;
    private float showStatusText;

    private bool hideTextOnTriggerExit;

    private bool hunterFoundAnimal;
    private bool workerBlockedByCreatures;

    private void Awake() {
        hunterJob = worker.GetComponent<HunterJob>();

        hunterJob.OnHunterFindsNoAnimal += HunterJob_OnHunterFindsNoAnimal;
        hunterJob.OnHunterChangedState += HunterJob_OnHunterChangedState;
        hunterJob.OnHunterFoundAnimal += HunterJob_OnHunterFoundAnimal;

        DayNightManager.Instance.OnDuskStart += DayNightManager_OnDuskStart;
    }


    private void Start() {
        talkText.text = "";
        interactionCollider.OnPlayerTriggeredIn += InteractionCollider_OnPlayerTriggeredIn;
        interactionCollider.OnPlayerTriggeredOut += InteractionCollider_OnPlayerTriggeredOut;
        workerAI.OnJobChanged += WorkerAI_OnJobChanged;
    }

    private void DayNightManager_OnDuskStart(object sender, System.EventArgs e) {
        if (!worker.GetRecruited()) return;
        int randomized = Random.Range(0, 10);
        if(randomized <= 4) {
            StartCoroutine(ShowDuskStarTextAfterDelay(1f));
        }
    }

    private IEnumerator ShowDuskStarTextAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        int randomized = Random.Range(0, 16);
        string localizationKey = "worker_duskStart" + randomized;

        talkText.text = LocalizationManager.Instance.GetLocalizedText(localizationKey);
        StartCoroutine(HideTextAfterDelay(3f));
    }

    private void WorkerAI_OnJobChanged(object sender, System.EventArgs e) {

        if (workerAI.GetJob() == WorkerAI.JobTypes.jobless) {
            int textRandomized = UnityEngine.Random.Range(1, 16);
            string localizationKey = "worker_recruited" + textRandomized;
            talkText.text = LocalizationManager.Instance.GetLocalizedText(localizationKey);
        }
        if (workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            int textRandomized = UnityEngine.Random.Range(1, 11);
            string localizationKey = "worker_assignedHunter" + textRandomized;
            talkText.text = LocalizationManager.Instance.GetLocalizedText(localizationKey);
        }

        StartCoroutine(HideTextAfterDelay(2f));
    }

    private void InteractionCollider_OnPlayerTriggeredOut(object sender, System.EventArgs e) {
        if (!hideTextOnTriggerExit) return;
        talkText.text = "";
    }

    private void InteractionCollider_OnPlayerTriggeredIn(object sender, System.EventArgs e) {

        if (workerAI.GetJob() == WorkerAI.JobTypes.hunter) {
            if (workerBlockedByCreatures) {
                string localizationKey = "worker_blockedByCreatures";
                talkText.text = LocalizationManager.Instance.GetLocalizedText(localizationKey);
                hideTextOnTriggerExit = true;
            }

            if (!hunterFoundAnimal) {
                string localizationKey = "worker_noMoreAnimals";
                talkText.text = LocalizationManager.Instance.GetLocalizedText(localizationKey);
                hideTextOnTriggerExit = true;
            }
        }
    }

    private void HunterJob_OnHunterFindsNoAnimal(object sender, System.EventArgs e) {
        if (hunterFoundAnimal) {
            hunterFoundAnimal = false;
        }
    }

    private void HunterJob_OnHunterFoundAnimal(object sender, System.EventArgs e) {
        if (!hunterFoundAnimal) {
            hunterFoundAnimal = true;
        }
    }

    private void HunterJob_OnHunterChangedState(object sender, System.EventArgs e) {
        HunterJob.HunterState state = hunterJob.GetState();

        if (state == HunterJob.HunterState.blockedByCreatures) {
            if (!workerBlockedByCreatures) {
                workerBlockedByCreatures = true;
            }
        }
        else {
            if (workerBlockedByCreatures) {
                workerBlockedByCreatures = false;
            }
        }

        if (state == HunterJob.HunterState.headingToGuard) {
            hunterFoundAnimal = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() == null) return;

    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (!hideTextOnTriggerExit) return;
        if (collision.gameObject.GetComponent<Player>() == null) return;

        hideTextOnTriggerExit = false;
        talkText.text = "";
    }

    private IEnumerator HideTextAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        talkText.text = "";
    }
}
