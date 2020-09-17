using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

enum LaserType
{
    DeadlyLaser,
    PowerBeam
}

public interface ILaserHitBehaviour
{
    Tuple<Vector3, Vector3> Reflect(Vector3 currDir, RaycastHit hit);
    bool OnObjectHit(GameObject hit_object);
    int GetCollisionMask();
    void OnNothingWasHit();
}

public class DeadlyLaserBehaviour : ILaserHitBehaviour
{
    private int _layerMask = 0;

    public DeadlyLaserBehaviour()
    {
        _layerMask = LayerMask.GetMask("Player", "Ground");
    }

    public int GetCollisionMask()
    {
        return _layerMask;
    }

    public void OnNothingWasHit()
    {
    }

    public bool OnObjectHit(GameObject hit_object)
    {
        if(hit_object.CompareTag("Player"))
        {
            hit_object.SendMessage("OnLaserHit");
            return true;
        }
       
        return false;
    }  

    public Tuple<Vector3, Vector3> Reflect(Vector3 currDir, RaycastHit hit)
    {
        return Tuple.Create(Vector3.Reflect(currDir, hit.normal), hit.point);
    }
}

public class PowerBeamBehaviour : ILaserHitBehaviour
{
    private int _layerMask = 0;
    private Detector _hitDetector = null;

    public PowerBeamBehaviour()
    {
        _layerMask = LayerMask.GetMask("Lens", "Ground");
    }

    public int GetCollisionMask()
    {
        return _layerMask;
    }

    public void OnNothingWasHit()
    {
        if(_hitDetector)
        {
            _hitDetector.Deactivate();
            _hitDetector = null;
        }
    }

    public bool OnObjectHit(GameObject hit_object)
    {
        if(hit_object.CompareTag("Detector"))
        {
            var detector = hit_object.GetComponent<Detector>();
            if(_hitDetector != detector)
            {
                if (_hitDetector)
                    _hitDetector.Deactivate();

                _hitDetector = detector;
                _hitDetector.Activate();
            }
            return true;
        }

        return false;
    }

    public Tuple<Vector3, Vector3> Reflect(Vector3 currDir, RaycastHit hit)
    {
        if(hit.collider.gameObject.CompareTag("Lens"))
        {
            var lens = hit.collider.gameObject.GetComponent<Lens>();
            return Tuple.Create(lens.GetReflectionDirection(), lens.transform.position);
        }

        return Tuple.Create(Vector3.Reflect(currDir, hit.normal), hit.point);

    }
}

[ExecuteInEditMode]
public class LaserSource : MonoBehaviour
{
    [SerializeField]
    private GameObject _beamOrigin = null;

    [SerializeField]
    private LineRenderer _beam = null;

    [SerializeField]
    private LaserType _laserType = LaserType.DeadlyLaser;

    [SerializeField]
    private bool _castsReflections = true;

    private ILaserHitBehaviour _laserHitBehaviour;
    private UnityEvent _onNothingWasHit = new UnityEvent();
    private const int MAX_REFLECTIONS_NUM = 10;

    private void Awake()
    {
        switch (_laserType)
        {
            case LaserType.DeadlyLaser:
                _laserHitBehaviour = new DeadlyLaserBehaviour();
                break;
            case LaserType.PowerBeam:
                _laserHitBehaviour = new PowerBeamBehaviour();
                _onNothingWasHit.AddListener(() => _laserHitBehaviour.OnNothingWasHit());
                break;
        }
    }

    private void Update()
    {
        var beamPoints = new List<Vector3>();
        var currentPoint = _beamOrigin.transform.position;
        var currentDir = _beamOrigin.transform.right;
        beamPoints.Add(currentPoint);

        var max_refl = _castsReflections ? MAX_REFLECTIONS_NUM : 1;
        for (int i = 0; i < max_refl; ++i)
        {
            RaycastHit hit;
            if(!Physics.Raycast(currentPoint, currentDir, out hit, 100.0f, _laserHitBehaviour.GetCollisionMask()))
            {
                _onNothingWasHit.Invoke();
                break;
            }

            if (hit.collider.CompareTag("Player"))
            {
                _laserHitBehaviour.OnObjectHit(hit.collider.gameObject);
                continue;
            }

            (currentDir, currentPoint) = _laserHitBehaviour.Reflect(currentDir, hit);
            beamPoints.Add(currentPoint);

            if (_laserHitBehaviour.OnObjectHit(hit.collider.gameObject))
            {
                break;
            }

            //beam hits orthogonal wall
            if (hit.normal == currentDir.normalized)
            {
                _onNothingWasHit.Invoke();
                break;
            }
        }

        _beam.positionCount = beamPoints.Count;
        _beam.SetPositions(beamPoints.ToArray());
    }


}
