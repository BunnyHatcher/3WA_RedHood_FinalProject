using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RH_PhysicsHandler : MonoBehaviour
{
    [Header("FloorDetection")]
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Vector3 _boxDimension;
    [SerializeField] private Transform _groundChecker;
    public float _yFloorOffset = 1f;
    [SerializeField] private FloorDetector _floorDetector;

    // Other Components
    private Rigidbody _rigidbody;
    private Animator _animator;

    //Other Scripts
    public RH_PlayerMovement _playerMovement;


    //public bool _isGrounded = true;



    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _playerMovement = GetComponent<RH_PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        DetectIfGrounded();

        if (_playerMovement._isGrounded)
        {
            _animator.SetBool("isGrounded", true);
            //_animator.SetBool("isJumping", false);
        }

        else
        { _animator.SetBool("isGrounded", false); }

    }

    private void DetectIfGrounded()
    {
        // Draw boxes that represents the ground checker
        Collider[] groundColliders = Physics.OverlapBox(_groundChecker.position, _boxDimension, Quaternion.identity, _groundMask); //list is referencing the different colliders that might be touched by the box

        _playerMovement._isGrounded = groundColliders.Length > 0;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(_groundChecker.position, _boxDimension * 2f); // by default OverlapBox only takes half the size of the box in each dimension, so we need to double the size of _boxDimension
    }


}
