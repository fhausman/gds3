using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public Interactable attachedObject;

    public void Activate()
    {
        attachedObject.OnInteractionStart();
    }

    public void Deactivate()
    {
        attachedObject.OnInteractionEnd();
    }
}
