using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("AmbientSounds");

        if (objs.Length > 1)
        {
            foreach (var obj in objs)
            {
                Destroy(obj);
            }
        }

        DontDestroyOnLoad(this.gameObject);
    }
}
