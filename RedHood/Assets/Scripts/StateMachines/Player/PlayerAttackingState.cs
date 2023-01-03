using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackingState : PlayerBaseState
{
    private float previousFrameTime;
    
    private bool alreadyAppliedForce; //to make sure we don't endlessly reapply force
    
    private Attack attack;
    
    public PlayerAttackingState(PlayerStateMachine stateMachine, int attackIndex) : base(stateMachine)
    {
        // defines the current attack
        attack = stateMachine.Attacks[attackIndex];
    }

    public override void Enter()
    {
        stateMachine.Weapon.SetAttack(attack.Damage, attack.Knockback);// to be able to set weapon damage in inspector
        stateMachine.Animator.CrossFadeInFixedTime(attack.AnimationName, attack.TransitionDuration); // AnimationName and TransitionDuration are defined in the Attack class
        
    }
    public override void Tick(float deltaTime)
    {

        Move(deltaTime);

        FaceTarget();
        
        // to prevent accidentally receiving data from the final frame of the previous animation
        float normalizedTime = GetNormalizedTime(stateMachine.Animator);// pass in Animator of the player as argument of the method

        if(/*normalizedTime >= previousFrameTime && --> check not necessary*/ normalizedTime < 1f)//safety condition to make sure we have the right timing
        {
            
            // add force
            if (normalizedTime >= attack.ForceTime)
            {
                TryApplyForce();
            }

            //Attack!
            if(stateMachine.InputReader.IsAttacking)
            {
                TryComboAttack(normalizedTime);

            }
        }

        else
        {
            // go back to locomotion
            if(stateMachine.Targeter.CurrentTarget != null)// if there is a target being targeted...
            {
                stateMachine.SwitchState(new PlayerTargetingState(stateMachine));//go back to targeting state
            }
            else // if not ...
            {
                stateMachine.SwitchState(new PlayerFreeLookState(stateMachine));//go back to FreelookState

            }

        }

        previousFrameTime = normalizedTime;




    }


    public override void Exit()
    {
        
    }



    //-----------------------------METHODS---------------------------------------------------------

    
    //CALCULATE ATTACK COMBOS
    private void TryComboAttack(float normalizedTime) //normalized time is the time passed
    {
        // Do we actually have a combo attack?
        if (attack.ComboStateIndex == -1) { return; } // -1 signals: no combo available

        // Are we far enough through the animation to trigger it?
        if(normalizedTime < attack.ComboAttackTime) { return; }

        //if yes: switch state to the corresponding attack state
        stateMachine.SwitchState
        (
           new PlayerAttackingState
           (
            stateMachine,
            attack.ComboStateIndex

            )

        );

    }

    // CALCULATE FORCE BEHIND ATTACK
    private void TryApplyForce()
    {
        
        if(alreadyAppliedForce) { return; }

        stateMachine.ForceReceiver.AddForce(stateMachine.transform.forward * attack.Force);

        alreadyAppliedForce = true; // after applying force, make sure to prevent more force being added endlessly
    }
    
    
    

}
