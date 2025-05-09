using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Chess.Game;
using TMPro;

public class Menu : MonoBehaviour
{
    [SerializeField]
    private GameObject mainPanel;

    [SerializeField]
    private Button multiplayerButton;

    [SerializeField]
    private Button singleplayerButton;

    [SerializeField]
    private Button aiConfigButton;

    [SerializeField]
    private GameObject aiConfigPanel;

    [SerializeField]
    private Toggle aiColorToggle;

    [SerializeField]
    private Slider aiTimeoutSlider;

    [SerializeField]
    TMP_Text aiTimeoutText;

    [SerializeField]
    private GameObject aiConfigConfirmButton;

    [SerializeField]
    private Button exitButton;

    [SerializeField]
    private Button aboutButton;

    [SerializeField]
    private GameObject aboutPanel;

    [SerializeField]
    private Button aboutBackButton;

    private static bool nextAiUseBlack = false;

    void Start()
    {
        singleplayerButton.onClick.AddListener(() => LoadMainScene(false));
        multiplayerButton.onClick.AddListener(() => LoadMainScene(true));
        exitButton.onClick.AddListener(() => Application.Quit());
        aboutButton.onClick.AddListener(() => ShowAboutPanel(true));
        aboutBackButton.onClick.AddListener(() => ShowAboutPanel(false));
        aiConfigButton.onClick.AddListener(() => ShowAiConfigPanel(true));
        aiConfigConfirmButton.GetComponent<Button>().onClick.AddListener(() => ShowAiConfigPanel(false));

        aiColorToggle.onValueChanged.AddListener((value) => {
            nextAiUseBlack = value;
        });
        aiTimeoutSlider.onValueChanged.AddListener((value) => {
            GameManager.nextAiTimeout = value;
            SetAiTimeoutText(value);
        });
        
        mainPanel.SetActive(true);
        aboutPanel.SetActive(false);
        aiConfigPanel.SetActive(false);

        aiColorToggle.isOn = nextAiUseBlack;
        aiTimeoutSlider.value = GameManager.nextAiTimeout;
        SetAiTimeoutText(GameManager.nextAiTimeout);
    }

    private void LoadMainScene(bool isMultiplayer)
    {
        if (isMultiplayer) {
            GameManager.PushNextSettings(true, true);
        } else {
            if (nextAiUseBlack) {
                GameManager.PushNextSettings(true, false);
            } else {
                GameManager.PushNextSettings(false, true);
            }
        }
        SceneManager.LoadScene("MainScene");
    }

    private void ShowAboutPanel(bool active)
    {
        mainPanel.SetActive(!active);
        aboutPanel.SetActive(active);
    }

    private void ShowAiConfigPanel(bool active)
    {
        mainPanel.SetActive(!active);
        aiConfigPanel.SetActive(active);
    }

    private void SetAiTimeoutText(float value)
    {
        aiTimeoutText.text = "THINKING TIME: " + value.ToString() + "s";
    }
}