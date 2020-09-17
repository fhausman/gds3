using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserInterval : MonoBehaviour
    //Made by Kamil :D
{

     public GameObject[] lasers;
    [SerializeField] public float repeatRate = 1.0f;
   
    void Start()
    {
        InvokeRepeating("EnableRepeat", 1.0f, repeatRate);
    }

    void EnableRepeat()
    {
        foreach(GameObject lasers in lasers)
        {
            lasers.GetComponent<LineRenderer>().enabled = !lasers.GetComponent<LineRenderer>().enabled;
            lasers.GetComponent<LaserSource>().enabled = !lasers.GetComponent<LaserSource>().enabled;
            lasers.GetComponent<BoxCollider>().enabled = !lasers.GetComponent<BoxCollider>().enabled;
        }
    }

   
}
