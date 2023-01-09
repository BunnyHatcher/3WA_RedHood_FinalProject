using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspicionState : BaseState
{
    bool _playerIsNear = false;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _stateNote.text = "Suspicious";
        _navAgent.ResetPath();
        _timeSinceLastSawPlayer = 0f;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _timeSinceLastSawPlayer += Time.deltaTime;

        _playerIsNear = Vector3.Distance(_enemy.transform.position, _player.transform.position) < 5;

        if (_playerIsNear)
        {
            _FSM.SetBool("CHASING", true); 
        }

        else
        {
            if (_timeSinceLastSawPlayer > _suspicionTime)
            {
                _FSM.SetBool("PATROLLING", true);
            }
        }
        

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _FSM.SetBool("SUSPICIOUS", false);
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
