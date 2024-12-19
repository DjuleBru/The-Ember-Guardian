using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunConeVisual : MonoBehaviour
{
    [SerializeField] private ParticleSystem bulletPS;
    private Animator coneVisualAnimator;

    public float angle = 45f; // Angle total du cône
    public float range = 5f;  // Portée du cône
    public int segments = 50; // Résolution du cône

    private float baseWidthScale;   // Facteur d'échelle initial en X
    private float baseHeightScale;  // Facteur d'échelle initial en Y
    private float widthScaleFactor;   // Facteur d'échelle initial en X
    private float heightScaleFactor;  // Facteur d'échelle initial en Y

    void Awake() {
        coneVisualAnimator = GetComponent<Animator>();
        // Enregistre les échelles initiales
        baseWidthScale = transform.localScale.x;
        baseHeightScale = transform.localScale.y;
    }

    private void Start() {
        InitializeConeParameters();
        UpdateCone();

        PlayerAim.Instance.OnPlayerAimSightEnded += PlayerAIm_OnPlayerAimSightEnded;
        PlayerAim.Instance.OnPlayerAimSightStarted += PlayerAIm_OnPlayerAimSightStarted;
        PlayerShoot.Instance.OnPlayerOverclockedSMGStarted += PlayerShoot_OnPlayerOverclockedSMGStarted;
        PlayerShoot.Instance.OnPlayerOverclockedSMGStopped += PlayerShoot_OnPlayerOverclockedSMGStopped;
        PlayerShoot.Instance.OnPlayerFocusBlastStarted += PlayerShoot_OnPlayerFocusBlastStarted;
        PlayerShoot.Instance.OnPlayerFocusBlastStopped += PlayerShoot_OnPlayerFocusBlastStopped;
    }

    private void PlayerShoot_OnPlayerFocusBlastStopped(object sender, System.EventArgs e) {
        coneVisualAnimator.SetTrigger("SkipHide");
        coneVisualAnimator.SetBool("ShowThenHide", false);
    }

    private void PlayerShoot_OnPlayerFocusBlastStarted(object sender, System.EventArgs e) {
        coneVisualAnimator.SetBool("ShowThenHide", true);
    }

    private void PlayerShoot_OnPlayerOverclockedSMGStopped(object sender, System.EventArgs e) {
        coneVisualAnimator.SetBool("ShowThenHide", false);

    }

    private void PlayerShoot_OnPlayerOverclockedSMGStarted(object sender, System.EventArgs e) {
        coneVisualAnimator.SetBool("ShowThenHide", true);
    }

    private void PlayerAIm_OnPlayerAimSightStarted(object sender, System.EventArgs e) {
        coneVisualAnimator.SetBool("Aiming", true);
    }

    private void PlayerAIm_OnPlayerAimSightEnded(object sender, System.EventArgs e) {
        coneVisualAnimator.SetBool("Aiming", false);
    }

    private void Update() {
        UpdateCone();
    }

    public void InitializeConeParameters() {
        // Récupère l'angle du ShapeModule
        var shape = bulletPS.shape;
        angle = shape.angle;

        // Récupère la portée en fonction de la vitesse et de la durée de vie des particules
        var main = bulletPS.main;
        float startSpeed = main.startSpeed.constant;
        float startLifetime = main.startLifetime.constant;
        range = startSpeed * startLifetime;

        // Calcul de la largeur et de la hauteur
        float width = range; // Hauteur = portée
        float height = 2 * Mathf.Tan(Mathf.Deg2Rad * (angle / 2)) * range; // Largeur calculée via l'angle

        widthScaleFactor = width / baseWidthScale;
        heightScaleFactor = height / baseHeightScale;

    }

    public void UpdateCone() {

        // Récupère l'angle du ShapeModule
        var shape = bulletPS.shape;
        angle = shape.angle;

        // Récupère la portée en fonction de la vitesse et de la durée de vie des particules
        var main = bulletPS.main;
        float startSpeed = main.startSpeed.constant;
        float startLifetime = main.startLifetime.constant;
        range = startSpeed * startLifetime;

        // Calcul de la largeur et de la hauteur
        float width = range; // Hauteur = portée
        float height = 2 * Mathf.Tan(Mathf.Deg2Rad * (angle / 2)) * range; // Largeur calculée via l'angle

        // Ajuste l'échelle locale pour maintenir les proportions de base
        transform.localScale = new Vector3(
            width / widthScaleFactor,  // Ajuste la largeur en tenant compte de l'échelle de base
            height / heightScaleFactor, // Ajuste la hauteur en tenant compte de l'échelle de base
            1f
        );
    }
}
