using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCollectible : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    [SerializeField] private ParticleSystem instantiatePS;
    [SerializeField] private ParticleSystem reachedDestinationPS;
    private Transform destinationTransform;

    private float minSpeed = .1f;
    private float maxSpeed = 35f;
    private float maxDistance; // distance à partir de laquelle on a la vitesse max

    private bool reachedDestination;

    public event EventHandler OnDestinationReached;

    private void Update() {
        if (destinationTransform == null) return;
        if (reachedDestination) return;
        HandleMoveToDestination();

        if(Vector3.Distance(transform.position, destinationTransform.position) < .25f) {
            ApplyReachedDestinationEffect();
        }
    }

    public void SetDestination(Transform destinationTransform) {
        this.destinationTransform = destinationTransform;
        maxDistance = Vector3.Distance(transform.position, destinationTransform.position) * 2.5f;
        instantiatePS.Play();

        maxSpeed = UnityEngine.Random.Range(maxSpeed - maxSpeed/5, maxSpeed + maxSpeed/5);
    }

    public virtual void ApplyReachedDestinationEffect() {
        OnDestinationReached?.Invoke(this, EventArgs.Empty);
        reachedDestination = true;
        reachedDestinationPS.Play();
        visual.gameObject.SetActive(false);

        StartCoroutine(DestroyGameObjectAfterDelay());
    }

    private IEnumerator DestroyGameObjectAfterDelay() {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
    private void HandleMoveToDestination() {
        float distance = Vector3.Distance(transform.position, destinationTransform.position);

        float t = Mathf.Clamp01(distance / maxDistance); // t = 1 quand loin, 0 quand proche
        float invertedT = 1f - t; // 0 quand loin, 1 quand proche
        float sharpT = Mathf.Pow(invertedT, 4f); // explosion quand on approche


        float currentSpeed = Mathf.Lerp(minSpeed, maxSpeed, sharpT);

        Vector3 direction = (destinationTransform.position - transform.position).normalized;
        transform.position += direction * currentSpeed * Time.deltaTime;
    }


}
