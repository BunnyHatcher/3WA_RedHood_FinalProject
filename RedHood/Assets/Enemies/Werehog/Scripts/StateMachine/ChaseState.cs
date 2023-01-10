using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : BaseState
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        _stateNote.text = "Chasing";
        _animator.SetBool("isMoving", true);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       // Safety Check to make sure, that Actions interrupt locomotion
        if (_isPerformingAction)
            return;
        
        // AI chases player's position
        _navAgent.SetDestination(_player.transform.position);

        _distanceFromTarget = Vector3.Distance(_enemy.transform.position, _player.transform.position);
        if (_distanceFromTarget > _endChaseDistance)
        {
            _FSM.SetBool("SUSPICIOUS", true);
        }

        if (_distanceFromTarget <= _navAgent.stoppingDistance /*_attackRange*/)
        {
            _FSM.SetBool("ATTACKING", true);
        }

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _FSM.SetBool("CHASING", false);
        _animator.SetBool("isMoving", false);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
