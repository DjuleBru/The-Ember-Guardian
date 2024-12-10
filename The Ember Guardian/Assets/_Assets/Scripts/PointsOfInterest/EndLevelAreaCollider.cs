using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLevelAreaCollider : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.GetComponent<Player>() != null) {
            MusicManager.Instance.SetEndLevelMusic();
            MusicManager.Instance.SetAudioTargerVolume(.3f);
            MusicManager.Instance.FadeInMusic(3f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.GetComponent<Player>() != null) {
            if(MusicManager.Instance != null) {
                MusicManager.Instance.FadeOutMusic(3f);
            }
        }
    }
}
