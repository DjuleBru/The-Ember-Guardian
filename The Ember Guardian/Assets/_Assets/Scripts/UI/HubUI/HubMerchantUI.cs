using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HubMerchantUI : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private TextMeshProUGUI merchantNameText;
    [SerializeField] private GameObject firstButtonSelected;

    private Animator hubMerchantUInimator;
    private HubMerchant hubMerchant;

    private void Awake() {
        hubMerchant = GetComponentInParent<HubMerchant>();
        hubMerchantUInimator = GetComponent<Animator>();

        mainPanel.SetActive(false);
    }

    private void Start() {
        hubMerchant.OnPlayerInteractedWithHubMerchant += HubMerchant_OnPlayerInteractedWithHubMerchant;
        hubMerchant.OnPlayerStoppedInteractingWithHubMerchant += HubMerchant_OnPlayerStoppedInteractingWithHubMerchant;

        merchantNameText.text = hubMerchant.GetHubMerchantName();
    }

    private void HubMerchant_OnPlayerStoppedInteractingWithHubMerchant(object sender, System.EventArgs e) {
        hubMerchantUInimator.SetTrigger("Hide");

        EventSystem.current.SetSelectedGameObject(null);
    }

    private void HubMerchant_OnPlayerInteractedWithHubMerchant(object sender, System.EventArgs e) {
        hubMerchantUInimator.SetTrigger("Show");

        EventSystem.current.SetSelectedGameObject(firstButtonSelected);
    }

}
