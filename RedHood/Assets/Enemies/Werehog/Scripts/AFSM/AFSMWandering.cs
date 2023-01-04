using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AFSMWandering : StateMachineBehaviour
{
    public float pursueDistance = 3f;
    private AFSMWandering _wander;
    private AFSMChasing _pursue;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        /*
        AgentBehavior[] behaviors;
        GameObject gameObject = animator.gameObject;
        behaviors = gameObject.GetComponents<AgentBehavior>();
        foreach (AgentBehavior b in behaviors)
            b.enabled = false;

        _wander = gameObject.GetComponent<AFSMWandering>();
        _pursue = gameObject.GetComponent<AFSMChasing>();
        if (_wander == null || _pursue == null)
            return;
        _wander.enabled = true;
        animator.gameObject.name = "Wandering";
        */
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

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
