using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class PerspectiveObject : MonoBehaviour {
    [Header("Perspective settings")]
    static float scaleFactor = 0.5f;      // Amplifie la taille en bas
    static float parallaxFactor = 1f;   // Amplifie le déplacement inverse
    static float parallaxPower = .85f;             
    float Y0 = -1.5f;                 // Niveau du joueur
    float Ymax = -10f;             // Limite "profonde"

    [Header("References")]
    [SerializeField] Transform perspectiveReferencePoint;
    [SerializeField] SpriteRenderer spriteRenderer; 
    [SerializeField] SpriteRenderer foliageSpriteRenderer; 
    [SerializeField] SpriteRenderer reflectionSpriteRenderer; 
    [SerializeField] SpriteRenderer foamSpriteRenderer;

    Transform cameraTransform;
    float sortingPrecision = 1000f; // Plus grand = plus de finesse (ex: 100 = 1 unité Y = 100 levels)
    int baseSortingOrder = -1;
    float depth;
    Vector3 baseScale;
    Vector3 lastCameraPos;
    Vector3 offset; // Décalage accumulé par le parallax

    void Start() {
        baseScale = transform.localScale;
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPos = cameraTransform.position;

        // --- 1. Calcule la profondeur normalisée ---
        depth = Mathf.InverseLerp(Y0, Ymax, perspectiveReferencePoint.position.y);
        depth = Mathf.Pow(depth, parallaxPower); // courbe douce

        ApplyPerspectiveScale();
        UpdateSortingOrder();
    }

    void LateUpdate() {
        ApplyPerspective();
        ApplyPerspectiveScale();
    }

    void ApplyPerspective() {
        // --- 3. Déplace en réaction au mouvement de la caméra ---
        Vector3 camDelta = cameraTransform.position - lastCameraPos;
        offset -= camDelta * (depth * parallaxFactor);
        transform.position += offset;
        offset = Vector3.zero; // Réinitialise pour éviter l'accumulation

        lastCameraPos = cameraTransform.position;
    }

    void ApplyPerspectiveScale() {
        // --- 2. Ajuste la taille ---
        float scale = 1f + depth * scaleFactor;
        transform.localScale = baseScale * scale;
    }

    void UpdateSortingOrder() {
        // Plus un sprite est bas, plus il doit être "devant"
        int order = baseSortingOrder - Mathf.RoundToInt(spriteRenderer.transform.position.y * sortingPrecision);
        spriteRenderer.sortingOrder = order -1;

        if (foliageSpriteRenderer != null) {
            foliageSpriteRenderer.sortingOrder = order;
        }
        if (reflectionSpriteRenderer != null) {
            reflectionSpriteRenderer.sortingOrder = order - 3;
        }
        if (foamSpriteRenderer != null) {
            foamSpriteRenderer.sortingOrder = order - 22;
        }

    }

    [Button] public void SetScaleFactor(float newScaleFactor) {
        scaleFactor = newScaleFactor;
    }
    [Button]
    public void SetParallaxFactor(float newparallaxFactor) {
        parallaxFactor = newparallaxFactor;
    }
    [Button]
    public void SetParallaxPower(float newparallaxPower) {
        parallaxPower = newparallaxPower;
    }
}
