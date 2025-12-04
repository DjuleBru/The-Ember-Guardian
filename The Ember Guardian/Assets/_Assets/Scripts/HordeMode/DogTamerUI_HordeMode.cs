using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DogTamerUI_HordeMode : MonoBehaviour
{
    [SerializeField] private HubMerchant hubMerchant;
    [SerializeField] private GameObject germanShepherdItemGO;
    [SerializeField] private GameObject germanShepherdItemListGO;
    [SerializeField] private GameObject retreiverItemsGO;
    [SerializeField] private GameObject retreiverItemListGO;
    [SerializeField] private GameObject darkCompanionItemsGO;
    [SerializeField] private GameObject darkCompanionItemListGO;

    private GameObject firstSelectedButtonGO;

    private void Start() {
        hubMerchant.OnPlayerOpenedHubMerchantShop += HubMerchant_OnPlayerOpenedHubMerchantShop;

        germanShepherdItemGO.SetActive(false);
        retreiverItemsGO.SetActive(false);
        darkCompanionItemsGO.SetActive(false);
        germanShepherdItemListGO.SetActive(false);
        retreiverItemListGO.SetActive(false);
        darkCompanionItemListGO.SetActive(false);

        Dog.DogType dogType = HordeModeCustomizationManager.Instance.GetSelectedDogType();

        if(dogType == Dog.DogType.GermanShepherd) {
            germanShepherdItemGO.SetActive(true);
            germanShepherdItemListGO.SetActive(true);
            firstSelectedButtonGO = germanShepherdItemGO;
        }
        if (dogType == Dog.DogType.GoldenRetreiver) {
            retreiverItemsGO.SetActive(true);
            retreiverItemListGO.SetActive(true);
            firstSelectedButtonGO = retreiverItemsGO;
        }
        if (dogType == Dog.DogType.DarkCompanion) {
            darkCompanionItemsGO.SetActive(true);
            darkCompanionItemListGO.SetActive(true);
            firstSelectedButtonGO = darkCompanionItemsGO;
        }
    }

    private void HubMerchant_OnPlayerOpenedHubMerchantShop(object sender, System.EventArgs e) {
        StartCoroutine(SetFirstSelectedGOAfterDelay());
    }

    private IEnumerator SetFirstSelectedGOAfterDelay() {
        yield return new WaitForEndOfFrame();
        if(GameInput.Instance.IsUsingGamepad()) {
            EventSystem.current.SetSelectedGameObject(firstSelectedButtonGO);
        }
    }
}
