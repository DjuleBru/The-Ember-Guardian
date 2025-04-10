using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenSteamPageButton : MonoBehaviour
{
    private Button button;

    private void Awake() {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => {
            OpenSteamPage();
        });
    }

    private void OpenSteamPage() {
        string url = "https://store.steampowered.com/app/3570060/The_Ember_Guardian/";
        Application.OpenURL(url);
    }
   
}
