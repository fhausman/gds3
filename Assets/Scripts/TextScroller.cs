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
    private string[] chapters;

    // Start is called before the first frame update
    void Start()
    {
        currentIndex = 0;
        chapters = _plotText.text.Split('\n');
        sceneText.text = chapters[currentIndex];
        fade = GameObject.FindObjectOfType<Fade>();
        sceneLoad = SceneManager.LoadSceneAsync(nextScene);
        sceneLoad.allowSceneActivation = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            currentIndex++;

            fade.FadeIn();
            if (currentIndex > chapters.Length - 1)
            {
                fade.onFadeOutEnd.AddListener(() => sceneLoad.allowSceneActivation = true);
                fade.FadeOut();
                gameObject.SetActive(false);
                return;
            }

            sceneText.text = chapters[currentIndex];
        }
    }
}
