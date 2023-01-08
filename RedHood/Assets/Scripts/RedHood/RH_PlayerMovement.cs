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
    protected ForceReceiver _forceReceiver;
    
    //Animations and StateMachine
    public string _currentState; // maybe work with State Tags? --> IsTag
    public RH_AnimatorHandler _animatorManager;    
    public Animator _stateMachine;


    // Input System
    [HideInInspector]
    public Vector2 _movementInput;
    [HideInInspector]
    public float _verticalInput;
    [HideInInspector]
    public float _horizontalInput;

    //Movement Values
    [Header("Movement")]
    public Vector3 _movementDirection = new Vector3();
    public float _currentSpeed;
    //public float _moveSpeed;
    public float _joggingSpeed = 300f;
    public float _runningSpeed = 500f;
    public float _turnSpeed = 500f;

    //Jumping
    public float _jumpForce = 5f;

    //Combat References
    [Header("Combat")]
    [SerializeField]
    private GameObject _leftWeapon;
    [SerializeField]
    private GameObject _leftWeaponSlot;
    [SerializeField]
    private GameObject _rightWeapon;
    [SerializeField]
    private GameObject _rightWeaponSlot;

    //Force Values

    #region Attack Values
    [field: SerializeField]  public float ForceTime { get; private set; }
    [field: SerializeField] public float Force { get; private set; }

    public int _playerDamage = 10;
    public float _playerKnockback = 15;

    #endregion

    private bool _alreadyAppliedForce;

    #region Not Used
    //Attacks & Combat
    //[field: SerializeField] public Attack[] Attacks { get; private set; }

    //Scriptable Object
    //protected PlayerControlSettings _playerParameters;
    #endregion

    //Combos
    public int _comboStateIndex = -1;    
    public float _comboAttackTime;


    // BOOLS
    [HideInInspector]
    public bool _isInteracting = false;
    [HideInInspector]
    public bool _isJumping = false;
    [HideInInspector]
    public bool _isGrounded = false;
    [HideInInspector]
    public bool _isFalling = false;
    [HideInInspector]
    public bool _forcesApplied = false;
    [HideInInspector]
    public bool _isDodging = false;
    [HideInInspector]
    public bool _isAttacking = false;
    [HideInInspector]
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
        _forceReceiver = GetComponent<ForceReceiver>();
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
        
        #region In Air vs Grounded

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
        #endregion

        #region Jumping
        if (_isJumping)
        {
            _movementDirection.y = _jumpForce;
            //Debug.Log("JumpForce applied");
            _isJumping = false; // to prevent changing to isJumping every frame after the first one: we want to jump only once
        }
        #endregion

        #region Attacking (NOT USED)
        /*
        if (_stateMachine.GetCurrentAnimatorStateInfo(2).IsTag("Attack"))
        {
            //We change to root motion and make sure the player can't move while the root motion is playing
            _stateMachine.applyRootMotion = true;
            _stateMachine.SetFloat("moveSpeed", 0f); //... this means the player can't move instead the root motion takes over
        }

        if (_stateMachine.GetCurrentAnimatorStateInfo(2).IsTag("Attack"))
        {
            TryApplyForce();

        }
         */

        #endregion

        MoveCharacter();

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

    public void TryApplyForce()
    {
        if (_alreadyAppliedForce) { return; }

        _forceReceiver.AddForce(transform.forward * Force);

        _alreadyAppliedForce = true;
    }

    #endregion

    #region Combo Calculations

    //Method to find out what state we are in when transitioning between states
    private float GetNormalizedTime()
    {
        AnimatorStateInfo currentInfo = _stateMachine.GetCurrentAnimatorStateInfo(2);
        AnimatorStateInfo nextInfo = _stateMachine.GetNextAnimatorStateInfo(2);

        if (_stateMachine.IsInTransition(0) && nextInfo.IsTag("Attack")) //If we are in an attack transition...
        {
            return nextInfo.normalizedTime; //... tell us how far we are through with it
        }
        else if (!_stateMachine.IsInTransition(0) && currentInfo.IsTag("Attack")) //If we are in an attack & not in transition...
        {
            return currentInfo.normalizedTime; //... tell us how far we are through with it
        }
        else // Safe fail condition
        {
            return 0f;
        }
    }
    private void TryComboAttack(float normalizedTime)
    {
        if (_comboStateIndex == -1) { return; }

        if (normalizedTime < _comboAttackTime) { return; }

        _stateMachine.SetBool("CanSwitchCombo", true);


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



