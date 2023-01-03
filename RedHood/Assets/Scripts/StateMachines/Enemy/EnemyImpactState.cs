using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyImpactState : EnemyBaseState
{

    private readonly int ImpactHash = Animator.StringToHash("GetHit1");

    private const float CrossFadeDuration = 0.1f;

    private float duration = 1f;

    public EnemyImpactState(EnemyStateMachine stateMachine) : base(stateMachine) { }


    //play Impact animation when entering the impact state
    public override void Enter() 
    {
        stateMachine.Animator.CrossFadeInFixedTime(ImpactHash, CrossFadeDuration);
    }

    //
    public override void Tick(float deltaTime)
    {
        
        Move(deltaTime); //move method for forces and gravity
        
        duration -= deltaTime; //timer:

        if (duration <= 0f) //twhen the duration runs out...
        {
            stateMachine.SwitchState(new EnemyIdleState(stateMachine)); // ... switch back to idle or locomotion state

        }
    }
    public override void Exit()
    {
        
    }

}
