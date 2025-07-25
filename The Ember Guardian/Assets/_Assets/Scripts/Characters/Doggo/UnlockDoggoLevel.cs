using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnlockDoggoLevel : MonoBehaviour
{
    [SerializeField] private Dog.DogType dogType;
    [SerializeField] private SpriteRenderer dogSpriteRenderer;
    [SerializeField] private Image inputIconImage;

    private bool playerInTriggerArea;
    private bool playerUnlockedDog;

    private void Start() {
        GameInput.Instance.OnPlayerInteractPerformed += GameInput_OnPlayerInteractPerformed;

        playerUnlockedDog = DogStats.Instance.GetDogUnlocked(dogType);
        inputIconImage.gameObject.SetActive(false);

        if(playerUnlockedDog) {
            gameObject.SetActive(false);
        }
    }

    private void GameInput_OnPlayerInteractPerformed(object sender, System.EventArgs e) {
        if (!playerInTriggerArea) return;
        if (playerUnlockedDog) return;

        StartCoroutine(UnlockDog());
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.GetComponent<Player>() == null) return;
        if (playerUnlockedDog) return;

        playerInTriggerArea = true;
        Player.Instance.SetInOtherInteractableObjectTriggerArea(true);
        CameraManager.Instance.ZoomIn(false, 1.4f, 1.5f);
        inputIconImage.gameObject.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.GetComponent<Player>() == null) return;
        if (!playerInTriggerArea) return;

        CameraManager.Instance.ZoomOut(true);
        playerInTriggerArea = false;
        Player.Instance.SetInOtherInteractableObjectTriggerArea(false);
        inputIconImage.gameObject.SetActive(false);
    }

    private IEnumerator UnlockDog() {
        Dog.Instance.UnlockDogType(dogType);
        Dog.Instance.SetPosition(transform.position);

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        PetDog.Instance.StartPetDog();
        playerUnlockedDog = true;
        dogSpriteRenderer.enabled = false;
        inputIconImage.gameObject.SetActive(false);
    }

}
