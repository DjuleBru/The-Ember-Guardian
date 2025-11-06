using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamIntegrationInitializer : MonoBehaviour
{
    void Start()
    {
        try {
            Steamworks.SteamClient.Init(3570060);
            PrintYourName();
        } catch(System.Exception e) {
            Debug.Log(e);
        }
    }

    private void PrintYourName() {
        Debug.Log(Steamworks.SteamClient.Name);
    }

}
