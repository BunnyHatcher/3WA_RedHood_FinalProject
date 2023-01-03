using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeadState : PlayerBaseState
{
    private readonly int DeadHash = Animator.StringToHash("Impact_Small");
    private const float CrossFadeDuration = 0.1f;

    public PlayerDeadState(PlayerStateMachine stateMachine) : base(stateMachine) { }
    

    public override void Enter()
    {
        //stateMachine.Animator.CrossFadeInFixedTime(DeadHash, CrossFadeDuration);
        stateMachine.Ragdoll.ToggleRagdoll(true); // activate player's ragdoll
        stateMachine.Weapon.gameObject.SetActive(false); // deactoivate player's weapon
    }
    public override void Tick(float deltaTime) { }
    

    public override void Exit() { }
    

}
