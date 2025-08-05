using UnityEngine;

public class FlameController : MonoBehaviour {
    public Transform[] segments;        // assignés dans l'ordre (de la base vers le bout)
    public Transform firePoint;         // point de départ du tir
    public Transform aimTarget;         // position du curseur ou direction de visée
    public float segmentSpacing = 0.5f; // distance entre chaque segment
    public float followSpeed = 15f;     // vitesse de suivi (plus bas = plus traînant)

    private Vector3[] segmentPositions;

    void Start() {
        segmentPositions = new Vector3[segments.Length];
        for (int i = 0; i < segments.Length; i++) {
            segmentPositions[i] = Vector3.zero;
        }
    }

    void Update() {
        Vector3 aimDirection = PlayerAim.Instance.GetAimDir();

        // Position du premier segment : toujours collé à firePoint
        segmentPositions[0] = Vector3.zero;
        segments[0].localPosition = segmentPositions[0];

        for (int i = 1; i < segments.Length; i++) {
            // Position cible du segment i : espacé dans la direction du tir
            Vector3 targetLocalPos = aimDirection * segmentSpacing * i;

            // Lissage vers cette position pour simuler une traînée
            segmentPositions[i] = Vector3.Lerp(segmentPositions[i], targetLocalPos, Time.deltaTime * followSpeed);

            // Application
            segments[i].localPosition = segmentPositions[i];
        }
    }
}
