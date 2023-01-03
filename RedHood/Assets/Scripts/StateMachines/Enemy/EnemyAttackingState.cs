using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackingState : EnemyBaseState
{
    private readonly int AttackHash = Animator.StringToHash("Attack1Forward");

    private const float TransitionDuration = 0.1f;
    
    public EnemyAttackingState(EnemyStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        // FacePlayer(); //to make sure, enemy turns to face you when they attack, POSSIBILITY 1
        
        stateMachine.Weapon.SetAttack(stateMachine.AttackDamage, stateMachine.AttackKnockback);
        
        stateMachine.Animator.CrossFadeInFixedTime(AttackHash, TransitionDuration);
    }

    public override void Tick(float deltaTime)
    {
        if (GetNormalizedTime(stateMachine.Animator) >= 1)
        { 
            stateMachine.SwitchState(new EnemyChasingState(stateMachine)); 
        }
        FacePlayer(); //to make sure, enemy turns to face you when they attack, POSSIBILITY 2


    }
    public override void Exit()
    {
        
    }


    
}
