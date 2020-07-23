using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    public Interactable attachedObject1;
    public Interactable attachedObject2;


    public void Activate()
    {
        attachedObject1.OnInteractionStart();
        attachedObject2.OnInteractionStart();
    }

    public void Deactivate()
    {
        attachedObject1.OnInteractionEnd();
        attachedObject2.OnInteractionEnd();
    }
}
