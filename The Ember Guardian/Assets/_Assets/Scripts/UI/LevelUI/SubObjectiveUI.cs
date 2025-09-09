using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SubObjectiveUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI subObjectiveText;
    private LevelUI_ObjectiveUI.SubObjectiveType subObjectiveType;
    private Animator animator;

    private bool completed;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        subObjectiveText.font = LocalizationManager.Instance.GetCurrentFont();
    }

    public void SetSubObjective(LevelUI_ObjectiveUI.SubObjectiveType subObjectiveType) {
        this.subObjectiveType = subObjectiveType;
        subObjectiveText.text = LevelUI_ObjectiveUI.Instance.GetSubObjectiveTextFromType(subObjectiveType);
    }

    public LevelUI_ObjectiveUI.SubObjectiveType GetSubObjectiveType() {
        return subObjectiveType;
    }

    public void SetCompleted() {
        if (completed) return;
        completed = true;
        gameObject.SetActive(true);
        StartCoroutine(SetCompletedObjective());
    }

    public void SetNext(LevelUI_ObjectiveUI.SubObjectiveType subObjectiveType) {
        completed = false;
        this.subObjectiveType = subObjectiveType;
        gameObject.SetActive(true);
        StartCoroutine(SetNextSubObjective(LevelUI_ObjectiveUI.Instance.GetSubObjectiveTextFromType(subObjectiveType)));
    }

    public bool GetCompleted() {
        return completed;
    }

    private IEnumerator SetCompletedObjective() {
        animator.SetTrigger("Hide");
        yield return new WaitForSeconds(1.5f);
        gameObject.SetActive(false);
    }

    private IEnumerator SetNextSubObjective(string nextSubObjective) {
        animator.SetTrigger("Next");
        yield return new WaitForSeconds(1.5f);
        subObjectiveText.text = nextSubObjective;
    }
}
