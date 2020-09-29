using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenu = null;

    MainControls Controls;
    bool paused = false;

    // Start is called before the first frame update
    void Start()
    {
        Controls = new MainControls();
        Controls.Enable();
        Controls.Player.Pause.performed += (ctx) => Pause();
    }

    public void Pause()
    {
        paused = !paused;
        if(paused)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0.0f;
        }
        else
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1.0f;
        }
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1.0f;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1.0f;
    }
}
