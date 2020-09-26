using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TextScroller : MonoBehaviour
{
    [SerializeField]
    private TextAsset _plotText = null;

    [SerializeField]
    private string nextScene = "";

    [SerializeField]
    private TextMeshProUGUI sceneText = null;

    [SerializeField]
    private UnityEvent[] events = null;

    public bool CanLoadNextLevel { get; set; } = true;

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
            events[0].Invoke();
        }
        fade = GameObject.FindObjectOfType<Fade>();
        sceneLoad = SceneManager.LoadSceneAsync(nextScene);
        sceneLoad.allowSceneActivation = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown && currentIndex <= chapters.Length - 1)
        {
            if (chapters != null)
            {
                currentIndex++;
                if (currentIndex > chapters.Length - 1)
                {
                    StartCoroutine(LoadNextScene());
                    return;
                }
                else
                {
                    fade.FadeIn();
                    sceneText.text = chapters[currentIndex];
                    events[currentIndex].Invoke();
                }
            }
            else
            {
                StartCoroutine(LoadNextScene());
            }
        }
    }

    void AllowLoad()
    {
        CanLoadNextLevel = true;
    }

    void DisallowLoad()
    {
        CanLoadNextLevel = false;
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitUntil(() => CanLoadNextLevel == true);

        fade.onFadeOutEnd.AddListener(() => sceneLoad.allowSceneActivation = true);
        fade.FadeOut();
        gameObject.SetActive(false);
    }
}
