using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottomBarMenu : MonoBehaviour
{
    public void OpenSteamPage() {
        string url = "https://store.steampowered.com/app/3570060/The_Ember_Guardian/";
        Application.OpenURL(url);
    }

    public void OpenDiscord() {
        string url = "https://discord.gg/DWyznGpwR5";
        Application.OpenURL(url);
    }

    public void OpenBugReportForm() {
        string url = "https://docs.google.com/forms/d/e/1FAIpQLSdmOyCWxbXn1ikaSVR_9YUrNv_UbGLpT-aJQ6YBe4GZulGbEA/viewform?usp=header";
        Application.OpenURL(url);
    }

    public void OpenFeedbackForm() {
        string url = "https://docs.google.com/forms/d/e/1FAIpQLSdnO9PK4LJzSHybBWvU1Mygsf57VN8u3m3_I0eSAU26Fi8IPQ/viewform?usp=header";
        Application.OpenURL(url);
    }
    public void Credits() {
        CreditsManager.Instance.StartShowCredits();
    }

}
