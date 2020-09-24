using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextScroller : MonoBehaviour
{
    [SerializeField]
    private TextAsset _plotText = null;

    [SerializeField]
    private string nextScene = "";

    [SerializeField]
    private TextMeshProUGUI sceneText = null;

    private int currentIndex = 0;
    private Fade fade;
    private AsyncOperation sceneLoad;
    private string[] chapters = null;

    // Start is called before the first frame update
    void Start()
    {
        currentIndex = 0;
        if (_plotText != null)
        {
            chapters = _plotText.text.Split('\n');
            sceneText.text = chapters[currentIndex];
        }
        fade = GameObject.FindObjectOfType<Fade>();
        sceneLoad = SceneManager.LoadSceneAsync(nextScene);
        sceneLoad.allowSceneActivation = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (chapters != null)
            {
                currentIndex++;

                fade.FadeIn();
                if (currentIndex > chapters.Length - 1)
                {
                    LoadNextScene();
                    return;
                }

                sceneText.text = chapters[currentIndex];
            }
            else
            {
                LoadNextScene();
            }
        }
    }

    void LoadNextScene()
    {
        fade.onFadeOutEnd.AddListener(() => sceneLoad.allowSceneActivation = true);
        fade.FadeOut();
        gameObject.SetActive(false);
    }
}
