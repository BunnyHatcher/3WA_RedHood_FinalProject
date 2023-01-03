using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RH_JUMPING : RH_BaseState
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        //Debug.Log("Enter JUMPING");
        //_playerMovement._isJumping = true;
       // _playerMovement._movementDirection.y = _playerMovement._jumpForce;
        _playerMovement._isJumping = true;



    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        if (_rigidbody.velocity.y < -0.2f && !_playerMovement._isGrounded)
        {
            _playerMovement._isFalling = true;
            Debug.Log("Player Velocity below -0.2f");
        }
        


    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}





}
