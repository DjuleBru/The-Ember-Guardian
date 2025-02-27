using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelAreaCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            if (EndLevelArea.Instance.GetPlayerDestroyedNest()) return;
            if (EndLevelArea.Instance.AllCreaturesKilled()) return;

            EndLevelArea.Instance.SetPlayerInTriggerArea(true);
            MusicManager.Instance.SetEndLevelMusic(2f);
            MusicManager.Instance.SetAudioTargerVolume(.3f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {

            EndLevelArea.Instance.SetPlayerInTriggerArea(false);
            EndLevelArea.Instance.TryFadeOutMusic();
        }
    }
}
