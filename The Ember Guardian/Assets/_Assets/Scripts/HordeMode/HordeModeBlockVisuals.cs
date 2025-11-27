using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HordeModeBlockVisuals : MonoBehaviour
{

    [SerializeField] private GameObject Ground_VG;
    [SerializeField] private GameObject Ground_LH;
    [SerializeField] private GameObject Ground_CC;
    [SerializeField] private GameObject Ground_FD;

    [SerializeField] private Transform propsParent;
    [SerializeField] private List<GameObject> propsPrefab_VG;
    [SerializeField] private List<GameObject> propsPrefab_LH;
    [SerializeField] private List<GameObject> propsPrefab_FD;
    [SerializeField] private List<GameObject> propsPrefab_CC;

    private void Start() {
        DisableAll();
        SetEnvironment();
    }

    private void DisableAll() {
        Ground_VG.SetActive(false);
        Ground_LH.SetActive(false);
        Ground_CC.SetActive(false);
        Ground_FD.SetActive(false);
    }

    private void SetEnvironment() {
        LevelSO.LevelEnvironment env = HordeModeCustomizationManager.Instance.GetSelectedEnvironment();

        if(env == LevelSO.LevelEnvironment.TheVerdantGraveyard) {
            Ground_VG.SetActive(true);
        }
        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            Ground_LH.SetActive(true);
        }
        if (env == LevelSO.LevelEnvironment.CorruptedCity) {
            Ground_CC.SetActive(true);
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            Ground_FD.SetActive(true);
        }

        ApplyDecor(env);
    }

    private void ApplyDecor(LevelSO.LevelEnvironment env) {
        return;
        List<GameObject> propsList = new List<GameObject>();

        if (env == LevelSO.LevelEnvironment.TheVerdantGraveyard) {
            propsList = propsPrefab_VG;
        }
        if (env == LevelSO.LevelEnvironment.TheLumenHollow) {
            propsList = propsPrefab_LH;
        }
        if (env == LevelSO.LevelEnvironment.CorruptedCity) {
            propsList = propsPrefab_CC;
        }
        if (env == LevelSO.LevelEnvironment.TheFracturedDistrict) {
            propsList = propsPrefab_FD;
        }

        GameObject prop = propsList[UnityEngine.Random.Range(0, propsList.Count)];

        Instantiate(prop, propsParent.transform.position, Quaternion.identity, propsParent);

        if(transform.position.x < 0) {
            Vector3 scale = new Vector3(-1, 1, 1);
            prop.transform.localScale = scale;
        }
    }
}
