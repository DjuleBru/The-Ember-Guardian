using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownMenuUI : MonoBehaviour {
    [SerializeField] private TMP_Dropdown dropdown;

    private List<Resolution> resolutions = new List<Resolution>();

    void Start() {
        InitResolutions();
    }

    void InitResolutions() {
        dropdown.ClearOptions();
        resolutions.Clear();

        Dictionary<(int, int), Resolution> uniqueResolutions =
            new Dictionary<(int, int), Resolution>();

        foreach (Resolution res in Screen.resolutions) {
            var key = (res.width, res.height);

            if (!uniqueResolutions.ContainsKey(key) ||
                res.refreshRateRatio.value > uniqueResolutions[key].refreshRateRatio.value) {
                uniqueResolutions[key] = res;
            }
        }

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        foreach (Resolution res in uniqueResolutions.Values) {
            resolutions.Add(res);
            options.Add($"{res.width} x {res.height}");

            if (res.width == Screen.width && res.height == Screen.height) {
                currentResolutionIndex = resolutions.Count - 1;
            }
        }

        dropdown.AddOptions(options);
        dropdown.value = currentResolutionIndex;
        dropdown.RefreshShownValue();

        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int index) {
        if (index < 0 || index >= resolutions.Count)
            return;

        Resolution res = resolutions[index];

        SettingsManager.Instance.SetResolution(res);
    }
}
