using UnityEngine;

public class FlameSegment : MonoBehaviour {

    public int segmentIndex = 0;          // Ordre du segment (0 = base, 3 = pointe)
    public float maxOffsetY = -0.3f;      // Jusqu’où le sprite peut "pendre"
    public float recoverSpeed = 5f;       // Vitesse à laquelle le sprite revient à y=0

    private float currentOffsetY = 0f;

    void Update() {
    }
}

