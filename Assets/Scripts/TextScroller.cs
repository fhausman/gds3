using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TextScroller : MonoBehaviour
{
    [SerializeField]
    private TextAsset[] plotText = null;

    [SerializeField]
    private string nextScene = "";

    [SerializeField]
    private TextMeshProUGUI sceneText = null;

    private int currentIndex = 0;
    private Fade fade;
    private AsyncOperation sceneLoad;

    // Start is called before the first frame update
    void Start()
    {
        currentIndex = 0;
        sceneText.text = plotText[currentIndex].text;
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
            if (currentIndex > plotText.Length - 1)
            {
                fade.onFadeOutEnd.AddListener(() => sceneLoad.allowSceneActivation = true);
                fade.FadeOut();
                gameObject.SetActive(false);
                return;
            }

            sceneText.text = plotText[currentIndex].text;
        }
    }
}
