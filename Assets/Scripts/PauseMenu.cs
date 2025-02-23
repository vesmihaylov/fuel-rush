using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private static PauseMenu instance;
    private bool isPaused = false;
    private string mainMenuSceneName = "Main_Menu";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        pauseMenuUI.SetActive(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != mainMenuSceneName)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    OnResumeButton();
                else
                    OnPauseButton();
            }
        }
    }

    public void OnPauseButton()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OnResumeButton()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        pauseMenuUI.SetActive(false);
        isPaused = false;
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Destroy(gameObject);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}