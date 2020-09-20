using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    public string sceneToLoad = "";

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            var fade = GameObject.Find("Fade").GetComponent<Fade>();
            if (fade)
            {
                fade.onFadeOutEnd.AddListener(() => SceneManager.LoadScene(sceneToLoad));
                fade.FadeOut();
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}
