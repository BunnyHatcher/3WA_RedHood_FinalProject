using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RH_AnimatorHandler : MonoBehaviour
{
    
    Animator _animator;
    Rigidbody _rigidbody;
    RH_PhysicsHandler _physicsHandler;
    RH_PlayerMovement _playerMovement;
    Targeter _targeter;
    //Target _target;

    int horizontal;
    int vertical;

    public float _moveSpeed;

    // Booleans
    public bool _isRunning;
    public bool _isTargeting;
    public bool _isNearLootable;

    private void Awake()
    {
        
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<RH_PlayerMovement>();
        _rigidbody = GetComponent<Rigidbody>();
        _targeter = GetComponentInChildren<Targeter>();
    }

    
    

    //UPDATING MOVEMENT ANIMATIONS
    
    private void Update()
    {
        _animator.SetFloat("moveSpeed", _moveSpeed, 0.1f, Time.deltaTime);
        _animator.SetFloat("SpeedX", _playerMovement._verticalInput);
        _animator.SetFloat("SpeedY", _playerMovement._horizontalInput);
        _animator.SetFloat("VerticalVelocity", _playerMovement._movementDirection.y);
    }


    // INPUT ACTIONS

    #region Movement Input
    public void OnRun(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                _animator.SetBool("isRunning", true);
                break;

            
            case InputActionPhase.Canceled:
                _animator.SetBool("isRunning", false);
                break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started /*&& _physicsHandler._isGrounded == true*/)
        {
            _animator.SetBool("isJumping", true);
            _animator.SetBool("isGrounded", false);

            _animator.SetTrigger("JumpTrigger");
            
            //Debug.Log("Jump context started");

           /* if (_rigidbody.velocity.y < -0.2f && !_playerMovement._isGrounded)
            {
                _animator.SetBool("isFalling", true);
            }*/
        }
        
        else if (context.canceled)
        {
            _animator.SetBool("isJumping", false);
        }
        

    }

    public void OnDodge(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _animator.SetTrigger("DodgeTrigger");
            //Debug.Log("Dodge context started");
            //_animator.applyRootMotion = true /*_playerMovement._isInteracting*/;
        }
        
    }

    #endregion

    #region Combat Input
    public void OnDrawSheathWeapon(InputAction.CallbackContext context)
    {       
        
            if (context.started && _playerMovement._weaponDrawn == false)
            {
               _animator.SetBool("InCombatStance", true);
               //Debug.Log("Weapon was drawn");
            }
            
            else if (context.started && _playerMovement._weaponDrawn == true)
            {             
               _animator.SetBool("InCombatStance", false);
               //Debug.Log("Weapon was sheathed");             
            }        
    }

    public void OnTargeting(InputAction.CallbackContext context)
    {
        
        //if(!_targeter.SelectTarget()) { return; }
        if(!context.started) { return; }

        if (!_isTargeting)
        {
            if (_targeter.SelectTarget())
            // Start Targeting
            {
                _isTargeting = true;
                _animator.SetBool("isTargeting", true);

                //TargetEvent?.Invoke();

                if (_playerMovement._weaponDrawn == true)
                { return; }

                _animator.SetBool("InCombatStance", true);
                //Debug.Log("Initiating Targeting");
            }
        }

        else //if (context.started && _isTargeting || !_targeter.SelectTarget()) // Check if there is a target in the list
        {
            // Cancel targeting
            _targeter.Cancel();
            _isTargeting = false;
            _animator.SetBool("isTargeting", false);


            //CancelEvent?.Invoke();

            //Debug.Log("Cancelled Targeting");
        }
     
    }



    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _animator.SetTrigger("LightAttack");
            //Debug.Log("Light Attack is triggered");
        }       
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _animator.SetTrigger("HeavyAttack");
            //Debug.Log("Heavy Attack is triggered");
        }
    }

    #endregion

    #region Miscellaneous

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _animator.SetTrigger("LootTrigger");
            _animator.SetBool("isInteracting", true);
            Debug.Log("Interacting");

        }
    }

    #endregion

    // EVENTS

    #region Events

    public void HandleTakeDamage()
    {
        _animator.SetBool("isKnockedBack", true);
    }

   
    public void HandlePlayerDeath()
    {
        //_anim.SetTrigger("ImpactReceived");
        _animator.SetBool("isDead", true);
    }

    #endregion



}
