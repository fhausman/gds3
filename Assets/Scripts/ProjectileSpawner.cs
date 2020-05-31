using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    float timeElapsed = 0.0f;
    public GameObject prefab;

    private void Update()
    {
        if(timeElapsed > 1.0f)
        {
            prefab.transform.position = transform.position;
            Instantiate(prefab);
            timeElapsed = 0.0f;
        }

        timeElapsed += Time.deltaTime;
    }
}
