using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        var tag = gameObject.tag;
        var others = GameObject.FindGameObjectsWithTag(tag);
        if(others.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
