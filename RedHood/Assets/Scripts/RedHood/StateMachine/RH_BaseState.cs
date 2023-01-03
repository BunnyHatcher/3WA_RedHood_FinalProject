using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class RH_BaseState : StateMachineBehaviour
{
    #region References

    //References necessary for character movements

    protected Rigidbody _rigidbody;
    protected Transform _cameraTransform;
    protected Transform _playerTransform;
    protected GameObject _playerGameObject;
    protected PlayerControlSettings _playerParameters;
    protected Animator _animator;
    //Scripts
    protected FloorDetector _floorDetector;
    protected RH_PlayerMovement _playerMovement;
    protected RH_PhysicsHandler _physicsHandler;
    protected RH_AnimatorHandler _animatorHandler;
    protected Targeter _targeter;


    public Vector3 _movementDirection;
    protected float _currentSpeed;

    #endregion



    #region Awake & Start

    private void Awake()
    {
        /*
        // Bring Scriptable Objects into CharacterStateBase Script
        string GUID = AssetDatabase.FindAssets("RedHoodParameters")[0]; // Find Scriptable Object asset
        string path = AssetDatabase.GUIDToAssetPath(GUID);
        _playerParameters = (PlayerControlSettings)AssetDatabase.LoadAssetAtPath(path, typeof(PlayerControlSettings));
        */

        _playerGameObject = GameObject.Find("RH_V3");

        //Get all Components directly on RedHoodCharacter
        _playerTransform = _playerGameObject.transform;
        _rigidbody = _playerGameObject.GetComponent<Rigidbody>();
        _physicsHandler = _playerGameObject.GetComponent<RH_PhysicsHandler>();
        _animatorHandler = _playerGameObject.GetComponent<RH_AnimatorHandler>();
        _playerMovement = _playerGameObject.GetComponent<RH_PlayerMovement>();
        _animator = _playerGameObject.GetComponent<Animator>();

        //Get all Components in Children of RedHoodCharacter
        _floorDetector = _playerGameObject.GetComponentInChildren<FloorDetector>();
        _targeter = _playerGameObject.GetComponentInChildren<Targeter>();
        //Set up Camera
        //_cameraTransform = Camera.main.transform;
    }

    

    #endregion

    #region Methods
    


    #endregion
}
