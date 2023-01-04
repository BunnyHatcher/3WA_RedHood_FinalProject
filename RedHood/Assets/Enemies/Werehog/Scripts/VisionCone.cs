using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionCone : MonoBehaviour
{
    [SerializeField]
    private LayerMask _playerLayer;
    
    [HideInInspector]
    public GameObject _target;

    private BaseState _baseState;

    public bool _fleetingDetection = false;
    public bool _fullDetection = false;

    private float _detectionTimer = 3f;


    private void Awake()
    {
        _baseState = FindObjectOfType<BaseState>();
    }

    private void Update()
    {
        // Collision timer
        if (_fleetingDetection == true)
        {
            _detectionTimer -= Time.deltaTime;
            if (_detectionTimer < 0)
            {
                _detectionTimer = 0;
                Debug.Log("Detection Timer = 0");
            }
        }
    }




    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 rayDirection = other.transform.position - transform.position;
            RaycastHit hit;

            if (Physics.Raycast(transform.position, rayDirection, out hit, Mathf.Infinity, _playerLayer))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("Fleeting Detection");
                    //_target = other.gameObject;
                    _fleetingDetection = true;                    
                }
            }

        }
    }




    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Vector3 rayDirection = other.transform.position - transform.position;
            RaycastHit hit;

            if (Physics.Raycast(transform.position, rayDirection, out hit, Mathf.Infinity, _playerLayer))
            {
                if (hit.collider.CompareTag("Player") && _fleetingDetection == true)
                {
                    if (_detectionTimer <= 0)
                    {
                        Debug.Log("Full detection");
                        _fullDetection = true;
                        _target = other.gameObject;
                        _baseState._timeSinceLastSawPlayer = 0;
                    }
                }

            }

        }
    }  
    




    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited Detection");
            _fullDetection = false;
            _fleetingDetection = false;
            _target = null;
        }
    }
   





}
