using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private LevelSO levelSO;


    private void Awake() {
        Instance = this;
    }

    public void ShowNewLocationUI() {
        bool levelRegionUnlocked = MetaProgressionManager.Instance.GetLevelRegionUnlocked(levelSO.environmentType);
        if (!levelRegionUnlocked) {
            MetaProgressionManager.Instance.SetLevelRegionUnlocked(levelSO.environmentType);
            LevelUI_Locations.Instance.ShowLocationText(levelSO.GetLevelEnvironmentTypeString());
        }
    }

    public LevelSO GetLevelSO() {
        return levelSO;
    }

    public void LooseLevel() {
        StartCoroutine(LooseLevelCoroutine());
    }

    private IEnumerator LooseLevelCoroutine() {
        yield return new WaitForSeconds(2f);
        SceneLoader.Instance.LoadHub(3f);
    }
}
