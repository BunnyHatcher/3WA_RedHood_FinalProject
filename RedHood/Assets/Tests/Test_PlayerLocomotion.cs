using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Test_PlayerLocomotion : MonoBehaviour
{

    protected Rigidbody _rigidbody;
    protected Transform _cameraTransform;
    public Transform _playerTransform;
    protected GameObject _playerGameObject;
    protected PlayerControlSettings _playerParameters;
    protected RH_InputManager _inputManager;
    protected RH_PhysicsHandler _physicsHandler;
    protected FloorDetector _floorDetector;
    public RH_AnimatorManager _animatorManager;


    public float _joggingSpeed = 5f;    
    public float _turnSpeed = 500f;

    public Vector3 _movementDirection = new Vector3();
    protected float _currentSpeed;



    #region Awake & Start

    private void Awake()
    {
        // Bring Scriptable Objects into Movement Script
        string GUID = AssetDatabase.FindAssets("RedHoodParameters")[0]; // Find Scriptable Object asset
        string path = AssetDatabase.GUIDToAssetPath(GUID);
        _playerParameters = (PlayerControlSettings)AssetDatabase.LoadAssetAtPath(path, typeof(PlayerControlSettings));

        //Get all Components directly on RedHoodCharacter
        _playerGameObject = GameObject.Find("TestPlayer");
        _inputManager = GetComponent<RH_InputManager>();
        _rigidbody = GetComponent<Rigidbody>();
        //_physicsHandler = GetComponent<RH_PhysicsHandler>();
        _animatorManager = GetComponent<RH_AnimatorManager>();
        //_playerTransform = _playerGameObject.transform; --> not neede, we drag PlayerTransform on to the field
        
        //Get all Components on children of RedHoodCharacter
        _floorDetector = GetComponentInChildren<FloorDetector>();

        //Set up Camera
        _cameraTransform = Camera.main.transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        _currentSpeed = _joggingSpeed;
    }

    #endregion

    // Update is called once per frame
    void Update()
    {
        _inputManager.HandleAllInputs();
        HandleMovement();

    }

    private void FixedUpdate()
    {
        MoveCharacter();
        StickToGround();
        RotateTowardsCamera();        
    }

    private void LateUpdate()
    {
    }



    #region Methods

    /*
    public void HandleAllMovement()
    {
        HandleMovement();
        RotateTowardsCamera();
    }
    */

    private void HandleMovement()
    {
        _movementDirection = _cameraTransform.forward * _inputManager._verticalInput
                             + _cameraTransform.right * _inputManager._horizontalInput;
        _movementDirection.Normalize();
        _movementDirection.y = 0;
        
        /*
        if (_inputManager._moveAmount >= 0.5f)
        {
            _currentSpeed = _joggingSpeed;
        }
        else
        {
            _currentSpeed = _playerParameters._walkingSpeed;
        }
        */
        

        _movementDirection *= _currentSpeed * Time.fixedDeltaTime;

        Debug.Log("Current Speed: " + _currentSpeed);

        
        //------TEST------------------------------
        _animatorManager._moveSpeed = _movementDirection.magnitude;
    }

    private void MoveCharacter()
    {
        Vector3 movementVelocity = _movementDirection;
        _rigidbody.velocity = movementVelocity;
    }

    private void RotateTowardsCamera()
    {
        //Rotation V1
        Vector3 targetDirection = Vector3.zero;
        targetDirection = _cameraTransform.forward; //* _inputManager._verticalInput
                        //+ _cameraTransform.right; //* _inputManager._horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;
        //Debug.Log("Target Direction " + targetDirection);

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(_playerTransform.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime);

        _playerTransform.rotation = playerRotation;

        //Rotation V2
        /*
        Vector3 lookDirection = _cameraTransform.forward;
        lookDirection.y = 0;


        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        _playerTransform.rotation = Quaternion.Lerp(_playerTransform.rotation, lookRotation, _turnSpeed * Time.deltaTime);
        */
    }

    public void StickToGround()
    {
        Vector3 averagePosition = _floorDetector.AverageHeight();

        Vector3 newPosition = new Vector3(_rigidbody.position.x, averagePosition.y + _physicsHandler._yFloorOffset, _rigidbody.position.z); // glues the character to the average position on the y-axis
        _rigidbody.MovePosition(newPosition);
        _movementDirection.y = 0;
    }
    #endregion
}



