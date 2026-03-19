using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Steamworks;

public class SteamOverlayDetector : MonoBehaviour {
    public static SteamOverlayDetector Instance;

    protected bool initialized;
    protected bool overlayOpen;
    protected bool openedPauseFromOverlay;

    protected void Awake() {
        Instance = this;


    }

    protected void Update() {

        if (initialized) return;

        if (SteamClient.IsValid) {

            SteamFriends.OnGameOverlayActivated += OnGameOverlayActivated;

            initialized = true;

            Debug.Log("SteamOverlayDetector initialized");
        }
    }

    private void OnGameOverlayActivated(bool active) {

        if (active) {

            OnOverlayOpened();

        }
        else {

            OnOverlayClosed();
        }
    }

    protected void OnOverlayOpened() {
        Debug.Log("OnOverlayOpened");
        overlayOpen = true;

        // Bloque les inputs
        if (GameInput.Instance != null) {
            GameInput.Instance.enabled = false;
        }

        if (EventSystem.current != null) {
            EventSystem.current.sendNavigationEvents = false;
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Ouvre le pause menu si besoin
        if (PauseMenuUI.Instance != null) {

            if (!PauseMenuUI.Instance.isPaused) {

                PauseMenuUI.Instance.ShowPauseMenu(true);
                openedPauseFromOverlay = true;

            }
            else {

                openedPauseFromOverlay = false;
            }
        }
    }

    protected void OnOverlayClosed() {

        overlayOpen = false;

        // Réactive les inputs
        if (GameInput.Instance != null) {
            GameInput.Instance.enabled = true;
        }

        if (EventSystem.current != null) {
            EventSystem.current.sendNavigationEvents = true;
        }

        // Ferme le pause menu UNIQUEMENT si c’est nous qui l’avons ouvert
        if (PauseMenuUI.Instance != null) {

            if (openedPauseFromOverlay) {

                PauseMenuUI.Instance.ShowPauseMenu(false);
            }
        }
    }

    public bool IsOverlayOpen() {
        return overlayOpen;
    }

    private void OnDestroy() {
        if (SteamClient.IsValid) {

            SteamFriends.OnGameOverlayActivated -= OnGameOverlayActivated;
        }
    }
}