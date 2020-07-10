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

    // Start is called before the first frame update
    void Start()
    {
        currentIndex = 0;
        sceneText.text = plotText[currentIndex].text;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentIndex++;

            if (currentIndex > plotText.Length - 1)
            {
                SceneManager.LoadScene(nextScene);
            }

            sceneText.text = plotText[currentIndex].text;
        }
    }
}
