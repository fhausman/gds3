using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody rb;

    // Start is called before the first frame update
    private void Reflect()
    {
        Debug.Log("Reflect");
        rb.AddForce(Vector3.right * 20.0f, ForceMode.VelocityChange);
    }

    private void Destroy()
    {
        Debug.Log("Destroy");
        Destroy(gameObject);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.left*10.0f, ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
