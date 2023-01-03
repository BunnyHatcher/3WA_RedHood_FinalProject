using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public abstract void Enter();

    public abstract void Tick(float deltaTime);

    public abstract void Exit();

    //CALCULATE THE CURRENT STATE OF THE STATE MACHINE
    
    //method to check at what state the animation is at the moment
    protected float GetNormalizedTime(Animator animator)
    {
        // get data for current state
        AnimatorStateInfo currentInfo = animator.GetCurrentAnimatorStateInfo(0);

        // get data for next state
        AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);

        // if we transition to an attack... 
        if (animator.IsInTransition(0) && nextInfo.IsTag("Attack"))
        {
            //... then give me the data for the next state
            return nextInfo.normalizedTime;
        }

        //if we are currently playing an attack animation, but we're not transitioning...
        else if (!animator.IsInTransition(0) && currentInfo.IsTag("Attack"))
        {
            //... then give me the data for the current state
            return currentInfo.normalizedTime;
        }

        else
        {
            return 0f;
        }
    }
}
