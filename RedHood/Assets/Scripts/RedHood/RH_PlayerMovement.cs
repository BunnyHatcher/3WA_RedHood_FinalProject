using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;

public class RH_PlayerMovement : MonoBehaviour
{
    #region References
    //Player Movement
    protected Rigidbody _rigidbody;
    protected Transform _cameraTransform;
    public Transform _playerTransform;
    protected GameObject _playerGameObject;
    
    //Physics Logic
    protected RH_PhysicsHandler _physicsHandler;
    protected FloorDetector _floorDetector;
    
    //Animations and StateMachine
    public string _currentState; // maybe work with State Tags? --> IsTag
    public RH_AnimatorHandler _animatorManager;    
    public Animator _stateMachine;

    //Scriptable Object
    protected PlayerControlSettings _playerParameters;


    // Input System
    public Vector2 _movementInput;
    public float _verticalInput;
    public float _horizontalInput;

    //Movement Values
    public Vector3 _movementDirection = new Vector3();
    public float _currentSpeed;
    //public float _moveSpeed;

    public float _joggingSpeed = 300f;
    public float _runningSpeed = 500f;
    public float _turnSpeed = 500f;

    //Jumping
    public float _jumpForce = 5f;

    //Combat
    [SerializeField]
    private GameObject _leftWeapon;
    [SerializeField]
    private GameObject _leftWeaponSlot;
    [SerializeField]
    private GameObject _rightWeapon;
    [SerializeField]
    private GameObject _rightWeaponSlot;
    
    //Combos
    public int _comboStateIndex = -1;    
    public float _comboAttackTime;


    // BOOLS
    public bool _isInteracting = false;

    public bool _isJumping = false;
    public bool _isGrounded = false;
    public bool _isFalling = false;
    public bool _forcesApplied = false;

    public bool _isDodging = false;

    public bool _isAttacking = false;    
    public bool _weaponDrawn = false;


    #endregion


    #region Awake & Start

    private void Awake()
    {
        /*
        // Bring Scriptable Objects into Movement Script
        string GUID = AssetDatabase.FindAssets("RedHoodParameters")[0]; // Find Scriptable Object asset
        string path = AssetDatabase.GUIDToAssetPath(GUID);
        _playerParameters = (PlayerControlSettings)AssetDatabase.LoadAssetAtPath(path, typeof(PlayerControlSettings));
        */

        //Get all Components directly on RedHoodCharacter
        _playerGameObject = GameObject.Find("RH_V3");
        _rigidbody = GetComponent<Rigidbody>();
        _physicsHandler = GetComponent<RH_PhysicsHandler>();
        _animatorManager = GetComponent<RH_AnimatorHandler>();
        //_playerTransform = _playerGameObject.transform; --> not needed, we drag PlayerTransform on to the field
        
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
        //When we interact, we don't have control anymore:
        if (_isInteracting)
        {
            return;
        }

        HandleMovement();
        RotateTowardsCamera();

    }

    private void FixedUpdate()
    {
        // Set FallVelocity when in States of tag "InAir"
        if (_stateMachine.GetCurrentAnimatorStateInfo(1).IsTag("InAir")) // Determine current state through Animator Tag via AnimatorStateInfo
        {
            SetFallVelocity();
            //Debug.Log("Player in Air, Velocity = " + _movementDirection.y);
        }
        // ... when in any other state: stay grounded
        else
        {
            StickToGround();
        }


        if (_isJumping)
        {
            _movementDirection.y = _jumpForce;
            //Debug.Log("JumpForce applied");
            _isJumping = false; // to prevent changing to isJumping every frame after the first one: we want to jump only once
        }


        if (_stateMachine.GetCurrentAnimatorStateInfo(1).IsTag("Dodge"))
        {

        }

        //... 
        
        MoveCharacter(); 
        

        if (_forcesApplied)
        {

        }
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
    #region Receiving Input
    public void HandleMovementInput(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
        _verticalInput = _movementInput.y;
        _horizontalInput = _movementInput.x;
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        _movementDirection = _cameraTransform.forward * _verticalInput
                             + _cameraTransform.right * _horizontalInput;
        _movementDirection.Normalize();
        _movementDirection.y = 0;
        _movementDirection *= _currentSpeed * Time.fixedDeltaTime;

       // Debug.Log("Current Speed: " + _currentSpeed);

        //Getting MoveSpeed for AnimationManager
        _animatorManager._moveSpeed = _movementDirection.magnitude;
    }

    public void HandleDodgingAndSprinting(float delta)
    {
        if (_isInteracting)
           return;

        _movementDirection = _cameraTransform.forward * _verticalInput
                             + _cameraTransform.right * _horizontalInput;

        float moveInput = _verticalInput + _horizontalInput;
        
        if (moveInput >= 0 )
        {
             _movementDirection.y = 0;
             Quaternion dodgeRotation = Quaternion.LookRotation(_movementDirection);
             _playerTransform.rotation = dodgeRotation;
        }
        else
        {
                
        }
        
        
    }

    private void MoveCharacter()
    {
        //Vector3 movementVelocity = _movementDirection;
        //_rigidbody.velocity = movementVelocity;
        _rigidbody.velocity = _movementDirection;
    }

    private void RotateTowardsCamera()
    {
        //Rotation V1
        Vector3 targetDirection = Vector3.zero;
        targetDirection = _cameraTransform.forward;
        targetDirection.Normalize();
        targetDirection.y = 0;
        //Debug.Log("Target Direction " + targetDirection);

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection); 
        _rigidbody.MoveRotation(Quaternion.Slerp(_rigidbody.rotation, targetRotation, _turnSpeed * Time.deltaTime));

        //_playerTransform.rotation = playerRotation;
    }

    public void MoveByPhysics(float deltaTime)
    {

    }

    private void SetFallVelocity()
    {
        _movementDirection.y = _rigidbody.velocity.y;
    }

    public void StickToGround()
    {
        Vector3 averagePosition = _floorDetector.AverageHeight();

        Vector3 newPosition = new Vector3(_rigidbody.position.x, averagePosition.y + _physicsHandler._yFloorOffset, _rigidbody.position.z); // glues the character to the average position on the y-axis
        _rigidbody.MovePosition(newPosition);
        _movementDirection.y = 0;
    }
    #endregion

    #region Combat
    public void DrawWeapon()
    {
        //Instantiate(_leftWeapon, _leftWeaponSlot.transform);
        //Instantiate(_rightWeapon, _rightWeaponSlot.transform);
        _leftWeapon.SetActive(true);
        _rightWeapon.SetActive(true);
    }

    public void SheathWeapon()
    {
        //Destroy(_leftWeapon);
        //Destroy(_rightWeapon);
        _leftWeapon.SetActive(false);
        _rightWeapon.SetActive(false);
    }
    #endregion

    #region Calculations for StateMachine
    private void DetectCurrentState()
    {
        int stateHash = _stateMachine.GetCurrentAnimatorStateInfo(1).fullPathHash;
        _currentState = System.Convert.ToString(stateHash);
        
        Debug.Log("Player State Machine is currently in State " + _currentState);
    }

    #endregion








    #endregion

}


