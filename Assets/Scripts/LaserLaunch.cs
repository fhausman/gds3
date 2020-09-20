using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserLaunch : MonoBehaviour
{
    public void Launch()
    {
        foreach(Transform child in gameObject.transform)
        {
            child.gameObject.GetComponent<LineRenderer>().enabled = true;
            child.gameObject.GetComponent<LaserSource>().enabled = true;
        }
    }

    public void Disable()
    {
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.GetComponent<LineRenderer>().enabled = false;
            child.gameObject.GetComponent<LaserSource>().enabled = false;
        }
    }
}
