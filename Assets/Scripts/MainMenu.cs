using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject quitConfirmationPanel;
    public GameObject mainMenuPanel;
    public GameObject mapSelectionPanel;
    public GameObject creditsPanel;
    public TextMeshProUGUI tooltipText;
    public GameObject goBackButton;

    public void OnStartGameButton()
    {
        mainMenuPanel.SetActive(false);
        mapSelectionPanel.SetActive(true);
        goBackButton.SetActive(true);
    }

    public void OnCreditsButton()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
        goBackButton.SetActive(true);
    }

    public void OnSettingsButton()
    {
        ShowTooltip("Coming Soon");
    }

    public void OnQuitButton()
    {
        quitConfirmationPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void OnConfirmQuit()
    {
        Application.Quit();
    }

    public void OnCancelQuit()
    {
        quitConfirmationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnMapSelectionButton(string mapName)
    {
        if (MapExists(mapName))
        {
            SceneManager.LoadScene(mapName);
        }
        else
        {
            ShowTooltip("Coming Soon");
        }
    }

    public void OnBackButton()
    {
        mainMenuPanel.SetActive(true);
        mapSelectionPanel.SetActive(false);
        creditsPanel.SetActive(false);
        goBackButton.SetActive(false);
    }

    private void ShowTooltip(string message)
    {
        tooltipText.text = message;
        tooltipText.gameObject.SetActive(true);
        Invoke("HideTooltip", 1.5f);
    }

    private void HideTooltip()
    {
        tooltipText.gameObject.SetActive(false);
    }

    private bool MapExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneFileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneFileName == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}