using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelAreaCollider : MonoBehaviour
{
    private EndLevelArea endLevelArea;
    public static event EventHandler OnPlayerTriggeredInAnyEndLevelArea;

    private void Awake() {
        endLevelArea = GetComponentInParent<EndLevelArea>();
    }


    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            if (endLevelArea.GetPlayerDestroyedNest()) return;
            if (endLevelArea.AllCreaturesKilled()) return;

            OnPlayerTriggeredInAnyEndLevelArea?.Invoke(this, EventArgs.Empty);
            endLevelArea.SetPlayerInTriggerArea(true);
            MusicManager.Instance.SetEndLevelMusic(2f);
            MusicManager.Instance.SetAudioTargerVolume(.3f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {

            endLevelArea.SetPlayerInTriggerArea(false);
            endLevelArea.TryFadeOutMusic();
        }
    }
}
