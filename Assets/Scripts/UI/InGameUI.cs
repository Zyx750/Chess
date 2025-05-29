using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Chess.Game;
using TMPro;

public class InGameUI : MonoBehaviour
{
    [SerializeField]
    private Button backButton;

    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    TMP_Text resultText;

    [SerializeField]
    private Button rematchBtn;

    [SerializeField]
    private Button menuBtn;

    void Start()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("Menu"));
        rematchBtn.onClick.AddListener(() => SceneManager.LoadScene("MainScene"));
        menuBtn.onClick.AddListener(() => SceneManager.LoadScene("Menu"));
    }

    public void ShowGameOver(int result) // -1 black win, 0 draw, 1 white win
    {
        resultText.text = result switch
        {
            -1 => "<color=black>BLACK WINS!</color>",
            0 => "IT'S A DRAW!",
            1 => "<color=white>WHITE WINS!</color>",
            _ => ""
        };
        gameOverPanel.SetActive(true);
    }
}
