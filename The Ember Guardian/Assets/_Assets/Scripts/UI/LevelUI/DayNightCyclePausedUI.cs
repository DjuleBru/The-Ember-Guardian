using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayNightCyclePausedUI : MonoBehaviour
{
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Image image;
    [SerializeField] private Animator animator;

    private void Start() {
        DayNightManager.Instance.OnCyclePaused += DayNightManager_OnCyclePaused;
        DayNightManager.Instance.OnCycleUnpaused += DayNightManager_OnCycleUnpaused;
    }

    private void DayNightManager_OnCycleUnpaused(object sender, System.EventArgs e) {
        image.sprite = playSprite;
        animator.SetTrigger("Hide");
        animator.ResetTrigger("Show");
    }

    private void DayNightManager_OnCyclePaused(object sender, System.EventArgs e) {
        image.sprite = pauseSprite;
        animator.SetTrigger("Show");
        animator.ResetTrigger("Hide");
    }
}
