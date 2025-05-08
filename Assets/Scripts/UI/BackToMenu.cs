using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Chess.Game;

public class BackToMenu : MonoBehaviour
{
    [SerializeField]
    private Button backButton;

    void Start()
    {
        backButton.onClick.AddListener(() => SceneManager.LoadScene("Menu"));
    }
}