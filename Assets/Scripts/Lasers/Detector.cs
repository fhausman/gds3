using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField]
    private Interactable[] attachedObjects;

    public void Activate()
    {
        foreach(var o in attachedObjects)
        {
            o.OnInteractionStart();
        }
    }

    public void Deactivate()
    {
        foreach (var o in attachedObjects)
        {
            o.OnInteractionEnd();
        }
    }
}
