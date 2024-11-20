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

    public LevelSO GetLevelSO() {
        return levelSO;
    }
}
