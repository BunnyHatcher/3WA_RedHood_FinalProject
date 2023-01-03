using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState // IdleState inherits all the methods it needs from BaseSTate
{

    private readonly int EnemyLocomotionHash = Animator.StringToHash("Locomotion");

    private readonly int EnemySpeedHash = Animator.StringToHash("Speed");

    private const float CrossFadeDuration = 0.1f;

    private const float AnimatorDampTime = 0.1f;

    //Constructor
    public EnemyIdleState(EnemyStateMachine stateMachine) : base(stateMachine) { }


    public override void Enter()
    {
        //Play BlendTree Animation via Hash
        stateMachine.Animator.CrossFadeInFixedTime(EnemyLocomotionHash, CrossFadeDuration);
    }


    public override void Tick(float deltaTime)
    {
        Move(deltaTime);

        if (IsInChaseRange())
        {
            // Transition to chasing state
            stateMachine.SwitchState(new EnemyChasingState(stateMachine));
            return;
        }

        FacePlayer();
        
        stateMachine.Animator.SetFloat(EnemySpeedHash, 0f, AnimatorDampTime, deltaTime);
        
    }
    
    public override void Exit()
    {
        
    }

}
