using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveTeleportation : MonoBehaviour
{
    public static ActiveTeleportation Instance;

    [SerializeField] private LayerMask obstacleLayer; // Masque des obstacles à vérifier
    [SerializeField] private GameObject teleportRayPrefab; // Masque des obstacles à vérifier

    private float delayToTeleportPlayer = 2f;
    private float teleportDistance = 15f;
    private float teleportDuration= .2f;

    public event EventHandler OnPlayerTeleportStarted;
    public event EventHandler OnPlayerTeleported;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        PlayerSkills.Instance.OnActiveSkillActivated += PlayerSkills_OnActiveSkillActivated;
    }

    private void PlayerSkills_OnActiveSkillActivated(object sender, PlayerSkills.OnSkillAddedEventArgs e) {
        if (e.skillItemAdded.skillType == SkillItem.SkillType.activeTeleportation) {
            teleportDistance = e.skillItemAdded.skillSO.activeSkillEffect.GetValueAtLevel(e.skillItemAdded.currentLevel);
            StartCoroutine(TeleportPlayer());
        }
    }

    [Button]
    public void TeleportPlayerDebug() {
        teleportDistance = 7f;
        StartCoroutine(TeleportPlayer());
    }

    private IEnumerator TeleportPlayer() {
        OnPlayerTeleportStarted?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(delayToTeleportPlayer);
        OnPlayerTeleported?.Invoke(this, EventArgs.Empty);

        // Calcule la position cible
        Vector2 teleportTarget = CalculateTeleportTarget();

        // Affiche un rayon ou une traînée entre le point de départ et d’arrivée
        Vector2 teleportRayTarget = new Vector2(teleportTarget.x, teleportRayPrefab.transform.position.y);
        Debug.Log("teleportRayTarget " + teleportRayTarget);
        Vector2 teleportStartPosition = new Vector2(Player.Instance.transform.position.x, teleportRayPrefab.transform.position.y);
        ShowTeleportRay(teleportStartPosition, teleportRayTarget);

        Player.Instance.transform.position = teleportTarget;
        // Interpole la position du joueur vers la cible
        //yield return StartCoroutine(MoveToPosition(teleportTarget, teleportDuration));
    }

    private Vector2 CalculateTeleportTarget() {
        // Récupère la direction du joueur (face à gauche ou droite)
        float direction = 1f;
        
        if(PlayerAim.Instance.GetAimDir().x <0) {
            direction = -1f;
        };


        // Calcule la position cible basée sur la direction et la distance
        Vector2 targetPosition = (Vector2)transform.position + new Vector2(direction * teleportDistance, 0);

        // Vérifie s'il y a un obstacle à la position cible
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right * direction, teleportDistance, obstacleLayer);

        if (hit.collider != null) {
            // Ajuste la position cible pour éviter l'obstacle
            targetPosition = hit.point;
        }

        return targetPosition;
    }

    private IEnumerator MoveToPosition(Vector2 targetPosition, float duration) {
        Vector2 startPosition = Player.Instance.transform.position; // Position de départ
        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            // Interpolation linéaire entre la position actuelle et la cible
            Player.Instance.transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // Attend le prochain frame
        }

        // Assure que la position finale est exactement la cible
        Player.Instance.transform.position = targetPosition;
    }
    private void ShowTeleportRay(Vector2 startPoint, Vector2 endPoint) {
        GameObject teleportRay = Instantiate(teleportRayPrefab, Vector3.zero, Quaternion.identity);
        LineRenderer lineRenderer = teleportRay.GetComponent<LineRenderer>();

        Debug.Log("startPoint " + startPoint);
        Debug.Log("endPoint " + endPoint);
        if (lineRenderer != null) {
            lineRenderer.SetPosition(0, startPoint); // Définir le point de départ
            lineRenderer.SetPosition(1, endPoint);   // Définir le point d'arrivée
        }

        // Détruire le rayon après une courte durée
        Destroy(teleportRay, 0.2f);
    }

}
