using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FastTravelTPUI : MonoBehaviour
{
    [SerializeField] private FastTravelTP fastTravelTP;

    [SerializeField] private List<Sprite> fastTravelTPIdentifiers;
    [SerializeField] private Sprite tentTravelTPIdentifier;
    [SerializeField] private GameObject changeDestinationTPUIGO;
    [SerializeField] private GameObject instructionsGO;
    [SerializeField] private GameObject setpOnTPGO;
    [SerializeField] private GameObject leftArrowGO;
    [SerializeField] private GameObject righArrowGO;
    [SerializeField] private GameObject leftInputGO;
    [SerializeField] private GameObject righInputGO;
    [SerializeField] private Image identifierIcon;

    private Sprite identifier;

    private void Start() {
        fastTravelTP.OnPlayerPositionedOnTP += FastTravelTP_OnPlayerPositionedOnTP;
        fastTravelTP.OnReceiverFastTravelTPChanged += FastTravelTP_OnReceiverFastTravelTPChanged;
        fastTravelTP.OnPlayerWarpStarted += FastTravelTP_OnPlayerWarpStarted;
        fastTravelTP.OnPlayerCanceledTP += FastTravelTP_OnPlayerCanceledTP;
        fastTravelTP.OnPlayerWarped += FastTravelTP_OnPlayerWarped;
        fastTravelTP.OnPlayerWarpEnded += FastTravelTP_OnPlayerWarpEnded;
        fastTravelTP.OnPlayerWarpedOut += FastTravelTP_OnPlayerWarpedOut;
        fastTravelTP.OnPlayerTriggeredOut += FastTravelTP_OnPlayerTriggeredOut;
        fastTravelTP.OnTPSetAsReceiver += FastTravelTP_OnTPSetAsReceiver;
        fastTravelTP.OnPlayerTriggeredIn += FastTravelTP_OnPlayerTriggeredIn;

        changeDestinationTPUIGO.SetActive(false);
        instructionsGO.SetActive(false);
        leftArrowGO.SetActive(false);
        righArrowGO.SetActive(false);

        if(!fastTravelTP.GetIsTentTP()) {
            identifier = fastTravelTPIdentifiers[fastTravelTP.GetFastTravelTPIdentifier()];
            identifierIcon.sprite = identifier;
        } else {
            identifierIcon.sprite = tentTravelTPIdentifier;
        }

    }

    private void FastTravelTP_OnPlayerWarpEnded(object sender, EventArgs e) {
        identifierIcon.sprite = identifier;
    }

    private void FastTravelTP_OnTPSetAsReceiver(object sender, EventArgs e) {
        setpOnTPGO.SetActive(false);
    }

    private void FastTravelTP_OnPlayerTriggeredOut(object sender, EventArgs e) {
        setpOnTPGO.SetActive(true);
        if(fastTravelTP.GetIsTentTP()) {
            identifierIcon.gameObject.SetActive(false);
        }
    }
    private void FastTravelTP_OnPlayerTriggeredIn(object sender, EventArgs e) {
        identifierIcon.gameObject.SetActive(true);
    }


    private void FastTravelTP_OnPlayerWarped(object sender, EventArgs e) {
        setpOnTPGO.SetActive(false);
    }

    private void FastTravelTP_OnPlayerWarpedOut(object sender, EventArgs e) {
        setpOnTPGO.SetActive(false);
    }

    private void FastTravelTP_OnPlayerWarpStarted(object sender, EventArgs e) {
        instructionsGO.SetActive(false);
        changeDestinationTPUIGO.SetActive(false);
    }

    private void FastTravelTP_OnPlayerCanceledTP(object sender, EventArgs e) {
        instructionsGO.SetActive(false);
        changeDestinationTPUIGO.SetActive(false);
        setpOnTPGO.SetActive(true);
        identifierIcon.sprite = identifier;
    }

    private void FastTravelTP_OnReceiverFastTravelTPChanged(object sender, EventArgs e) {
        FastTravelTP receiver = fastTravelTP.GetReceiverFastTravelTP();
        var availability = fastTravelTP.GetLeftRightAvailability();

        leftInputGO.SetActive(availability.canGoLeft);
        righInputGO.SetActive(availability.canGoRight);

        float direction = receiver.transform.position.x - fastTravelTP.transform.position.x;
        leftArrowGO.SetActive(direction < 0);
        righArrowGO.SetActive(direction > 0);

        identifierIcon.sprite = fastTravelTPIdentifiers[receiver.GetFastTravelTPIdentifier()];
    }

    private void FastTravelTP_OnPlayerPositionedOnTP(object sender, System.EventArgs e) {
        changeDestinationTPUIGO.SetActive(true);
        instructionsGO.SetActive(true);
        setpOnTPGO.SetActive(false);
    }
}
