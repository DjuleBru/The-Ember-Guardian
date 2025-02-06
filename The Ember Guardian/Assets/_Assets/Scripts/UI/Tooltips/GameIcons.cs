using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameIcons : MonoBehaviour {

    public static GameIcons Instance;

    public Sprite playIcon;
    public Sprite pauseIcon;
    public Sprite bigBlueOrbIcon;
    public Sprite smallBlueOrbIcon;

    private Dictionary<string, Sprite> iconDictionary;

    private void Awake() {
        Instance = this;

        iconDictionary = new Dictionary<string, Sprite>
        {
            { "play", playIcon },
            { "pause", pauseIcon },
            { "bigBlueOrb", bigBlueOrbIcon },
            { "smallBlueOrb", smallBlueOrbIcon },
        };
    }

    public Dictionary<string, Sprite> GetIconDictionary() {
        return iconDictionary;
    }
}
