using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RH_ATTACKING : RH_BaseState
{
    private float timePassed;
    private float clipLength;
    private float clipSpeed;
    private float _previousFrameTime;


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        //_playerMovement._isAttacking = true;
  
        timePassed = 0f;

        //Set WeaponDrawn bool to true if not yet true
        if (_playerMovement._weaponDrawn == false)
        {
            _playerMovement._weaponDrawn = true;
        }

        //Debug.Log("Enter ATTACKING");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        if (animator.GetCurrentAnimatorStateInfo(2).normalizedTime > 1)
        {
           //Debug.Log("Combo Ready");
        }





        /*
        float normalizedTime = GetNormalizedTime();
        if (normalizedTime > _previousFrameTime && normalizedTime < 1f) // if we are still inside of the state
        {
            TryComboAttack(normalizedTime);
        }
        else
        {
            //go back to locomotion
        }
        _previousFrameTime = normalizedTime;
        */



    }


    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _playerMovement._isAttacking = false;
        _playerMovement._weaponDrawn = false;
    }


    #region Methods

    //Method to find out what state we are in when transitioning between states
    private float GetNormalizedTime()
    {
        AnimatorStateInfo currentInfo = _animator.GetCurrentAnimatorStateInfo(2);
        AnimatorStateInfo nextInfo = _animator.GetNextAnimatorStateInfo(2);

        if (_animator.IsInTransition(0) && nextInfo.IsTag("Attack")) //If we are in an attack transition...
        {
            return nextInfo.normalizedTime; //... tell us how far we are through with it
        }
        else if (!_animator.IsInTransition(0) && currentInfo.IsTag("Attack")) //If we are in an attack & not in transition...
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
        //if (_playerMovement._comboStateIndex == -1) { return; }

        if (normalizedTime < _playerMovement._comboAttackTime) { return; }

        _animator.SetBool("CanSwitchCombo", true);


    }
    #endregion



}
