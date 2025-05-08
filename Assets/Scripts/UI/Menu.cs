using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Chess.Game;

public class Menu : MonoBehaviour
{
    [SerializeField]
    private Button singleplayerButton;

    [SerializeField]
    private Button multiplayerButton;

    void Start()
    {
        singleplayerButton.onClick.AddListener(() => LoadMainScene(false));
        multiplayerButton.onClick.AddListener(() => LoadMainScene(true));
    }

    private void LoadMainScene(bool isMultiplayer)
    {
        if (isMultiplayer) {
            GameManager.PushNextSettings(true, true);
        } else {
            GameManager.PushNextSettings(true, false);
        }
        SceneManager.LoadScene("MainScene");
    }
}