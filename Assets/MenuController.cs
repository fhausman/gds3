using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject _mainMenu = null;

    [SerializeField]
    private GameObject _credits = null;

    [SerializeField]
    private Fade _fade = null;


    public void FadeOut()
    {
        _fade.onFadeOutEnd.AddListener(StartGame);
        _fade.FadeOut();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Level1(Snaps)Alt");
    }

    public void Credits()
    {
        _mainMenu.SetActive(false);
        _credits.SetActive(true);
    }

    public void Back()
    {
        _mainMenu.SetActive(true);
        _credits.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
