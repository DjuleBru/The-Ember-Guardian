using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton_MainMenu : MenuButton
{
    private void Start() {
        MainMenuUI.Instance.OnGameStart += MainMenuUI_OnGameStart;
    }

    private void MainMenuUI_OnGameStart(object sender, System.EventArgs e) {
        throw new System.NotImplementedException();
    }
}
