using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogCurrenciesDetectionCollider : MonoBehaviour
{
    private List<Collectible> collectiblesInTriggerArea = new List<Collectible>();
    private Collectible closestCollectibleToCollect;

    private void Update() {
        CheckClosestCollectibleToCollect();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        Collectible collectible = collision.GetComponent<Collectible>();

        if(collectible != null && !collectiblesInTriggerArea.Contains(collectible)) {

            collectiblesInTriggerArea.Add(collectible);

        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        Collectible collectible = collision.GetComponent<Collectible>();
        if (collectible != null && collectiblesInTriggerArea.Contains(collectible)) {

            collectiblesInTriggerArea.Remove(collectible);

        }
    }

    public Collectible GetClosestCollectibleToCollect() {
        return closestCollectibleToCollect;
    }

    public void CheckClosestCollectibleToCollect() {
        float closestDistance = Mathf.Infinity;

        foreach (Collectible collectible in collectiblesInTriggerArea) {
            if (!collectible.GetCanBePickedUpByWorker()) continue;
            float distanceToDog = Mathf.Abs(transform.position.x - collectible.transform.position.x);
            float distanceToPlayer = Mathf.Abs(Player.Instance.transform.position.x - collectible.transform.position.x);

            if (distanceToPlayer < 5f) continue;

            if (distanceToDog < closestDistance) {
                closestDistance = distanceToDog;
                closestCollectibleToCollect = collectible;
            }
        }

    }

}
