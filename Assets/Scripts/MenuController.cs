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

    [SerializeField]
    private GameObject _music = null;

    private void Start()
    {
        Cursor.visible = true;
    }

    public void FadeOut()
    {
        _fade.onFadeOutEnd.AddListener(StartGame);
        _fade.onFadeOutEnd.AddListener(() => _music.SetActive(true));
        _fade.FadeOut();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Story1");
        Cursor.visible = false;
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
