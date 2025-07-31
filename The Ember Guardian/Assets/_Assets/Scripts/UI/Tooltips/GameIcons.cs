using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameIcons : MonoBehaviour {

    public static GameIcons Instance;

    public Sprite playIcon;
    public Sprite pauseIcon;
    public Sprite bigBlueOrbIcon;
    public Sprite smallBlueOrbIcon;
    public Sprite fuelIcon;
    public Sprite ammoIcon;
    public Sprite blueGemIcon;
    public Sprite redGemIcon;
    public Sprite greenGemIcon;
    public Sprite yellowGemIcon;
    public Sprite purpleGemIcon;
    public Sprite huntingFlagIcon;
    public Sprite tentTPIcon;

    private Dictionary<string, Sprite> iconDictionary;

    private void Awake() {
        Instance = this;

        iconDictionary = new Dictionary<string, Sprite>
        {
            { "play", playIcon },
            { "pause", pauseIcon },
            { "bigBlueOrb", bigBlueOrbIcon },
            { "smallBlueOrb", smallBlueOrbIcon },
            { "fuel", fuelIcon },
            { "ammo", ammoIcon },
            { "blueGem", blueGemIcon },
            { "redGem", redGemIcon },
            { "yellowGem", yellowGemIcon },
            { "greenGem", greenGemIcon },
            { "purpleGem", purpleGemIcon },
            { "huntingFlag", huntingFlagIcon },
            { "tentTPIcon", tentTPIcon },
        };
    }

    public Dictionary<string, Sprite> GetIconDictionary() {
        return iconDictionary;
    }
}
