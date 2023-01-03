using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDeadState : EnemyBaseState
{
    private readonly int DeadHash = Animator.StringToHash("Impact_Small");
    private const float CrossFadeDuration = 0.1f;

    public EnemyDeadState(EnemyStateMachine stateMachine) : base(stateMachine) { }


    public override void Enter()
    {
        stateMachine.Animator.CrossFadeInFixedTime(DeadHash, CrossFadeDuration);
        stateMachine.Ragdoll.ToggleRagdoll(true); // toggle ragdoll
        stateMachine.Weapon.gameObject.SetActive(false); // make sure enemies' weapon is deactivated when they die
        GameObject.Destroy(stateMachine.Target); // to prevent from being able to target an enemy after they are dead
    }
    public override void Tick(float deltaTime) { }


    public override void Exit() { }
}
