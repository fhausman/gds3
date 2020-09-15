using UnityEngine;

public class BlinkingLights : MonoBehaviour
{
    [SerializeField]
    private Light _light = null;

    [SerializeField]
    private float _probability = 0.2f;

    [SerializeField]
    private float _brightIntensity = 100.0f;

    [SerializeField]
    private float _darkerIntensity = 50.0f;
    // Update is called once per frame
    void Update()
    {
        var rng = Random.Range(0.0f, 1.0f);
        if(rng < _probability)
        {
            _light.intensity = _darkerIntensity;
        }
        else
        {
            _light.intensity = _brightIntensity;
        }
    }
}
